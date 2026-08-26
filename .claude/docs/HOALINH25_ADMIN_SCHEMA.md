# HỆ THỐNG QUẢN TRỊ (ADMIN) — ZALO MINI APP "DƯỢC PHẨM HOA LINH 25 NĂM"

> Tài liệu thiết kế kiến trúc & CSDL. Nhánh: `feature/hoalinh-25years`. Schema DB: **`hl25`**.
> Nền tảng: ABP Framework (.NET 9, DDD), Multi-Tenancy.
> Trạng thái: **THIẾT KẾ — CHỜ XÁC NHẬN** (chưa viết code production).
> Cập nhật: 2026-08-25.

---

## 1. TỔNG QUAN & PHẠM VI

### 1.1 Bối cảnh
Mini App Zalo "Dược phẩm Hoa Linh 25 năm" là chương trình kỷ niệm 25 năm thành lập (19/09/2026 – 30/10/2026, phạm vi toàn quốc). Người dùng: (1) tạo thiệp chúc mừng (ghép ảnh + lời chúc), (2) chia sẻ Zalo/Facebook để nhận lượt quay, (3) chơi vòng quay may mắn nhận quà. Hệ thống Admin quản trị toàn bộ cấu hình, dữ liệu và báo cáo của chương trình.

### 1.2 Luồng người dùng (rút từ Figma — file `ffnEUAhIsN0V7CpM3kCFLu`, node `298:7238`)
1. **Trang chủ** — banner "Thử thách sáng tạo thiệp chúc mừng tuổi 25" + nút "Thể lệ chương trình".
2. **Thể lệ chương trình** — nội dung HTML (ý nghĩa, thời gian, phạm vi, hình thức, ĐVTC) + nút "Xác nhận đồng ý tham gia".
3. **Lấy quyền / Follow OA** — yêu cầu follow Zalo OA + cấp quyền lấy thông tin.
4. **Tạo thiệp** — upload ảnh (JPG/PNG, max 5MB) + nhập lời chúc (≤ 500 ký tự) → **Xem trước thiệp**.
5. **Upload** — tiến trình tải lên → **Upload thành công / thất bại**.
6. **Chia sẻ** — "Hoa Linh tặng bạn 01 lượt quay may mắn"; chia sẻ Zalo/Facebook để +01 lượt (tối đa 02 lượt) + Tải xuống + Quay ngay.
7. **Vòng quay may mắn** — hiển thị điểm số, số lượt còn lại, lịch sử trúng thưởng; ô "Thêm 1 lượt".
8. **Kết quả trúng quà** — hiển thị quà tặng (VD "Combo 04 sản phẩm…").
9. **Tài khoản** — điểm số, số lượt quay, số quà, **Lịch sử nhận quà** (tên quà, ngày, trạng thái "Đã nhận"), link chia sẻ, nút Tải xuống/Chia sẻ/Chỉnh sửa thông tin.
10. **Chỉnh sửa thông tin cá nhân** — họ tên, ngày sinh, giới tính, SĐT, địa chỉ nhận quà.

> Ghi chú phạm vi: Figma còn hiển thị tab "Kiến thức / Trò chơi / Lịch họp / Chat" và mini-game câu hỏi (nguồn sinh "điểm"). **Các phần này KHÔNG thuộc 5 nhóm tính năng Admin lần này** (có thể trùng module gamification đang parked ở nhánh `feature/hoalinh-gamification`). Ở tài liệu này, "điểm số" chỉ được lưu như thuộc tính người dùng + (tùy chọn) sổ giao dịch điểm; cơ chế sinh điểm từ game nằm ngoài phạm vi.

### 1.3 Năm nhóm tính năng Admin
| # | Nhóm | Thực thể/dữ liệu chính | Ghi chú |
|---|------|------------------------|---------|
| 1 | Cài đặt Mini App | `Hl25AppConfig` + tái dùng Zalo OA/ZNS | HTML editor (Summernote) cho Thể lệ/Luật chơi/TVC; Logo/Banner/TVC |
| 2 | Quản lý Frame | `Hl25FrameCampaign`, `Hl25FrameTemplate`, `Hl25FrameCreation` | Chiến dịch + mẫu frame + lịch sử tạo ảnh |
| 3 | Vòng quay may mắn | `Hl25WheelConfig`, `Hl25WheelSlot`, `Hl25Gift`, `Hl25SpinTurnLog`, `Hl25SpinLog` | Cấu hình + kho quà + lịch sử nhận lượt + lịch sử quay |
| 4 | Quản lý Người dùng | `Hl25Participant` | Người chơi + follow OA + consent + địa chỉ nhận quà |
| 5 | Báo cáo Thống kê | (không entity mới) | Query tổng hợp từ nhóm 2/3/4 |

---

## 2. CONVENTION KIẾN TRÚC (bám sát repo)

### 2.1 Quy ước chung
- **Schema:** mọi entity đặt `[Table("AppHl25<Name>", Schema = "hl25")]`. Prefix bảng: `AppHl25*`.
- **Base class:** aggregate → `FullAuditedAggregateRoot<Guid>, IMultiTenant`; entity log/dòng con thuần → `CreationAuditedEntity<Guid>` (theo mẫu `HlApiLog`).
- **TenantId:** `public Guid? TenantId { get; set; }` (implement `IMultiTenant`).
- **Async:** mọi method AppService trả `Task` (quy tắc CLAUDE.md).
- **AsyncExecuter:** Count/ToList/FirstOrDefault qua `IAsyncQueryableExecuter`, KHÔNG dùng EF Core trực tiếp ở Application layer (RULES.md).
- **WithDetailsAsync:** dùng khi cần eager-load navigation/child collection.
- **Decimal:** `HasColumnType("decimal(18,2)")`; **enum:** `HasConversion<byte>()`.

