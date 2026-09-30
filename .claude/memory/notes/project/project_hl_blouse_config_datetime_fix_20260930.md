# Hoa Linh Sales — BlouseConfig lưu ngày giờ và cấu hình — 2026-09-30

## Phạm vi và nguyên nhân

- User báo `/HoaLinh/BlouseConfig`: đổi Bắt đầu/Kết thúc rồi Lưu thì hiện lại ngày giờ mặc định.
- Reproduce bằng parser **flatpickr 4.6.13**: `parseDate('2026-10-30T23:55:00', 'd/m/Y H:i')` cho `20/06/2026 00:00`. API trả ISO không có offset, nhưng `loadCampaign` truyền nguyên chuỗi vào `setDate` dùng display format Việt Nam. Tương tự `2026-09-30T08:35:00` bị đọc thành cùng ngày 20/06. Đây là lỗi đọc/render đã chứng minh bằng thư viện, không phải bằng chứng database tự reset.
- Code trước task đã gửi local ISO không Z bằng `toNaiveIsoString`; giữ convention này vì entity SQL DateTime không offset và Mini App kiểm tra bằng giờ local. Không đổi global clock/timezone.
- Phát hiện thêm read/save chọn campaign khác nhau: read ưu tiên active rồi CreationTime, save chỉ lấy CreationTime mới nhất. Có thể ghi nhầm bản inactive khi tồn tại nhiều bản ghi. Không truy vấn DB để khẳng định tenant thật có trường hợp này.

## Thay đổi

- `Web/Pages/HoaLinh/BlouseConfig/index.js`: parse ISO bằng `new Date` trước `picker.setDate`; render trực tiếp DTO trả về sau save; đọc giá trị đang hiển thị khi submit để nhập tay/xoá không dùng lại `selectedDates` cũ. Kiểm tra ngày/tháng/giờ và thứ tự Start/End. Giữ local ISO, độ chính xác phút; fallback input text nếu flatpickr CDN không tải được. Picker dùng minuteIncrement=1, disableMobile để giữ cùng định dạng.
- Form chỉ cho lưu sau khi tải thành công; chặn double submit và lưu trong lúc upload banner; giữ dữ liệu nhập khi API save lỗi. Dùng `Number` sau native validation để không cắt giá trị số kiểu `1e3` thành 1.
- `Index.cshtml`: submit form (hỗ trợ Enter), required/maxLength/min/max/step cho các trường tương ứng DTO; nút save disabled ban đầu.
- `HlBlouseAdminAppService`: read/save dùng cùng helper chọn campaign theo IsActive, CreationTime, Id, vẫn giữ repository tenant filter; validate End >= Start trước khi mutate; trim ProgramName.
- `HlBlouseCampaignSaveDto`: Required/StringLength/Range khớp giới hạn entity, tránh lỗi SQL và số âm khi gọi API trực tiếp.
- Không đổi entity, migration, route hay tên/trường API; không tác động gateway, TenantAutoMigrateMiddleware hoặc logic đăng ký Sale. Không sửa dữ liệu/lịch cũ trong DB.

## Kiểm chứng

- `dotnet build src/Genora.MultiTenancy.Web/Genora.MultiTenancy.Web.csproj --no-restore -v:q -clp:ErrorsOnly`: **0 errors**, 56 warnings sẵn có. Sandbox bị chặn ghi một số file bin; chạy ngoài sandbox với approval đã thành công.
- **35 HoaLinhSales tests pass**: 13 case mới `HlBlouseCampaignTests`, 8 validator tests, 14 export tests. Test mới dùng repository substitute: create/update/read toàn bộ field, clear dates/content, zero/false, multi-row selector, invalid dates/DTO, host/tenant permission. Không phải integration test database thật.
- Lệnh test Application bình thường bị chặn bởi **lỗi có sẵn CS7036** ở `Hlg/HlgRankingShareImageTests.cs:84`: constructor `HlgRankingAppService` thiếu IConfiguration. Không sửa file HLG. Đã chạy HoaLinhSales với MSBuild target tạm trong ignored `artifacts/blouse-config-validation/ExcludeUnrelatedBrokenTest.targets`, remove riêng file lỗi khỏi Compile trước CoreCompile. Lệnh dùng `--filter FullyQualifiedName~HoaLinhSales -p:CustomAfterMicrosoftCommonTargets=<absolute targets path>`; không tuyên bố toàn bộ Application suite đã pass.
- **27 Node tests pass** tại timezone Asia/Bangkok: `node --test test/hl-blouse-config-ui-regressions.cjs test/hl-sales-ui-regressions.cjs` (15 Blouse +12 Sale hiện có). 15 Blouse tests cũng pass tại TZ=UTC. `node --check` pass.
- Node harness chạy production `index.js` với mock DOM/proxy/picker, kiểm tra API payload, render/reload, typed/calendar/cleared dates, repeated saves, all fields, false/zero, failed load/save, pending/double submit và no-picker fallback. Parser root-cause tái hiện riêng bằng thư viện flatpickr thật tải vào ignored artifacts.
- Browser skill đã đọc, runtime discovery trả `[]`: **chưa UAT browser đăng nhập hoặc database tenant thật**. Không deploy trong task này.

## UAT khi cập nhật Web

### Follow-up: sửa lỗi build HLG constructor (2026-09-30)

- Theo yêu cầu tiếp theo của user, sửa `test/Genora.MultiTenancy.Application.Tests/Hlg/HlgRankingShareImageTests.cs`: thêm namespace Configuration và truyền `new ConfigurationBuilder().Build()` vào tham số IConfiguration của `HlgRankingAppService`. Các call site còn lại đã đúng. Không sửa service production.
- `dotnet build Genora.MultiTenancy.sln --no-restore -v:q -clp:ErrorsOnly`: **PASS, 0 errors / 2 warnings**, chạy ngoài sandbox vì sandbox chặn ghi một số file bin Web.
- `dotnet test test/Genora.MultiTenancy.Application.Tests/Genora.MultiTenancy.Application.Tests.csproj --no-build --no-restore --filter 'FullyQualifiedName~HlgRankingShareImageTests|FullyQualifiedName~HlgDesignContentTests|FullyQualifiedName~HoaLinhSales' -v:minimal`: **82/82 pass**, không loại test và không dùng workaround CustomAfterMicrosoftCommonTargets nữa. Workaround ở phần kiểm chứng ban đầu chỉ là lịch sử.

### Các bước

1. Publish Web gồm DLL Application/Contracts + Razor và `Pages/HoaLinh/BlouseConfig/index.js`; Ctrl+F5 trang sau cập nhật.
2. Chọn Bắt đầu `30/09/2026 08:37`, Kết thúc `30/10/2026 23:59`; lưu, F5, lưu thêm lần nữa: ngày giờ giữ nguyên.
3. Đổi bằng bàn phím; xoá ngày thành không giới hạn; kiểm tra mốc kết thúc trước bắt đầu bị từ chối.
4. Đổi tên, giới thiệu, số áo/điểm/giới hạn (gồm 0), banner URL/upload và trạng thái bật/tắt; lưu rồi F5 đối chiếu.
5. Dữ liệu ngày đã bị lưu sai trước bản sửa cần admin chọn lại mốc mong muốn; không tự suy đoán/ghi đè lịch chiến dịch.

## Workspace

- Giữ nguyên thay đổi có sẵn trong Web/DbMigrator appsettings, log bị xoá/log mới và uploads/hl-blouse của user.
- Task trước về POST `/api/mini-app/hl/auth` 500 vẫn chỉ ở bước điều tra/đề xuất, không được ngầm xem là đã sửa qua task BlouseConfig.
