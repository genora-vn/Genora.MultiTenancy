# Hoa Linh 25 Năm (hl25) + HLG — S3 Storage Migration (ods.vn) — 2026-10-09

> Nhánh: `feature/dev-hoalinh-25year-s3-storage` (đã tạo, **CHƯA merge/deploy** — anh ưu tiên task khác trước).
> Baseline trước nhánh này: `dev` HEAD `6156986` (merge `feature/dev-hoalinh-gamification`).

## Bối cảnh / vấn đề gốc
- Module hl25 (và HLG) đang lưu ảnh thiệp/avatar/kho quà trực tiếp lên `wwwroot/uploads/{hl25|hlg}/{tenantId}/...` trên Windows Server. Dung lượng tích lũy (user đã tự chuyển hơn 23GB ảnh cũ sang S3) gây cạn kiệt ổ đĩa server.
- Giải pháp: dùng S3-compatible storage `https://s3.ods.vn` (bucket `c01-hoalinh-prod-01`, user cấp sẵn AccessKey/SecretKey, ACL bucket-level "Everyone: ReadOnly" cho object — không cần set ACL per-object).
- Yêu cầu: cấu trúc object key trên S3 phải khớp đúng cấu trúc path cũ trên Windows Server (`uploads/{subFolder}/{tenantId}/{file}`) để không phải đổi gì ở tầng đọc/hiển thị FE; cơ chế phải tái sử dụng được cho cả hl25 và HLG (và module khác sau này nếu cần).

## Approach đã chốt (Plan mode → user confirm → Build mode)
1. **Abstraction `IImageStorageProvider`** với 2 implementation chọn theo config:
   - `LocalImageStorageProvider` — giữ nguyên hành vi `wwwroot` hiện tại.
   - `S3ImageStorageProvider` — dùng `AWSSDK.S3` trực tiếp (không dùng ABP BlobStoring vì cần kiểm soát public URL + key structure chính xác).
2. **`ManageImageService` là facade public** (API cũ `UploadImageAsync`/`DeleteFileAsync` giữ nguyên, callers không đổi) — tự chọn Local/S3 theo `Storage:Provider` + whitelist `Storage:S3:SubFolders` (**match theo tiền tố segment đầu** của subFolder, không phải exact match — quan trọng vì các path lồng nhau như `hlg/videos`, `hlg/ranking-share` đều phải thuộc nhóm `hlg`).
3. Mở rộng interface + 3 method mới để cover toàn bộ 4 đường "bypass" (ghi file trực tiếp, không qua `ManageImageService` cũ):
   - `UploadBytesAsync` (bytes thô, không re-encode — dùng cho ảnh chia sẻ ranking HLG + video admin).
   - `CopyAsync` (dùng cho đổi tên ảnh thiệp hl25 theo ZaloUserId+timestamp — trước đây `File.Copy` trực tiếp).
   - `OpenReadAsync` (dùng cho ZIP tải toàn bộ ảnh thiệp admin — trước đây `File.OpenRead` trực tiếp).
4. **Chỉ hl25 + hlg dùng S3** (whitelist `SubFolders: ["hl25","hlg"]`); module khác (salon/news/pro/caddie/documents...) tiếp tục dùng Local, không bị ảnh hưởng gì.
5. **URL tuyệt đối S3 lưu trực tiếp vào DB** cho ảnh mới — vì `ImageHelper.NormalizeThumb`/các `ToFullUrl` cục bộ (hl25, HLG ranking) đều đã idempotent với URL `http(s)` tuyệt đối (guard có sẵn), nên không cần sửa code đọc/hiển thị ở bất kỳ đâu khác.
6. **Ảnh cũ giữ nguyên trên Local** (mixed state, không backfill) — user đã tự chuyển 23GB ảnh cũ qua S3 bằng tay rồi, không cần migrate thêm.

## 4 đường bypass đã refactor để dùng chung `IManageImageService`
1. `MiniAppHl25Service.RenameImageWithTimestampAsync` — `File.Copy` → `CopyAsync`.
2. `Hl25FrameCreationAppService.DownloadAllImagesAsync` (ZIP toàn bộ ảnh thiệp, admin) — `File.OpenRead` → `OpenReadAsync` (bỏ `IHostEnvironment`, không cần nữa).
3. `HlgRankingAppService.SaveShareImageAsync` — `File.WriteAllBytesAsync` → `UploadBytesAsync`.
4. `Hlg/Upload.cshtml.cs` (`OnPostVideoAsync`, admin HLG video) — ghi `FileStream` trực tiếp → `UploadBytesAsync` (bỏ `IWebHostEnvironment`, không cần nữa).