### 2.2 Layer & vị trí file (theo code map)
| Layer | Đường dẫn dự kiến |
|-------|-------------------|
| Entities | `src/…Domain/DomainModels/AppHl25/` |
| Enums/Consts | `src/…Domain.Shared/Hl25/` |
| DTO + IAppService | `src/…Application.Contracts/AppDtos/Hl25/` |
| Feature | `src/…Application.Contracts/Features/AppHl25Features/` |
| Permission | bổ sung vào `Permissions/MultiTenancyPermissions.cs` + `MultiTenancyPermissionDefinitionProvider.cs` |
| AppService | `src/…Application/AppServices/Hl25/` |
| AutoMapper | bổ sung `MultiTenancyApplicationAutoMapperProfile.cs` |
| DbContext ext | `src/…EntityFrameworkCore/EntityFrameworkCore/MultiTenancyDbContextModelCreatingExtensionsHl25.cs` |
| Controller (MiniApp) | `src/…HttpApi/Controllers/HoaLinh25MiniAppController.cs` |
| Web Pages | `src/…Web/Pages/Hl25/` |
| Menu | bổ sung block trong `Web/Menus/MultiTenancyMenuContributor.cs` |

### 2.3 Feature (bật/tắt theo tenant)
- `AppHl25Features { GroupName = "Hl25"; Management = "Hl25.Management"; }`
- `AppHl25FeatureDefinitionProvider`: `AddGroup("Hl25")` → `AddFeature("Hl25.Management", defaultValue:"false", ToggleStringValueType)`.

### 2.4 Permission (dual Tenant/Host — theo mẫu Zalo)
- **Nhóm Tenant** `MiniAppHl25` → set `MultiTenancySides.Tenant` + `.RequireFeatures(AppHl25Features.Management)` trên root + MỌI child (RULES.md: thiếu 1 level là leak).
- **Nhóm Host** `MiniAppHl25Host` → set `MultiTenancySides.Host`, KHÔNG gọi `RequireFeatures`.
- Hằng số trong `MultiTenancyPermissions.AppHl25` (tenant) / `HostAppHl25` (host), mỗi nhóm chức năng có `Default/Create/Edit/Delete`.
- Menu check kép: `feature.IsEnabledAsync(Hl25.Management) && perms.IsGrantedAsync(...)` + `.RequirePermissions(...)`.

> Lưu ý (đã kiểm chứng codebase): repo KHÔNG dùng helper `P()`; dual permission thực hiện bằng cặp group Tenant/Host + property `MultiTenancySide`. Feature thực tế của HL cũ là `HoaLinh.Management` (không phải `AllowHoaLinhModule`).

---

## 3. BẢN ĐỒ TÁI SỬ DỤNG (Bước 3)

| Nhu cầu | Tái dùng cái gì | Vị trí |
|---------|-----------------|--------|
| HTML Editor (Thể lệ/Luật chơi/TVC) | **Summernote 0.8.18** (CDN, thủ công) + hàm `initNewsEditor` | `Web/Pages/AppGolfCourses/index.js:9-38`; CSS/JS: `AppGolfCourses/Index.cshtml:19,25` |
| Upload ảnh (Logo/Banner/mẫu frame/ảnh thiệp) | `IManageImageService.UploadImageAsync(file, tenantId, subFolder:"hl25")` | `Application.Contracts/AppDtos/AppImages/IManageImageService.cs:9`; impl `AppServices/AppImages/ManageImageService.cs:15` |
| Giới hạn 5MB ảnh thiệp | **PHẢI tự thêm** (ManageImageService hiện KHÔNG chặn size) | validate ở AppService/DTO trước khi gọi upload |
| Zalo OA Auth (token/OA ID) | Entity `ZaloAuth` (`AppZaloAuth`) + `ZaloSettingNames` | `Domain/DomainModels/AppZaloAuth/ZaloAuth.cs:10`; `AppServices/AppZaloAuths/ZaloSettingNames.cs` |
| Gửi ZNS/ZBS | `IZaloZbsClient` + `IZaloZbsTemplateResolver` + toggle | DI `MultiTenancyApplicationModule.cs:90-97` |
| Nhật ký Zalo | `IZaloLogWriter` → bảng `AppZaloLog` (`ZaloLog`) | `Domain/DomainModels/AppZaloAuth/ZaloLog.cs:10` |
| Cấu trúc module mẫu | Module Hoa Linh DMS (schema `HL`) | `DomainModels/AppHlOrders/`, `AppServices/HoaLinh/`, `MultiTenancyDbContextModelCreatingExtensionsHoaLinh.cs` |

> Nguyên tắc: **cấu hình Zalo OA/ZNS tái dùng module có sẵn** (không tạo entity Zalo mới cho hl25). Admin hl25 chỉ đọc/ghi cấu hình chung qua `Hl25AppConfig` và trỏ tới cấu hình Zalo dùng chung.

---

## 4. ENUMS & CONSTANTS (đặt tại `Domain.Shared/Hl25/`)

```csharp
// Trạng thái quà trong kho
public enum Hl25GiftStatus : byte { Available = 0, OutOfStock = 1, Disabled = 2 }

// Nguồn cộng lượt quay
// LƯU Ý NGHIỆP VỤ (đã chốt): 1 lượt quay chỉ được cộng khi HOÀN TẤT 1 chu kỳ "Tạo thiệp + Chia sẻ".
// Tạo thiệp đơn thuần KHÔNG cộng lượt; phải chia sẻ thành công mới +1. Source = nền tảng chia sẻ của chu kỳ đó.
public enum Hl25SpinTurnSource : byte { ShareZalo = 1, ShareFacebook = 2, AdminGrant = 3, Other = 4 }

// Trạng thái trao thưởng (lượt quay)
public enum Hl25RewardStatus : byte { Pending = 0, Won = 1, NotWon = 2, Delivered = 3, Cancelled = 4 }

// Trạng thái chiến dịch Frame
public enum Hl25CampaignStatus : byte { Draft = 0, Active = 1, Paused = 2, Ended = 3 }

// Nền tảng chia sẻ (lịch sử tạo thiệp)
public enum Hl25SharePlatform : byte { None = 0, Zalo = 1, Facebook = 2 }

// Giới tính người tham gia
public enum Hl25Gender : byte { Unknown = 0, Male = 1, Female = 2, Other = 3 }
```

