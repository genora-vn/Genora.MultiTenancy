# HLG — Tách menu Ngành hàng, Nhãn hàng, Sản phẩm (2026-09-23)

- Thêm hai mục độc lập `Hlg.Brands` (`/Hlg/Brands`) và `Hlg.Products` (`/Hlg/Products`) dưới menu HLG, cạnh `Hlg.Categories`.
- Cả ba mục dùng chung kiểm tra quyền Knowledge theo đúng dual Tenant/Host hiện có.
- Bỏ hai nút điều hướng Brands/Products khỏi trang Categories và link quay về Categories khỏi Brands/Products; luồng drill-down Categories → Brands → Products bằng row action vẫn giữ nguyên.
- Trang Products chỉ hiện tên ngành hàng khi được mở theo ngữ cảnh có `parentId`, tránh tạo khoảng trống khi mở trực tiếp từ menu.
- Xác minh: `git diff --check` pass; Web build sang `artifacts/hlg-menu-build` pass 0 errors. Build output mặc định thất bại ở bước copy do Web host đang khóa DLL, không phải lỗi compile. Không có migration hoặc thay đổi DB.
