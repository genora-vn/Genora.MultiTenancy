using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDocuments;

public partial class AppDocumentsDataSeedContributor
{
    private List<PageSeed> GetFnbPages() => new()
    {
        new PageSeed
        {
            Slug = "gioi-thieu",
            Title = "Giới thiệu",
            DisplayOrder = 1,
            ContentHtml = @"<h2>F&amp;B (Food &amp; Beverage)</h2>
<p>Chuyên mục <strong>F&amp;B</strong> cho phép quản lý toàn bộ hoạt động đặt đồ ăn thức uống tại sân golf hoặc cơ sở kinh doanh. Từ quản lý danh mục, sản phẩm đến xử lý đơn hàng realtime với bảng bếp chuyên dụng.</p>

<h3>Tổng quan các chức năng</h3>

<h4>1. Danh mục F&amp;B</h4>
<p>Quản lý các nhóm/danh mục món ăn, thức uống (Đồ uống, Món chính, Tráng miệng, Snack...). Hỗ trợ import/export Excel hàng loạt, toggle trạng thái hoạt động trực tiếp trên danh sách. Mỗi sản phẩm F&amp;B sẽ thuộc về một danh mục để phân loại và hiển thị trên Mini App.</p>

<h4>2. Sản phẩm F&amp;B</h4>
<p>Quản lý từng món ăn, thức uống với đầy đủ thông tin: hình ảnh, giá, danh mục, thứ tự hiển thị. Hỗ trợ import/export Excel, toggle trạng thái hoạt động và hiển thị trên Mini App (IsAvailable). Sản phẩm có badge màu theo danh mục để dễ phân biệt.</p>

<h4>3. Đơn hàng F&amp;B</h4>
<p>Quản lý tất cả đơn đặt đồ ăn thức uống từ Mini App. Theo dõi trạng thái phục vụ (Đã tạo → Đang chuẩn bị → Đang giao → Đã phục vụ) và thanh toán (Chưa TT → Đã TT / Thất bại). Hỗ trợ cập nhật trạng thái inline, xuất Excel, auto-refresh và thông báo realtime qua SignalR khi có đơn mới.</p>

<h4>4. Bảng bếp (Kitchen Board)</h4>
<p>Giao diện realtime chuyên dụng cho nhân viên bếp. Hiển thị đơn hàng dạng board/kanban theo trạng thái, phát âm thanh khi có đơn mới, hỗ trợ cập nhật trạng thái nhanh bằng một click. Thiết kế để mở trên màn hình riêng tại khu vực bếp.</p>",
            FeatureName = FeatFnb,
            TenantPermissionName = PermAppFnbOrders,
            HostPermissionName = PermHostAppFnbOrders
        },
        new PageSeed
        {
            Slug = "danh-muc-fnb",
            Title = "Danh mục F&B",
            DisplayOrder = 2,
            ContentHtml = @"<h2>Danh mục F&amp;B</h2>
<p>Trang <strong>Danh mục F&amp;B</strong> cho phép quản lý các nhóm/danh mục món ăn, thức uống. Mỗi sản phẩm F&amp;B sẽ thuộc về một danh mục.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị danh mục với tên, mã, thứ tự sắp xếp, trạng thái</li>
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
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Danh mục F&amp;B sẽ được bổ sung</em></p></div>",
            FeatureName = FeatFnb,
            TenantPermissionName = PermAppFnbCategories,
            HostPermissionName = PermHostAppFnbCategories
        },
        new PageSeed
        {
            Slug = "san-pham-fnb",
            Title = "Sản phẩm F&B",
            DisplayOrder = 3,
            ContentHtml = @"<h2>Sản phẩm F&amp;B</h2>
<p>Trang <strong>Sản phẩm F&amp;B</strong> cho phép quản lý từng món ăn, thức uống với đầy đủ thông tin: hình ảnh, giá, danh mục, trạng thái hiển thị trên Mini App.</p>
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
<li>Hiển thị trên App (Có / Không / Tất cả)</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Sản phẩm F&amp;B sẽ được bổ sung</em></p></div>",
            FeatureName = FeatFnb,
            TenantPermissionName = PermAppFnbItems,
            HostPermissionName = PermHostAppFnbItems
        },
        new PageSeed
        {
            Slug = "don-hang-fnb",
            Title = "Đơn hàng F&B",
            DisplayOrder = 4,
            ContentHtml = @"<h2>Đơn hàng F&amp;B</h2>
<p>Trang <strong>Đơn hàng F&amp;B</strong> cho phép quản lý tất cả đơn đặt đồ ăn thức uống. Hỗ trợ theo dõi trạng thái phục vụ, thanh toán và cập nhật realtime qua SignalR.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị đơn hàng với mã, bag tag, khách hàng, tổng tiền, trạng thái phục vụ, trạng thái thanh toán</li>
<li><strong>Xem nhanh:</strong> Modal xem thông tin đơn hàng</li>
<li><strong>Xem chi tiết:</strong> Mở trang chi tiết đơn hàng</li>
<li><strong>Cập nhật trạng thái phục vụ:</strong> Chuyển trạng thái inline (nút trên cột trạng thái)</li>
<li><strong>Hủy đơn hàng:</strong> Hủy đơn với xác nhận</li>
<li><strong>Xuất Excel:</strong> Export danh sách đơn hàng</li>
<li><strong>Auto Refresh:</strong> Tự động làm mới mỗi 30 giây</li>
<li><strong>Thông báo realtime:</strong> Nhận thông báo đơn hàng mới qua SignalR</li>
</ul>
<h3>Trạng thái phục vụ</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trạng thái</th><th>Mô tả</th><th>Hành động tiếp theo</th></tr></thead>
<tbody>
<tr><td>Đã tạo</td><td>Đơn hàng mới</td><td>→ Đang chuẩn bị</td></tr>
<tr><td>Đang chuẩn bị</td><td>Bếp đang làm</td><td>→ Đang giao</td></tr>
<tr><td>Đang giao</td><td>Đang mang đến khách</td><td>→ Đã phục vụ</td></tr>
<tr><td>Đã phục vụ</td><td>Khách đã nhận</td><td>Hoàn tất</td></tr>
<tr><td>Đã hủy</td><td>Đơn bị hủy</td><td>—</td></tr>
</tbody>
</table>
<h3>Trạng thái thanh toán</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trạng thái</th><th>Mô tả</th></tr></thead>
<tbody>
<tr><td>Chưa thanh toán</td><td>Đơn chưa được thanh toán</td></tr>
<tr><td>Đã thanh toán</td><td>Đã thu tiền thành công</td></tr>
<tr><td>Thất bại</td><td>Thanh toán thất bại</td></tr>
</tbody>
</table>
<h3>Bộ lọc</h3>
<ul>
<li>Tìm kiếm (mã đơn, tên khách, bag tag)</li>
<li>Trạng thái phục vụ</li>
<li>Trạng thái thanh toán</li>
<li>Thời gian tạo (Từ ngày — Đến ngày)</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Đơn hàng F&amp;B sẽ được bổ sung</em></p></div>",
            FeatureName = FeatFnb,
            TenantPermissionName = PermAppFnbOrders,
            HostPermissionName = PermHostAppFnbOrders
        },
        new PageSeed
        {
            Slug = "bang-bep",
            Title = "Bảng bếp (Kitchen Board)",
            DisplayOrder = 5,
            ContentHtml = @"<h2>Bảng bếp (Kitchen Board)</h2>
<p>Trang <strong>Bảng bếp</strong> là giao diện realtime dành cho nhân viên bếp, hiển thị các đơn hàng cần xử lý theo dạng board/kanban. Hỗ trợ cập nhật trạng thái nhanh và nhận thông báo đơn mới tức thì.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Hiển thị dạng Board:</strong> Các đơn hàng được nhóm theo trạng thái phục vụ</li>
<li><strong>Cập nhật trạng thái nhanh:</strong> Click để chuyển đơn sang trạng thái tiếp theo</li>
<li><strong>Thông báo realtime:</strong> Âm thanh + hiển thị khi có đơn hàng mới</li>
<li><strong>Auto Refresh:</strong> Tự động cập nhật giao diện</li>
<li><strong>Xem chi tiết đơn:</strong> Xem danh sách món trong đơn hàng</li>
</ul>
<h3>Hướng dẫn sử dụng</h3>
<ol>
<li>Mở trang <strong>Bảng bếp</strong> trên màn hình riêng tại khu vực bếp</li>
<li>Khi có đơn mới, hệ thống sẽ phát âm thanh thông báo</li>
<li>Nhấn vào đơn hàng để xem chi tiết các món cần chuẩn bị</li>
<li>Nhấn nút chuyển trạng thái khi hoàn thành từng bước</li>
</ol>
<h3>Lưu ý</h3>
<ul>
<li>Nên mở Bảng bếp trên màn hình/tablet riêng đặt tại bếp</li>
<li>Đảm bảo kết nối internet ổn định để nhận thông báo realtime</li>
<li>Có thể mở đồng thời trên nhiều thiết bị</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Bảng bếp sẽ được bổ sung</em></p></div>",
            FeatureName = FeatFnb,
            TenantPermissionName = PermAppFnbKitchenBoard,
            HostPermissionName = PermHostAppFnbKitchenBoard
        }
    };
}