Constants: `Hl25Consts`:
- `MaxCardImageSizeBytes = 5 * 1024 * 1024` (giới hạn 5MB ảnh thiệp — tự validate).
- `MaxWishLength = 500` (lời chúc).
- `DefaultImageSubFolder = "hl25"`.
- **`MaxSpinTurnsPerUser = 2`** (đã chốt): trần lượt quay/người.
- **Quy tắc cộng lượt (đã chốt):** mỗi chu kỳ **"Tạo thiệp → Chia sẻ thành công" = +1 lượt**. Người dùng được làm tối đa **2 chu kỳ** → tối đa **2 lượt**. Tạo thiệp mà chưa chia sẻ KHÔNG cộng lượt.

---

## 5. THIẾT KẾ ENTITIES (schema `hl25`)

> Quy ước cột kiểm toán chung (ABP tự thêm, KHÔNG liệt kê lại): `Id (PK)`, `TenantId`, `CreationTime`, `CreatorId`, `LastModificationTime`, `LastModifierId`, `IsDeleted`, `DeleterId`, `DeletionTime`. Aggregate = `FullAuditedAggregateRoot<Guid>`.

### 5.1 Nhóm 1 — Cài đặt Mini App

#### `Hl25AppConfig` — `[Table("AppHl25AppConfig", Schema="hl25")]`
Cấu hình chung của chương trình. 1 bản ghi / tenant (singleton theo tenant).

| Field | Kiểu | Null | Mô tả |
|-------|------|------|-------|
| `ProgramName` | `string(256)` | ✗ | Tên chương trình |
| `LogoUrl` | `string(1024)` | ✓ | Logo (upload qua ManageImageService) |
| `BannerUrl` | `string(1024)` | ✓ | Banner trang chủ |
| `TvcUrl` | `string(1024)` | ✓ | Link video TVC |
| `TvcHtml` | `string(max)` | ✓ | Nội dung TVC (HTML — Summernote) |
| `RulesHtml` | `string(max)` | ✓ | Thể lệ chương trình (HTML — Summernote) |
| `GamePlayHtml` | `string(max)` | ✓ | Luật chơi (HTML — Summernote) |
| `StartTime` | `DateTime` | ✓ | Thời gian bắt đầu |
| `EndTime` | `DateTime` | ✓ | Thời gian kết thúc |
| `Scope` | `string(256)` | ✓ | Phạm vi (VD "Toàn quốc") |
| `OrganizerName` | `string(256)` | ✓ | ĐVTC (VD "Dược phẩm Hoa Linh") |
| `IsActive` | `bool` | ✗ | Bật/tắt chương trình |

- **Index:** `IX_AppHl25AppConfig_TenantId` (`TenantId`).
- **FK:** không. **Tái dùng:** trường HTML → Summernote; ảnh → `IManageImageService`.
- **Zalo OA/ZNS:** KHÔNG tạo entity mới — Admin trỏ tới cấu hình Zalo dùng chung (`ZaloAuth` + `ZaloSettingNames`); Nhật ký Zalo đọc từ `AppZaloLog`.

### 5.2 Nhóm 2 — Quản lý Frame

#### `Hl25FrameCampaign` — `[Table("AppHl25FrameCampaigns", Schema="hl25")]`
Đợt/chiến dịch ghép ảnh.

| Field | Kiểu | Null | Mô tả |
|-------|------|------|-------|
| `Name` | `string(256)` | ✗ | Tên chiến dịch |
| `Description` | `string(2000)` | ✓ | Mô tả |
| `StartTime` | `DateTime` | ✓ | Bắt đầu |
| `EndTime` | `DateTime` | ✓ | Kết thúc |
| `Status` | `Hl25CampaignStatus (byte)` | ✗ | Draft/Active/Paused/Ended |
| `Templates` | nav `ICollection<Hl25FrameTemplate>` | — | Child collection (dùng `WithDetailsAsync`) |

- **Index:** `IX_..._TenantId_Status` (`TenantId`,`Status`).

#### `Hl25FrameTemplate` — `[Table("AppHl25FrameTemplates", Schema="hl25")]`
Mẫu frame thuộc chiến dịch.

| Field | Kiểu | Null | Mô tả |
|-------|------|------|-------|
| `CampaignId` | `Guid` | ✗ | **FK** → `Hl25FrameCampaign.Id` |
| `Name` | `string(256)` | ✗ | Tên mẫu |
| `ImageUrl` | `string(1024)` | ✗ | Ảnh mẫu frame (PNG khung trong suốt) |
| `ThumbnailUrl` | `string(1024)` | ✓ | Ảnh thu nhỏ |
| `DisplayOrder` | `int` | ✗ | Thứ tự hiển thị |
| `IsActive` | `bool` | ✗ | Bật/tắt |

- **PK:** `Id`. **FK:** `CampaignId` → `Hl25FrameCampaigns.Id` (`OnDelete: Cascade`).
- **Index:** `IX_..._TenantId_CampaignId` (`TenantId`,`CampaignId`).

#### `Hl25FrameCreation` — `[Table("AppHl25FrameCreations", Schema="hl25")]`
Lịch sử tạo ảnh của người dùng (đối soát & chăm sóc).

