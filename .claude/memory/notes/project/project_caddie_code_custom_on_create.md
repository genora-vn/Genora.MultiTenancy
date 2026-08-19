---
name: project-caddie-code-custom-on-create
description: Quản lý Caddie — Create modal cho nhập Mã Caddy tùy ý (bỏ trống tự sinh); Edit giữ read-only
metadata: 
  node_type: memory
  type: project
  originSessionId: fcebfc0a-cff6-46d3-849e-3213a3963a61
  modified: 2026-08-07T08:02:56.660Z
---

## Caddie code — cho nhập tùy ý khi tạo mới (2026-07-30)

Yêu cầu: modal Thêm mới Caddie cho phép nhập Mã Caddy tùy ý (trước đây render sẵn mã disabled), modal Chỉnh sửa giữ nguyên không cho sửa mã.

### 3 thay đổi (Application build + full solution 0 errors, KHÔNG migration):
1. **DTO** `CreateUpdateCaddieDto` (Application.Contracts/AppDtos/Caddies): thêm `[StringLength(50)] string? CaddieCode` — nhập tùy ý khi create, bỏ trống → tự sinh; update bỏ qua.
2. **Service** `CaddieAppService.CreateAsync` (~L196): thay `var code = await GenerateCaddieCodeAsync()` bằng: nếu `input.CaddieCode` có giá trị → dùng (trim) + check trùng qua `AsyncExecuter.AnyAsync` trên `_caddieRepo` (throw `UserFriendlyException` nếu trùng); bỏ trống → `GenerateCaddieCodeAsync()`. `UpdateAsync` KHÔNG đụng CaddieCode (giữ nguyên).
3. **UI** CreateModal.cshtml (~L37): input Mã Caddy đổi từ `disabled value="@Model.GeneratedCode"` → `asp-for="Caddie.CaddieCode"` (editable, value=@Model.GeneratedCode làm gợi ý, placeholder "bỏ trống để tự động sinh"). EditModal.cshtml (~L48) GIỮ NGUYÊN `disabled value="@Model.CaddieCode"` (read-only đúng yêu cầu).

`GenerateCaddieCodeAsync` sinh mã dạng CD-XXX (max CaddieCode bắt đầu "CD-" +1). CreateModal.cshtml.cs OnGetAsync vẫn set GeneratedCode làm giá trị mặc định hiển thị.

Xem [[project_caddie_module_complete]], [[project_caddie_avatar_refactor]].
