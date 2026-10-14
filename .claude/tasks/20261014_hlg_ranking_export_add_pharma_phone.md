[HLG] Bổ sung thêm cột SĐT Chủ nhà thuốc vào file Xuất Excel kết quả của tính năng quản trị Sự kiện xếp hạng (/Hlg/Ranking):

BỐI CẢNH
File Xuất Excel kết quả của tính năng quản trị Sự kiện xếp hạng (/Hlg/Ranking) đang thiếu cột SĐT Chủ nhà thuốc.

YÊU CẦU

- Hãy kiểm tra và cập nhật tính năng Xuất Excel kết quả trong tính năng quản trị Sự kiện xếp hạng (/Hlg/Ranking) bổ sung cột SĐT Chủ nhà thuốc (nằm sau cột Zalo User ID) lấy từ trường PharmaPhone trong bảng [HLG].[AppHlgUserProfiles].
- Test dữ liệu excel sau khi xuất ra đảm bảo có trường thông tin SĐT Chủ nhà thuốc. Vì khi đăng ký bắt buộc nhập số điện thoại chủ nhà thuốc nên trường này luôn luôn có dữ liệu.

## Trạng thái thực hiện — 2026-10-14
- Đã triển khai cột G `SĐT Chủ nhà thuốc`, đọc profile theo CustomerId + tenant; cả export report và finalize/re-export snapshot; giá trị xuất kiểu text. Legacy profile thiếu/null có thể để trống.
- HLG Application tests 148/148 pass; Web build 0 errors/61 warnings trong phiên implementation.
- Commit `5d4f2cd`, merge vào `dev` tại `779367f`. Không migration/API change. Chưa staging/production UAT.
- Chi tiết: [project note](../memory/notes/project/project_hlg_ranking_export_pharma_phone_20261014.md).