| Field | Kiểu | Null | Mô tả |
|-------|------|------|-------|
| `ParticipantId` | `Guid` | ✗ | **FK** → `Hl25Participant.Id` |
| `CampaignId` | `Guid?` | ✓ | **FK** → `Hl25FrameCampaign.Id` |
| `TemplateId` | `Guid?` | ✓ | **FK** → `Hl25FrameTemplate.Id` |
| `ResultImageUrl` | `string(1024)` | ✗ | Ảnh thiệp đã tạo |
| `WishMessage` | `string(500)` | ✓ | Lời chúc (≤ 500 ký tự) |
| `ShareLink` | `string(1024)` | ✓ | Link chia sẻ |
| `SharePlatform` | `Hl25SharePlatform (byte)` | ✗ | None/Zalo/Facebook |
| `ShareTime` | `DateTime?` | ✓ | Thời điểm chia sẻ |
| `CreatedTime` | `DateTime` | ✗ | Thời điểm tạo ảnh (= CreationTime, giữ riêng cho báo cáo) |

- **FK:** `ParticipantId` (Restrict), `CampaignId`/`TemplateId` (SetNull).
- **Index:** `IX_..._TenantId_ParticipantId`, `IX_..._TenantId_CampaignId`, `IX_..._TenantId_CreatedTime` (phục vụ báo cáo theo thời gian).

### 5.3 Nhóm 3 — Vòng quay may mắn

#### `Hl25WheelConfig` — `[Table("AppHl25WheelConfig", Schema="hl25")]`
Cấu hình UI vòng quay. **Đã chốt: singleton theo tenant** (1 cấu hình / tenant, KHÔNG gắn campaign).

| Field | Kiểu | Null | Mô tả |
|-------|------|------|-------|
| `Title` | `string(256)` | ✓ | Tiêu đề ("Vòng quay may mắn") |
| `SubTitle` | `string(512)` | ✓ | Phụ đề |
| `PrimaryColor` | `string(16)` | ✓ | Màu chủ đạo (hex) |
| `SecondaryColor` | `string(16)` | ✓ | Màu phụ (hex) |
| `BackgroundImageUrl` | `string(1024)` | ✓ | Ảnh nền vòng quay |
| `PointerImageUrl` | `string(1024)` | ✓ | Ảnh kim quay |
| `SlotCount` | `int` | ✗ | Số ô quay |
| `IsActive` | `bool` | ✗ | Bật/tắt |
| `Slots` | nav `ICollection<Hl25WheelSlot>` | — | Child collection |

- **Index:** `IX_..._TenantId`.

#### `Hl25WheelSlot` — `[Table("AppHl25WheelSlots", Schema="hl25")]`
Từng ô trên vòng quay + tỷ lệ trúng.

| Field | Kiểu | Null | Mô tả |
|-------|------|------|-------|
| `WheelConfigId` | `Guid` | ✗ | **FK** → `Hl25WheelConfig.Id` |
| `GiftId` | `Guid?` | ✓ | **FK** → `Hl25Gift.Id` (null = ô "Chúc may mắn") |
| `Label` | `string(256)` | ✓ | Nhãn hiển thị trên ô |
| `SlotImageUrl` | `string(1024)` | ✓ | Ảnh quà trên ô |
| `WinRate` | `decimal(9,4)` | ✗ | Tỷ lệ trúng (%) của ô |
| `DisplayOrder` | `int` | ✗ | Vị trí ô |
| `ColorHex` | `string(16)` | ✓ | Màu ô |

- **FK:** `WheelConfigId` (Cascade), `GiftId` (SetNull).
- **Index:** `IX_..._TenantId_WheelConfigId`.
- **Ràng buộc nghiệp vụ:** tổng `WinRate` các ô nên = 100 (validate ở AppService, không ở DB).

#### `Hl25Gift` — `[Table("AppHl25Gifts", Schema="hl25")]`
Kho quà tặng.

| Field | Kiểu | Null | Mô tả |
|-------|------|------|-------|
| `Name` | `string(256)` | ✗ | Tên quà |
| `ImageUrl` | `string(1024)` | ✓ | Hình ảnh quà |
| `Description` | `string(1000)` | ✓ | Mô tả (VD "Combo 04 sản phẩm…") |
| `TotalQuantity` | `int` | ✗ | Tổng số lượng |
| `RemainingQuantity` | `int` | ✗ | Số lượng còn lại (giảm khi trao) |
| `Value` | `decimal(18,2)` | ✓ | Giá trị quà |
| `Status` | `Hl25GiftStatus (byte)` | ✗ | Available/OutOfStock/Disabled |

- **Index:** `IX_..._TenantId_Status`.
- **Nghiệp vụ:** khi `RemainingQuantity <= 0` → tự set `Status=OutOfStock` (AppService); trừ kho phải ACID trong transaction lúc quay trúng.

#### `Hl25SpinTurnLog` — `[Table("AppHl25SpinTurnLogs", Schema="hl25")]`
Lịch sử **nhận lượt quay** (mỗi lần cộng lượt).

| Field | Kiểu | Null | Mô tả |
|-------|------|------|-------|
| `ParticipantId` | `Guid` | ✗ | **FK** → `Hl25Participant.Id` |
| `Source` | `Hl25SpinTurnSource (byte)` | ✗ | InitialCardCreated/ShareZalo/ShareFacebook/AdminGrant/Other |
| `TurnsAdded` | `int` | ✗ | Số lượt cộng (thường +1) |
| `Note` | `string(512)` | ✓ | Ghi chú nguồn (VD id bài chia sẻ) |
| `GrantedTime` | `DateTime` | ✗ | Thời điểm cộng lượt |
| `FrameCreationId` | `Guid?` | ✓ | **FK** → `Hl25FrameCreation.Id` (nếu cộng từ chia sẻ thiệp) |

- **FK:** `ParticipantId` (Restrict), `FrameCreationId` (SetNull).
- **Index:** `IX_..._TenantId_ParticipantId`, `IX_..._TenantId_GrantedTime`.