## Bug tìm & sửa trong lúc viết test (đúng nguyên tắc tìm root cause, không patch tạm)
- **Bug 1 (double-prefix path):** `HlgRankingShareImageConsts.PublicSubPath = "uploads/hlg/ranking-share"` đã có sẵn tiền tố `"uploads/"`, nhưng `ManageImageService.UploadBytesAsync` TỰ thêm `"uploads/"` khi dựng key → thành `uploads/uploads/hlg/ranking-share/...` (sai) + không match whitelist. **Fix:** thêm constant mới `HlgRankingShareImageConsts.ImageSubFolder = "hlg/ranking-share"` (KHÔNG có tiền tố `uploads/`) dùng riêng cho lời gọi `UploadBytesAsync`, giữ `PublicSubPath` cũ cho các chỗ khác không đổi.
- **Bug 2 (whitelist exact-match sai):** `ResolveProvider` ban đầu so khớp **tuyệt đối** `subFolder` với whitelist entries → `"hlg/ranking-share"` không khớp `"hlg"` → luôn rơi về Local dù `Provider=S3`. **Fix:** đổi sang so khớp theo **segment đầu tiên** (`subFolder.Split('/')[0]`), nên `"hlg/videos"`, `"hlg/ranking-share"` đều đúng thuộc nhóm `"hlg"`.
- **Bug 3 (test fixture, không phải code chính):** `HlgRankingShareImageTests.Build()` dùng `Substitute.For<IManageImageService>()` (mock trống) sau khi thêm tham số constructor mới — NSubstitute mặc định trả `null` cho `Task<string>` KHÔNG throw, nên test `Accepts_Valid_Png_And_Returns_Public_Uploads_Url` nhận `Url=null` thay vì lỗi rõ ràng. **Fix:** dựng `ManageImageService` THẬT (Provider=Local → `LocalImageStorageProvider` thật ghi file lên `wwwroot` tạm; `S3ImageStorageProvider` thật nhưng `IAmazonS3` mock ở biên mạng, không bị gọi khi Provider=Local) — test giờ xác minh đúng hành vi ghi file thật.

## Cấu hình (`Storage` section mới, default an toàn)
- `appsettings.json` (**commit**): `Storage:Provider = "Local"`, `Storage:S3.*` toàn placeholder rỗng — **không có secret nào bị commit**, không đổi hành vi hiện tại tới khi ai đó chủ động đổi `Provider`.
- `appsettings.Staging.json` (**đã tạo tại `src/Genora.MultiTenancy.Web/`, đã thêm vào `.gitignore` cùng `appsettings.Production.json` — KHÔNG commit, KHÔNG nằm trong git**): đã điền sẵn credentials thật user cung cấp:
  - `ServiceUrl: https://s3.ods.vn`, `Bucket: c01-hoalinh-prod-01`
  - `AccessKey: QPJD3ZZ2YMP3EBCSAESJ`, `SecretKey: 7l5x4EePeGeQDCLRaD5bE47LmGJUmp57xWuxv5wQ` (user: "hoalinh", role owner, quota 1TiB)
  - `PublicBaseUrl: https://s3.ods.vn/c01-hoalinh-prod-01`, `CannedAcl: ""` (rỗng — bucket đã có policy "Everyone: ReadOnly" cho object, không set ACL per-object), `SubFolders: ["hl25","hlg"]`.
  - **File này KHÔNG trong git — khi deploy, user phải tự copy file này lên server qua kênh an toàn (SCP/RDP), KHÔNG qua git**, đặt cạnh `appsettings.json`, set `ASPNETCORE_ENVIRONMENT=Staging`, restart host.

