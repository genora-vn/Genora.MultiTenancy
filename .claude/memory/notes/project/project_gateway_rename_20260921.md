# Đổi tên project gateway dùng chung — 2026-09-21

## Yêu cầu
User yêu cầu đổi `Genora.MultiTenancy.Hl25Gateway` thành `Genora.MultiTenancy.Gateway` trước khi deploy staging, để tên thể hiện đúng vai trò gateway cho nhiều tenant/module.

## Thay đổi
- Di chuyển thư mục và đổi tên csproj thành `src/Genora.MultiTenancy.Gateway/Genora.MultiTenancy.Gateway.csproj`.
- Đổi project kiểm thử thành `test/Genora.MultiTenancy.Gateway.Tests/Genora.MultiTenancy.Gateway.Tests.csproj`.
- Cập nhật namespace/using, ProjectReference, tên và đường dẫn trong solution, launch profile và DLL khởi chạy trong `web.config`. Project GUID trong solution giữ nguyên.
- Assembly publish mới: `Genora.MultiTenancy.Gateway.dll`; assembly test: `Genora.MultiTenancy.Gateway.Tests.dll`.
- Cập nhật lệnh build/test/publish và mô tả project trong `docs/tenant-gateway/README.md`, `docs/hl25-gateway/README.md`.
- Giữ các khóa cấu hình `TenantGateway`, `TenantGatewayGuard` và các lớp/key/header HL25 legacy để tương thích cấu hình cũ. Không đổi route, quota, CORS, tenant mapping, guard hay nghiệp vụ ABP. Không migration/package update.
- Các note cũ giữ lệnh và tên project tại thời điểm thực hiện; thêm chỉ dẫn sang note này để không dùng đường dẫn cũ khi triển khai.

## Kiểm chứng thực chạy
1. `dotnet test test/Genora.MultiTenancy.Gateway.Tests/Genora.MultiTenancy.Gateway.Tests.csproj -c Release --nologo --logger "console;verbosity=minimal"`: restore/build hai project theo tên mới thành công; **42 passed / 0 failed**.
2. `dotnet publish src/Genora.MultiTenancy.Gateway/Genora.MultiTenancy.Gateway.csproj -c Release --no-restore --nologo -o artifacts/gateway-rename-check-20260921`: PASS, chỉ publish vào artifacts local, chưa deploy.
3. Kiểm tra artifact: metadata assembly là `Genora.MultiTenancy.Gateway`; `web.config` gọi `.\Genora.MultiTenancy.Gateway.dll`; không có DLL `Genora.MultiTenancy.Hl25Gateway` trong gói publish.
4. `dotnet sln Genora.MultiTenancy.sln list`: project gateway và test đều trỏ đến đường dẫn mới. Source/test/runbook hiện hành không còn tham chiếu tên project cũ.
5. So sánh 13 file source/config/project với nội dung đã staged trước task: chỉ thay chuỗi tên project/namespace, không có thay đổi nội dung khác. `git diff --check`: PASS.
6. Không chạy lại Web/JS suite vì không thay source Web hoặc script JS; kết quả 40 Web/12 Node ở phiên multi-tenant trước là lịch sử, không nhận là lần chạy mới.

## Trạng thái bàn giao
Branch `hotfix/20260920`. User đã staged các thay đổi gateway phiên trước; thao tác rename chỉ cập nhật working tree, không sửa Git index/stage/commit/push. Khi review/commit cần bao gồm cả đường dẫn cũ bị xóa và project mới. Appsettings Web/DbMigrator, Program Web và logs của user được giữ nguyên.

Runbook triển khai: `docs/tenant-gateway/README.md`. Vẫn cần điền IIS origin staging và secret, deploy rồi test HTTP429/direct-origin403 và tải thực tế. Quota vẫn HL25=500, HLG=300, local một process. Chưa deploy IIS, browser UAT hoặc chạy load test. Gói artifacts kiểm tra rename chưa có cấu hình triển khai/secret, không phải bản đã sẵn sàng kết nối staging.