#### `Hl25SpinLog` — `[Table("AppHl25SpinLogs", Schema="hl25")]`
Lịch sử **lượt quay đã thực hiện**.

| Field | Kiểu | Null | Mô tả |
|-------|------|------|-------|
| `ParticipantId` | `Guid` | ✗ | **FK** → `Hl25Participant.Id` |
| `WheelSlotId` | `Guid?` | ✓ | **FK** → `Hl25WheelSlot.Id` (ô trúng) |
| `GiftId` | `Guid?` | ✓ | **FK** → `Hl25Gift.Id` (quà trúng, null nếu trượt) |
| `GiftNameSnapshot` | `string(256)` | ✓ | Snapshot tên quà tại thời điểm quay |
| `SpinTime` | `DateTime` | ✗ | Thời điểm quay |
| `RewardStatus` | `Hl25RewardStatus (byte)` | ✗ | Pending/Won/NotWon/Delivered/Cancelled |
| `DeliveredTime` | `DateTime?` | ✓ | Thời điểm trao thưởng |
| `ReceiverAddressSnapshot` | `string(512)` | ✓ | Snapshot địa chỉ nhận quà |

- **FK:** `ParticipantId` (Restrict), `WheelSlotId`/`GiftId` (SetNull).
- **Index:** `IX_..._TenantId_ParticipantId`, `IX_..._TenantId_GiftId`, `IX_..._TenantId_SpinTime`, `IX_..._TenantId_RewardStatus`.

### 5.4 Nhóm 4 — Người dùng

#### `Hl25Participant` — `[Table("AppHl25Participants", Schema="hl25")]`
Người tham gia chương trình.

| Field | Kiểu | Null | Mô tả |
|-------|------|------|-------|
| `ZaloUserId` | `string(64)` | ✓ | ID người dùng Zalo (định danh Mini App) |
| `FullName` | `string(256)` | ✓ | Họ và tên |
| `PhoneNumber` | `string(13)` | ✓ | SĐT (regex `^(0\d{9,10}\|84\d{9,10})$` — theo RULES.md) |
| `BirthDate` | `DateTime?` | ✓ | Ngày sinh |
| `Gender` | `Hl25Gender (byte)` | ✗ | Unknown/Male/Female/Other |
| `ReceiveAddress` | `string(1024)` | ✓ | Địa chỉ nhận quà |
| `JoinedTime` | `DateTime` | ✗ | Ngày tham gia |
| `IsFollowingOa` | `bool` | ✗ | Trạng thái Follow OA |
| `HasConsent` | `bool` | ✗ | Đồng ý chia sẻ thông tin |
| `ConsentTime` | `DateTime?` | ✓ | Thời điểm đồng ý |
| `RemainingSpinTurns` | `int` | ✗ | Số lượt quay còn lại |
| `TotalSpinTurns` | `int` | ✗ | Tổng lượt đã nhận (tối đa 2 — xem quy tắc) |
| `EarnedCycles` | `int` | ✗ | Số chu kỳ "Tạo thiệp + Chia sẻ" đã hoàn tất (tối đa 2) |
| `TotalGiftsWon` | `int` | ✗ | Tổng số quà đã trúng |
| `AvatarUrl` | `string(1024)` | ✓ | Ảnh đại diện Zalo |

- **Index:** `IX_..._TenantId_ZaloUserId` (unique theo tenant), `IX_..._TenantId_PhoneNumber`, `IX_..._TenantId_JoinedTime`.
- **Nghiệp vụ (đã chốt):** `RemainingSpinTurns` giảm khi quay; **tăng +1 mỗi khi hoàn tất 1 chu kỳ "Tạo thiệp → Chia sẻ thành công"**, ghi `Hl25SpinTurnLog`. `EarnedCycles` chặn trần: khi `EarnedCycles >= Hl25Consts.MaxSpinTurnsPerUser (=2)` thì không cộng thêm lượt. Cập nhật phải khớp sổ lượt.
- **KHÔNG có trường `Points`** (điểm số thuộc module gamification — ngoài phạm vi hl25, theo quyết định đã chốt).

### 5.5 Nhóm 5 — Báo cáo (không entity mới)
Query tổng hợp (dùng `AsyncExecuter`) từ các bảng trên:
- **Đổi Frame theo thời gian/chiến dịch:** group `Hl25FrameCreation` theo `CreatedTime`/`CampaignId`.
- **Tham gia Vòng quay:** đếm distinct `ParticipantId` + `SUM(TurnsAdded)`/`COUNT(SpinLog)`.
- **Vòng quay theo Quà:** group `Hl25SpinLog` theo `GiftId` (đã trao) đối chiếu `Hl25Gift.TotalQuantity/RemainingQuantity` (tỷ lệ + tồn kho).

### 5.6 Sơ đồ quan hệ (tóm tắt)
```
Hl25FrameCampaign 1───* Hl25FrameTemplate
Hl25FrameCampaign 1───* Hl25FrameCreation *───1 Hl25Participant
Hl25FrameTemplate 0..1─* Hl25FrameCreation
Hl25WheelConfig   1───* Hl25WheelSlot *───0..1 Hl25Gift
Hl25Participant   1───* Hl25SpinTurnLog 0..1─* Hl25FrameCreation
Hl25Participant   1───* Hl25SpinLog *───0..1 Hl25WheelSlot / Hl25Gift
Hl25AppConfig  (singleton/tenant, độc lập)
Zalo OA/ZNS/Log: TÁI DÙNG (ZaloAuth / ZaloLog / ZaloSettingNames) — không thuộc schema hl25
```

---

## 6. DTOs & APPSERVICES

