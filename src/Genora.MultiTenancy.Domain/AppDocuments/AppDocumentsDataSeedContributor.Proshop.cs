using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDocuments;

public partial class AppDocumentsDataSeedContributor
{
    private List<PageSeed> GetProshopPages() => new()
    {
        new PageSeed
        {
            Slug = "gioi-thieu",
            Title = "Giới thiệu",
            DisplayOrder = 1,
            ContentHtml = @"<h2>Proshop</h2>
<p>Chuyên mục <strong>Proshop</strong> cho phép quản lý cửa hàng bán lẻ phụ kiện, trang phục golf. Hỗ trợ quản lý danh mục, sản phẩm, đơn hàng và bảng đơn hàng realtime.</p>
<h3>Các chức năng trong chuyên mục</h3>
<ul>
<li><strong>Danh mục Proshop</strong> — Quản lý nhóm/danh mục sản phẩm</li>
<li><strong>Sản phẩm Proshop</strong> — Quản lý từng sản phẩm với giá và hình ảnh</li>
<li><strong>Đơn hàng Proshop</strong> — Quản lý đơn đặt hàng, theo dõi trạng thái</li>
<li><strong>Bảng đơn hàng</strong> — Giao diện realtime theo dõi và xử lý đơn hàng</li>
</ul>",
            FeatureName = FeatProshop,
            TenantPermissionName = PermAppProOrders,
            HostPermissionName = PermHostAppProOrders
        },
        new PageSeed
        {
            Slug = "danh-muc-proshop",
            Title = "Danh mục Proshop",
            DisplayOrder = 2,
            ContentHtml = @"<h2>Danh mục Proshop</h2>
<p>Trang <strong>Danh mục Proshop</strong> cho phép quản lý các nhóm/danh mục sản phẩm Proshop. Mỗi sản phẩm sẽ thuộc về một danh mục.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị danh mục với mã, tên, thứ tự sắp xếp, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo danh mục mới</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin danh mục</li>
<li><strong>Xóa:</strong> Xóa danh mục không còn sử dụng</li>
<li><strong>Import Excel:</strong> Nhập hàng loạt danh mục từ file Excel</li>
<li><strong>Export Excel:</strong> Xuất danh sách danh mục ra file Excel</li>
<li><strong>Download Template:</strong> Tải file mẫu Excel để import</li>
<li><strong>Bật/tắt trạng thái:</strong> Toggle trạng thái hoạt động trực tiếp trên danh sách</li>
</ul>
<h3>Bộ lọc</h3>
<ul>
<li>Tìm kiếm theo từ khóa</li>
<li>Trạng thái (Hoạt động / Ngừng / Tất cả)</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Danh mục Proshop sẽ được bổ sung</em></p></div>",
            FeatureName = FeatProshop,
            TenantPermissionName = PermAppProCategories,
            HostPermissionName = PermHostAppProCategories
        },
        new PageSeed
        {
            Slug = "san-pham-proshop",
            Title = "Sản phẩm Proshop",
            DisplayOrder = 3,
            ContentHtml = @"<h2>Sản phẩm Proshop</h2>
<p>Trang <strong>Sản phẩm Proshop</strong> cho phép quản lý từng sản phẩm bán lẻ với đầy đủ thông tin: hình ảnh, giá, danh mục, trạng thái hiển thị trên Mini App.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị sản phẩm với hình ảnh, tên, danh mục (badge màu), giá, thứ tự, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo sản phẩm mới với hình ảnh, giá, mô tả</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin sản phẩm</li>
<li><strong>Xóa:</strong> Xóa sản phẩm</li>
<li><strong>Import Excel:</strong> Nhập hàng loạt sản phẩm từ file Excel</li>
<li><strong>Export Excel:</strong> Xuất danh sách sản phẩm ra file Excel</li>
<li><strong>Download Template:</strong> Tải file mẫu Excel để import</li>
<li><strong>Bật/tắt trạng thái:</strong> Toggle trạng thái hoạt động trực tiếp</li>
<li><strong>Bật/tắt hiển thị:</strong> Toggle hiển thị sản phẩm trên Mini App (IsAvailable)</li>
</ul>
<h3>Bộ lọc</h3>
<ul>
<li>Tìm kiếm theo từ khóa</li>
<li>Danh mục (dropdown)</li>
<li>Trạng thái (Hoạt động / Ngừng / Tất cả)</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Sản phẩm Proshop sẽ được bổ sung</em></p></div>",
            FeatureName = FeatProshop,
            TenantPermissionName = PermAppProItems,
            HostPermissionName = PermHostAppProItems
        },
        new PageSeed
        {
            Slug = "don-hang-proshop",
            Title = "Đơn hàng Proshop",
            DisplayOrder = 4,
            ContentHtml = @"<h2>Đơn hàng Proshop</h2>
<p>Trang <strong>Đơn hàng Proshop</strong> cho phép quản lý tất cả đơn đặt hàng Proshop. Hỗ trợ theo dõi trạng thái xử lý, thanh toán và cập nhật realtime qua SignalR.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị đơn hàng với mã, bag tag, khách hàng, tổng tiền, trạng thái phục vụ, trạng thái thanh toán</li>
<li><strong>Xem nhanh:</strong> Modal xem thông tin đơn hàng</li>
<li><strong>Xem chi tiết:</strong> Mở trang chi tiết đơn hàng</li>
<li><strong>Cập nhật trạng thái:</strong> Chuyển trạng thái inline (nút trên cột trạng thái)</li>
<li><strong>Hủy đơn hàng:</strong> Hủy đơn với xác nhận</li>
<li><strong>Xuất Excel:</strong> Export danh sách đơn hàng</li>
<li><strong>Auto Refresh:</strong> Tự động làm mới mỗi 30 giây</li>
<li><strong>Thông báo realtime:</strong> Nhận thông báo đơn hàng mới qua SignalR</li>
</ul>
<h3>Trạng thái phục vụ</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trạng thái</th><th>Mô tả</th><th>Hành động tiếp theo</th></tr></thead>
<tbody>
<tr><td>Đã tạo</td><td>Đơn hàng mới</td><td>→ Đang xử lý</td></tr>
<tr><td>Đang xử lý</td><td>Đang chuẩn bị hàng</td><td>→ Sẵn sàng</td></tr>
<tr><td>Sẵn sàng</td><td>Hàng đã sẵn sàng giao</td><td>→ Đã giao</td></tr>
<tr><td>Đã giao</td><td>Khách đã nhận hàng</td><td>Hoàn tất</td></tr>
<tr><td>Đã hủy</td><td>Đơn bị hủy</td><td>—</td></tr>
</tbody>
</table>
<h3>Trạng thái thanh toán</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trạng thái</th><th>Mô tả</th></tr></thead>
<tbody>
<tr><td>Chưa thanh toán</td><td>Đơn chưa được thanh toán</td></tr>
<tr><td>Đã thanh toán</td><td>Đã thu tiền thành công</td></tr>
<tr><td>Hoàn tiền</td><td>Đã hoàn tiền cho khách</td></tr>
</tbody>
</table>
<h3>Bộ lọc</h3>
<ul>
<li>Tìm kiếm (mã đơn, tên khách, bag tag)</li>
<li>Trạng thái phục vụ</li>
<li>Trạng thái thanh toán</li>
<li>Thời gian tạo (Từ ngày — Đến ngày)</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Đơn hàng Proshop sẽ được bổ sung</em></p></div>",
            FeatureName = FeatProshop,
            TenantPermissionName = PermAppProOrders,
            HostPermissionName = PermHostAppProOrders
        },
        new PageSeed
        {
            Slug = "bang-don-hang",
            Title = "Bảng đơn hàng (Orders Board)",
            DisplayOrder = 5,
            ContentHtml = @"<h2>Bảng đơn hàng (Orders Board)</h2>
<p>Trang <strong>Bảng đơn hàng</strong> là giao diện realtime dành cho nhân viên Proshop, hiển thị các đơn hàng cần xử lý theo dạng board. Hỗ trợ cập nhật trạng thái nhanh và nhận thông báo đơn mới tức thì.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Hiển thị dạng Board:</strong> Các đơn hàng được nhóm theo trạng thái xử lý</li>
<li><strong>Cập nhật trạng thái nhanh:</strong> Click để chuyển đơn sang trạng thái tiếp theo</li>
<li><strong>Thông báo realtime:</strong> Âm thanh + hiển thị khi có đơn hàng mới</li>
<li><strong>Auto Refresh:</strong> Tự động cập nhật giao diện</li>
<li><strong>Xem chi tiết đơn:</strong> Xem danh sách sản phẩm trong đơn hàng</li>
</ul>
<h3>Hướng dẫn sử dụng</h3>
<ol>
<li>Mở trang <strong>Bảng đơn hàng</strong> trên màn hình tại quầy Proshop</li>
<li>Khi có đơn mới, hệ thống sẽ phát âm thanh thông báo</li>
<li>Nhấn vào đơn hàng để xem chi tiết sản phẩm cần chuẩn bị</li>
<li>Nhấn nút chuyển trạng thái khi hoàn thành từng bước xử lý</li>
</ol>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Bảng đơn hàng sẽ được bổ sung</em></p></div>",
            FeatureName = FeatProshop,
            TenantPermissionName = PermAppProOrdersBoard,
            HostPermissionName = PermHostAppProOrdersBoard
        }
    };
}
