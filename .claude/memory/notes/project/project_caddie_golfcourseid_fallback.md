---
name: project_caddie_golfcourseid_fallback
description: Caddie GolfCourseId NOT NULL — ResolveGolfCourseIdAsync fallback lấy sân golf duy nhất từ AppGolfCourses
metadata: 
  node_type: memory
  type: project
  originSessionId: 81d5b313-b800-4559-ba7f-4e4acfa2a89a
---

AppCaddies.GolfCourseId là NOT NULL trong DB nhưng UI không bắt buộc chọn sân. Fix:

- Thêm `ResolveGolfCourseIdAsync(Guid? inputGolfCourseId)` trong CaddieAppService
- Nếu input có giá trị và != Guid.Empty → dùng input
- Nếu null/empty → query `_golfCourseRepo` lấy Id đầu tiên (hệ thống chỉ có 1 sân)
- Throw UserFriendlyException nếu không có sân nào
- Dùng trong cả CreateAsync và UpdateAsync

**Why:** DB column `GolfCourseId` là NOT NULL, nhưng form Create/Edit không truyền GolfCourseId (field ẩn hoặc null). Insert fail với SqlException.

**How to apply:** [[project_caddie_avatar_refactor]]