> Quy ước: mỗi entity có bộ `XxxDto` (đọc), `CreateUpdateXxxDto` (ghi), `GetXxxListInput : PagedAndSortedResultRequestDto` (lọc). AppService kế thừa `CrudAppService` khi CRUD chuẩn; service tùy biến (báo cáo, mini-app) tự viết. Đăng ký AutoMapper trong `MultiTenancyApplicationAutoMapperProfile.cs`.

### 6.1 Nhóm 1 — Cài đặt
- **`Hl25AppConfigDto`** / **`CreateUpdateHl25AppConfigDto`**: toàn bộ field mục 5.1. Trường HTML (`RulesHtml`,`GamePlayHtml`,`TvcHtml`) — không cần `JsonPropertyName` (repo dùng `SnakeCaseLower` policy).
- **`IHl25AppConfigAppService`**: `GetAsync()` (lấy config tenant hiện tại, tạo mặc định nếu chưa có), `UpdateAsync(dto)`, `UploadAssetAsync(IRemoteStreamContent file, string assetType)` (logo/banner → `IManageImageService`).
- Zalo OA/ZNS: **KHÔNG DTO mới** — Admin tái dùng trang cấu hình Zalo hiện có; hl25 chỉ link menu tới.

### 6.2 Nhóm 2 — Frame
- **`Hl25FrameCampaignDto`** (kèm `List<Hl25FrameTemplateDto> Templates`) / **`CreateUpdateHl25FrameCampaignDto`**.
- **`Hl25FrameTemplateDto`** / **`CreateUpdateHl25FrameTemplateDto`** (+ `UploadTemplateImageAsync`).
- **`Hl25FrameCreationDto`** (đọc-only, cho lịch sử) / **`GetHl25FrameCreationListInput`** (lọc theo `CampaignId`, `ParticipantId`, khoảng `CreatedTime`, `SharePlatform`).
- **`IHl25FrameCampaignAppService : ICrudAppService<...>`**; **`IHl25FrameTemplateAppService`**; **`IHl25FrameCreationAppService`** (chỉ `GetListAsync` + `GetAsync` + `ExportExcelAsync`).
- **Lưu ý MARS/autoSave (RULES.md):** khi tạo campaign kèm templates → insert campaign trước, template sau qua repo; không cascade insert một phát.

### 6.3 Nhóm 3 — Vòng quay
- **`Hl25WheelConfigDto`** (+ `List<Hl25WheelSlotDto> Slots`) / **`CreateUpdateHl25WheelConfigDto`** (kèm danh sách slot; validate tổng `WinRate = 100`).
- **`Hl25GiftDto`** / **`CreateUpdateHl25GiftDto`** (+ `UploadGiftImageAsync`); `GetHl25GiftListInput` (lọc `Status`).
- **`Hl25SpinTurnLogDto`** (đọc-only) / **`GetHl25SpinTurnLogListInput`** (lọc `ParticipantId`,`Source`, khoảng `GrantedTime`).
- **`Hl25SpinLogDto`** (đọc-only) / **`GetHl25SpinLogListInput`** (lọc `ParticipantId`,`GiftId`,`RewardStatus`, khoảng `SpinTime`).
- **`IHl25WheelConfigAppService`**, **`IHl25GiftAppService : ICrudAppService`**, **`IHl25SpinTurnLogAppService`** (list/export), **`IHl25SpinLogAppService`** (list/export + `UpdateRewardStatusAsync(id, status, deliveredTime)` để đánh dấu đã trao).
- **Trừ kho ACID (RULES.md pattern loyalty):** khi mini-app quay trúng → transaction: trừ `Hl25Gift.RemainingQuantity`, ghi `Hl25SpinLog`, giảm `Participant.RemainingSpinTurns`. Trao thưởng qua Admin là bước 2 (`RewardStatus → Delivered`).

### 6.4 Nhóm 4 — Người dùng
- **`Hl25ParticipantDto`** / **`CreateUpdateHl25ParticipantDto`** (Admin sửa được địa chỉ/tên); `GetHl25ParticipantListInput` (lọc `IsFollowingOa`,`HasConsent`, khoảng `JoinedTime`, keyword tên/SĐT).
- **`IHl25ParticipantAppService : ICrudAppService`** + `ExportExcelAsync` + `GrantSpinTurnAsync(participantId, turns, note)` (cộng lượt thủ công → ghi `Hl25SpinTurnLog` source=AdminGrant).
- **Phone regex** `^(0\d{9,10}|84\d{9,10})$`, maxlength 13 — đồng bộ DTO + cshtml + JS + server (RULES.md).

### 6.5 Nhóm 5 — Báo cáo
- **`IHl25ReportAppService`** (không CRUD):
  - `GetFrameStatsAsync(GetFrameStatsInput)` → theo thời gian/chiến dịch (số lượt tạo, số người, số chia sẻ).
  - `GetWheelParticipationStatsAsync(input)` → số người tham gia + tổng lượt quay + tổng lượt cấp.
  - `GetWheelGiftStatsAsync(input)` → theo quà: đã trao / tỷ lệ / tồn kho còn lại.
- DTO kết quả: `Hl25FrameStatsDto`, `Hl25WheelParticipationStatsDto`, `Hl25WheelGiftStatsDto` (list dòng + tổng). Query bằng `AsyncExecuter` (không EF trực tiếp).