## File chính đã tạo/sửa
- (mới) `Application.Contracts/AppDtos/AppImages/IImageStorageProvider.cs`, `StorageOptions.cs`.
- (mới) `Application/AppServices/AppImages/LocalImageStorageProvider.cs`, `S3ImageStorageProvider.cs`.
- `Application/AppServices/AppImages/ManageImageService.cs` — viết lại thành facade (interface `IManageImageService` mở rộng 3 method mới).
- `Application/MultiTenancyApplicationModule.cs` — đăng ký `IAmazonS3` singleton (`BasicAWSCredentials` + `AmazonS3Config{ServiceURL,ForcePathStyle,AuthenticationRegion}`), bind `StorageOptions`, register 2 provider.
- `Application/AppServices/Hl25/MiniAppHl25Service.cs` — `RenameImageWithTimestampAsync` dùng `CopyAsync`.
- `Application/AppServices/Hl25/Hl25FrameCreationAppService.cs` — `DownloadAllImagesAsync` dùng `OpenReadAsync`, bỏ `IHostEnvironment`.
- `Application/AppServices/Hlg/HlgRankingAppService.cs` — `SaveShareImageAsync` dùng `UploadBytesAsync` (thêm `IManageImageService` vào constructor).
- `Domain.Shared/Hlg/HlgRankingShareImageConsts.cs` — thêm `ImageSubFolder` constant.
- `Web/Pages/Hlg/Upload.cshtml.cs` — `OnPostVideoAsync` dùng `UploadBytesAsync`, bỏ `IWebHostEnvironment`.
- `Web/appsettings.json` — thêm section `Storage` (Local default, placeholder S3 rỗng).
- (mới, KHÔNG commit) `Web/appsettings.Staging.json` — credentials thật.
- `.gitignore` — thêm 2 dòng loại trừ `appsettings.Staging.json`/`appsettings.Production.json`.
- `Application.Contracts/Genora.MultiTenancy.Application.Contracts.csproj` — có thể cần kiểm tra lại package reference AWSSDK.S3 nằm ở Application layer (không phải Contracts).
- (mới) `test/.../AppImages/ManageImageServiceStorageSelectionTests.cs` — 12 test mới (provider selection theo whitelist, URL build S3/Local, idempotency, copy/read mixed-state, round-trip upload+read).
- `test/.../Hlg/HlgRankingShareImageTests.cs` — sửa `Build()` dùng `ManageImageService` thật (không mock trống) để test hành vi ghi file thật đúng.

## Verify
- `dotnet add package AWSSDK.S3` → cài `AWSSDK.S3 4.0.104.2` + `AWSSDK.Core 4.0.102.9` (package mới, chưa có trong repo trước đây).
- Build toàn bộ solution (`dotnet build` không filter): **0 lỗi CS** (NU1902/NU1903 warning về `SixLabors.ImageSharp` 3.1.12 là pre-existing, không liên quan).
- Test: `dotnet test` toàn bộ **218/218 PASS** (12 test mới `AppImages` + toàn bộ test cũ hl25/hlg không bị regressions sau 2 vòng fix bug ở trên).
- **CHƯA chạy thật trên staging** (chưa có cơ hội deploy — user báo sẽ ưu tiên task khác trước khi quay lại nhánh này).

## Việc còn lại khi quay lại task này
1. Merge/rebase nhánh `feature/dev-hoalinh-25year-s3-storage` vào `dev` khi sẵn sàng triển khai (hiện KHÔNG merge).
2. Deploy code lên staging + **tự tay copy `appsettings.Staging.json` lên server** (không qua git) + set `ASPNETCORE_ENVIRONMENT=Staging` + restart host.
3. Test thật trên staging: upload ảnh thiệp hl25 mới (xem URL trả về đúng dạng `https://s3.ods.vn/c01-hoalinh-prod-01/uploads/hl25/{tenantId}/{file}`), xem lại ảnh trên FE mini app, test ZIP "Tải ảnh ZIP" (đọc được cả ảnh cũ Local lẫn ảnh mới S3), test đổi tên ảnh thiệp (CreateFrame với `RenameWithTimestamp=true`), test HLG share-image ranking, test video admin HLG.
4. Nếu ổn, cân nhắc backfill ảnh cũ còn lại trên Local (nếu còn, user nói đã tự chuyển 23GB rồi nên có thể không cần) + dọn file cũ trên Windows Server để giải phóng disk — đây là mục tiêu gốc của task.
5. Theo dõi chi phí/quota S3 (user quota 1TiB) khi lưu lượng tăng.
