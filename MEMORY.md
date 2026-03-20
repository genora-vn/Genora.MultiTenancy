# Project Memory: Genora.MultiTenancy

## Current Working State
- **Modules Active:** `AppCalendarSlots`, `AppZaloAuths`.
- **Current Task:** [Tôi sắp làm task: Thêm cấu hình các chức năng liên quan đến tính năng FnB].

## Important Logic Decisions
- `AppZaloAuths`: Sử dụng Zalo Access Token để định danh Tenant.
- `AppCalendarSlotPrices`: Giá được tính dựa trên khung giờ và loại member.

## Things to Remember (To avoid re-reading)
- Cấu trúc Database cho `AppNews` đã ổn định, không cần quét lại Domain layer của News.
- Các Helper trong `Genora.MultiTenancy.Application/Helpers/` dùng chung cho toàn bộ AppServices.