### 6.6 MiniApp Controller (API cho FE Zalo)
- **`HoaLinh25MiniAppController`** (`src/…HttpApi/Controllers/`) — endpoints tiêu dùng bởi FE (tham chiếu `HoaLinhMiniAppController`):
  - `GET /api/hl25/config` — cấu hình + thể lệ.
  - `POST /api/hl25/participants/register` — đăng ký/upsert theo `ZaloUserId`.
  - `PUT /api/hl25/participants/me` — cập nhật thông tin cá nhân.
  - `POST /api/hl25/frames` — tạo thiệp (upload ảnh + lời chúc), trả `ShareLink`. **Chưa cộng lượt.**
  - `POST /api/hl25/frames/{id}/share` — xác nhận chia sẻ (Zalo/FB) thành công → **hoàn tất 1 chu kỳ, +1 lượt** (nếu `EarnedCycles < 2`). Transaction: tăng `RemainingSpinTurns`+`TotalSpinTurns`+`EarnedCycles`, ghi `Hl25SpinTurnLog`, set `Hl25FrameCreation.SharePlatform/ShareTime`.
  - `GET /api/hl25/wheel` — cấu hình vòng quay + số lượt còn lại.
  - `POST /api/hl25/wheel/spin` — thực hiện quay (transaction ACID).
  - `GET /api/hl25/me/gifts` — lịch sử nhận quà.
- Internal AppService nhiều param phức tạp → cân nhắc `[RemoteService(false)]` + `[DisableValidation]` (RULES.md).

---

## 7. KẾ HOẠCH TRIỂN KHAI THEO PHASE (chờ xác nhận)

> Nguyên tắc: mỗi Phase build sạch (`dotnet build` 0 error) trước khi sang phase kế. Migration tạo khi Web KHÔNG lock dll (kill Web + clean bin/obj — RULES.md). KHÔNG viết code cho tới khi anh xác nhận.

| Phase | Nội dung | Deliverable | Migration |
|-------|----------|-------------|-----------|
| **P0 — Foundation** | Enums/Consts (`Domain.Shared/Hl25`), Feature `Hl25.Management` + provider, Permission dual (Tenant/Host) trong `MultiTenancyPermissions` + provider, Menu group `MenuGroup.Hl25` (feature+permission check). | Build sạch, menu ẩn/hiện đúng feature. | — |
| **P1 — Entities + DB** | 10 entity (mục 5) + `MultiTenancyDbContextModelCreatingExtensionsHl25.cs` (`ConfigureHl25Module`) + DbSet + index/FK. | Migration `AddHl25Module`. | ✅ 1 migration |
| **P2 — Cài đặt Mini App** | `Hl25AppConfig` CRUD + trang `Web/Pages/Hl25/Settings` với **Summernote** (Thể lệ/Luật/TVC) + upload Logo/Banner (`IManageImageService`, giới hạn 5MB). Link tới cấu hình Zalo OA/ZNS + Nhật ký Zalo có sẵn. | Trang cài đặt hoạt động. | — |
| **P3 — Frame** | Campaign + Template CRUD (upload mẫu frame) + trang Lịch sử tạo ảnh (filter + Excel export). | 3 màn Admin Frame. | (nếu bổ sung field) |
| **P4 — Vòng quay** | WheelConfig + Slots (UI/màu/tỷ lệ), Gift (kho quà), Lịch sử nhận lượt, Lịch sử lượt quay (+ đánh dấu trao thưởng). | 4 màn Admin Wheel. | (nếu bổ sung field) |
| **P5 — Người dùng** | Participant list/detail/edit + cộng lượt thủ công + Excel export. | Màn Người dùng. | — |
| **P6 — Báo cáo** | 3 báo cáo (Frame / Vòng quay tham gia / Vòng quay theo quà) + biểu đồ. | Dashboard báo cáo. | — |
| **P7 — MiniApp API** | `HoaLinh25MiniAppController` (register/frame/share/spin/gifts) + spin ACID + tích hợp ZNS/ZBS thông báo (tùy chọn). | API cho FE Zalo. | — |

**Thứ tự ưu tiên đề xuất:** P0 → P1 → P2 → P4 (vòng quay là lõi) → P3 → P5 → P6 → P7. (Có thể điều chỉnh theo anh.)

### 7.1 Rủi ro & lưu ý
- **5MB limit:** `ManageImageService` không chặn size → tự validate trước upload (P2/P3).
- **Nguồn "điểm số":** ngoài phạm vi (thuộc gamification). Chỉ lưu như thuộc tính; nếu cần liên thông với module gamification parked → xử lý khi merge, không làm ở đây.
- **Trùng lặp Zalo:** tái dùng hoàn toàn module Zalo OA/ZNS/Log — không tạo bảng Zalo mới trong `hl25`.
- **Feature gating:** RequireFeatures trên root + MỌI child permission (tránh leak).

### 7.2 Quyết định nghiệp vụ (ĐÃ CHỐT 2026-08-25)
1. **Điểm số (Points):** ❌ KHÔNG quản lý ở hl25 (thuộc module gamification — ngoài phạm vi). Đã bỏ trường `Points` khỏi `Hl25Participant`.
2. **Vòng quay:** ✅ **Singleton theo tenant** (1 cấu hình `Hl25WheelConfig` / tenant, không gắn campaign).
3. **Trần lượt quay/người:** ✅ **Tối đa 2 lượt** (`MaxSpinTurnsPerUser = 2`). Cơ chế: mỗi chu kỳ **"Tạo thiệp → Chia sẻ thành công" = +1 lượt**, làm tối đa 2 chu kỳ. Tạo thiệp mà chưa chia sẻ KHÔNG cộng lượt. Trường `EarnedCycles` trên `Hl25Participant` chặn trần.
4. **Trao thưởng:** ✅ **2 bước** — quay trúng = `RewardStatus.Won`; Admin đối chiếu địa chỉ nhận quà rồi đánh dấu `Delivered` (`UpdateRewardStatusAsync`).

---

## 8. TRẠNG THÁI TÀI LIỆU & TRIỂN KHAI
- Bước 1 (Figma) ✅ — trích xuất 10+ màn hình, xác định luồng nghiệp vụ.
- Bước 2 (CSDL) ✅ — 10 entity schema `hl25` + PK/FK/Index.
- Bước 3 (tái sử dụng) ✅ — Summernote / ManageImageService / Zalo OA-ZNS-Log / pattern module HL.
- Bước 4 (tài liệu) ✅ — file này.
- Bước 5 (Phase plan) ✅ — đã duyệt.
- Bước 6 (đồng bộ memory) ✅ — ACTIVE_CONTEXT / TASK_LOG / PROJECT_STATE.

### Tiến độ triển khai
- **✅ P0 — Foundation:** 6 enum (`Enums/Hl25Enums.cs`) + `Hl25/Hl25Consts.cs`; Feature `Hl25.Management` (`AppHl25Features` + provider); Permission dual 5 nhóm Tenant + 5 Host (group `MiniAppHl25` / `MiniAppHl25Host`); menu `MenuGroup.Hl25` (order 51); localization vi/en. Build Web 0 errors.
- **✅ P1 — Entities + DB:** 10 entity (`Domain/DomainModels/AppHl25/`) + `MultiTenancyDbContextModelCreatingExtensionsHl25.cs` (`ConfigureHl25Module`) + 10 DbSet. Migration **`20260825160252_AddHl25Module`** (10 bảng, 23 index, 5 FK). Build EF 0 errors. ⚠️ **CHƯA chạy `dotnet ef database update`** (chờ khi cần áp DB). Đã commit `2ffacb7`.
- **✅ P2 — Cài đặt Mini App (commit `a3b08cc`):** DTO `Hl25AppConfigDto`/`CreateUpdateHl25AppConfigDto` + `IHl25AppConfigAppService`; `Hl25AppConfigAppService` (singleton/tenant: `GetAsync` tự tạo mặc định / `UpdateAsync` / `UploadAssetAsync` validate 5MB) + AutoMapper; trang `Web/Pages/Hl25/Settings` (`.cshtml`+`.cshtml.cs`+`.js`) — Summernote cho Thể lệ/Luật chơi/TVC, upload Logo/Banner (preview + chặn 5MB client), link `/AppZaloAuths` + `/AppZaloLogs`. Build Application + Web 0 errors.
- **✅ P4 — Vòng quay may mắn (commit `c1791fa`):** 4 AppService — `Hl25GiftAppService` (CRUD kho quà + `UploadGiftImageAsync` validate 5MB + tự set OutOfStock khi hết hàng), `Hl25WheelConfigAppService` (singleton/tenant `GetAsync`/`UpdateAsync` cấu hình + slots, validate tổng WinRate=100, pattern MARS delete+insert slots), `Hl25SpinTurnLogAppService` (list read-only, join Participant lấy tên/SĐT), `Hl25SpinLogAppService` (list + `UpdateRewardStatusAsync` trao thưởng 2 bước Won→Delivered) + AutoMapper. Trang `Web/Pages/Hl25/Wheel` 4 tab (Kho quà DataTable + Gift Create/Edit modal; Cấu hình vòng quay + bảng slot động tính tổng %; Lịch sử nhận lượt; Lịch sử lượt quay + nút "đánh dấu đã trao") + `Wheel.js`. Build Application + Web 0 errors.
- **✅ P3 — Quản lý Frame (commit `9243043`):** 3 AppService — `Hl25FrameCampaignAppService` (CRUD chiến dịch + đếm `TemplateCount`), `Hl25FrameTemplateAppService` (CRUD mẫu frame + `UploadTemplateImageAsync` validate 5MB, lọc theo campaign), `Hl25FrameCreationAppService` (list read-only, join Participant + Campaign để hiển thị tên) + AutoMapper. Trang `Web/Pages/Hl25/Frames` 3 tab (Chiến dịch DataTable + Create/Edit modal; Mẫu Frame DataTable + lọc chiến dịch + upload ảnh qua `IFormFile` server-side + Create/Edit modal; Lịch sử tạo ảnh read-only hiển thị thumbnail/lời chúc/link chia sẻ) + `Frames.js`. Build Application + Web 0 errors.
- **✅ P5 — Quản lý Người dùng (commit `50ea1d7`):** `Hl25ParticipantAppService` (list filter theo tên/SĐT/FollowOA/Consent/khoảng ngày; `GetAsync`; `UpdateAsync` sửa thông tin + ghi mốc consent khi bật; `GrantSpinTurnAsync` cộng lượt thủ công source=AdminGrant, KHÔNG áp trần `MaxSpinTurnsPerUser`; `ExportExcelAsync`) + `Hl25ParticipantExcelExporter` (ClosedXML, 11 cột) + AutoMapper. Controller `Hl25ParticipantExcelController` (`api/app/hl25-participant-excel/export`, `[HttpGet]` + `[DisableValidation]`). Trang `Web/Pages/Hl25/Participants` (bảng + bộ lọc + nút Xuất Excel qua `genora.excel.download`) + `ParticipantEditModal` + `ParticipantGrantModal` (cộng lượt) + `Participants.js`. Build Application + HttpApi + Web 0 errors.
- **✅ P6 — Báo cáo Thống kê:** `Hl25ReportAppService` (3 method query qua `AsyncExecuter`, dual permission Reports): `GetFrameStatsAsync` (tổng lượt tạo/chia sẻ + số người distinct + chi tiết theo ngày), `GetWheelParticipationStatsAsync` (số người quay + tổng lượt quay + tổng lượt đã cấp + số trúng + số người còn lượt), `GetWheelGiftStatsAsync` (cơ cấu giải theo quà: tổng/còn kho/đã trúng/đã trao + tỷ lệ %) + DTO `Hl25ReportDtos`. Trang `Web/Pages/Hl25/Reports` (bộ lọc khoảng ngày + 3 khối: Frame KPI + biểu đồ Chart.js line theo ngày; Vòng quay KPI; bảng cơ cấu giải theo quà) + `Reports.js`. Build Application + Web 0 errors. Chưa commit.
- **⏳ Kế tiếp:** P7 (MiniApp API — controller register/frame/share/spin/gifts + spin ACID).


