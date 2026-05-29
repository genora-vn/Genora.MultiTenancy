using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDocuments;

public partial class AppDocumentsDataSeedContributor
{
    private List<PageSeed> GetCustomerBookingPages() => new()
    {
        new PageSeed
        {
            Slug = "gioi-thieu",
            Title = "Giới thiệu",
            DisplayOrder = 1,
            ContentHtml = @"<h2>Khách hàng &amp; Đặt chỗ (Golf)</h2>
<p>Chuyên mục <strong>Khách hàng &amp; Đặt chỗ (Golf)</strong> cho phép quản lý toàn bộ thông tin khách hàng sân golf và các đơn đặt chỗ (booking) từ nhiều nguồn khác nhau.</p>

<h3>Tổng quan các chức năng</h3>

<h4>1. Khách hàng Golf</h4>
<p>Quản lý danh sách khách hàng sân golf với đầy đủ thông tin: mã khách hàng, họ tên, mã VGA, số điện thoại, loại khách hàng, email và trạng thái. Hỗ trợ import hàng loạt từ Excel, phân loại theo nguồn (Zalo Mini App, Thủ công, Import, Khác) và lọc theo nhiều tiêu chí. Khách hàng từ nguồn Mini App được đồng bộ tự động và không cho phép sửa số điện thoại để đảm bảo tính nhất quán.</p>

<h4>2. Đặt chỗ Golf</h4>
<p>Quản lý tất cả booking sân golf từ nhiều nguồn: Mini App (khách tự đặt), Hotline (nhân viên đặt hộ), Agent (đại lý). Theo dõi trạng thái booking qua các giai đoạn: Đang xử lý → Đã xác nhận → Đã thanh toán → Hoàn thành (hoặc Hủy). Hỗ trợ xuất Excel, lọc theo trạng thái/nguồn/ngày chơi, và quản lý thanh toán (COD, Online, Chuyển khoản).</p>",
            FeatureName = FeatBookings,
            TenantPermissionName = PermAppCustomers,
            HostPermissionName = PermHostAppCustomers
        },
        new PageSeed
        {
            Slug = "khach-hang-golf",
            Title = "Khách hàng Golf",
            DisplayOrder = 2,
            ContentHtml = @"<h2>Khách hàng Golf</h2>
<p>Trang <strong>Khách hàng Golf</strong> cho phép quản lý danh sách khách hàng sân golf, bao gồm thông tin cá nhân, loại khách hàng, mã VGA và nguồn khách hàng.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị khách hàng với mã, họ tên, mã VGA, số điện thoại, loại KH, email, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo khách hàng mới với đầy đủ thông tin</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin khách hàng</li>
<li><strong>Xóa:</strong> Xóa khách hàng</li>
<li><strong>Import Excel:</strong> Nhập hàng loạt khách hàng từ file Excel</li>
<li><strong>Download Template:</strong> Tải file mẫu Excel để import</li>
</ul>
<h3>Bộ lọc</h3>
<ul>
<li>Loại khách hàng (Visitor, Member, Member Guest...)</li>
<li>Nguồn khách hàng (Zalo Mini App, Thủ công, Import, Khác)</li>
<li>Trạng thái (Hoạt động / Ngừng)</li>
<li>Thời gian tạo (Từ ngày — Đến ngày)</li>
</ul>
<h3>Nguồn khách hàng</h3>
<table class=""table table-bordered"">
<thead><tr><th>Nguồn</th><th>Mô tả</th><th>Ghi chú</th></tr></thead>
<tbody>
<tr><td>Zalo Mini App</td><td>Khách đăng ký qua Mini App</td><td>Không được sửa SĐT</td></tr>
<tr><td>Thủ công (Manual)</td><td>Nhân viên tạo trực tiếp</td><td>Sửa được tất cả</td></tr>
<tr><td>Import (Extent)</td><td>Nhập từ file Excel</td><td>Sửa được tất cả</td></tr>
<tr><td>Khác (Other)</td><td>Nguồn khác</td><td>Sửa được tất cả</td></tr>
</tbody>
</table>
<h3>Lưu ý</h3>
<ul>
<li>Khách hàng từ nguồn <strong>Zalo Mini App</strong> không được sửa số điện thoại (vì là key đồng bộ)</li>
<li>Số điện thoại được ẩn một phần trên giao diện để bảo mật</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Khách hàng Golf sẽ được bổ sung</em></p></div>",
            FeatureName = FeatBookings,
            TenantPermissionName = PermAppCustomers,
            HostPermissionName = PermHostAppCustomers
        },
        new PageSeed
        {
            Slug = "dat-cho-golf",
            Title = "Đặt chỗ Golf",
            DisplayOrder = 3,
            ContentHtml = @"<h2>Đặt chỗ Golf</h2>
<p>Trang <strong>Đặt chỗ Golf</strong> cho phép quản lý tất cả booking sân golf từ nhiều nguồn: Mini App, Hotline, Agent. Hỗ trợ theo dõi trạng thái, xuất Excel và quản lý thanh toán.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị booking với mã, khách hàng, loại KH, loại KM, ngày chơi, giờ, số golfer, tổng tiền, trạng thái</li>
<li><strong>Xem chi tiết:</strong> Xem thông tin đầy đủ của booking</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin booking (trừ booking đã hủy)</li>
<li><strong>Xóa:</strong> Xóa booking (trừ booking đã hủy)</li>
<li><strong>Xuất Excel:</strong> Export danh sách booking theo bộ lọc</li>
</ul>
<h3>Trạng thái Booking</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trạng thái</th><th>Mô tả</th></tr></thead>
<tbody>
<tr><td>Đang xử lý</td><td>Booking mới tạo, chờ xác nhận</td></tr>
<tr><td>Đã xác nhận</td><td>Đã xác nhận với khách hàng</td></tr>
<tr><td>Đã thanh toán</td><td>Khách đã thanh toán</td></tr>
<tr><td>Hoàn thành</td><td>Khách đã chơi xong</td></tr>
<tr><td>Hủy hoàn tiền</td><td>Đã hủy và hoàn tiền</td></tr>
<tr><td>Hủy không hoàn tiền</td><td>Đã hủy, không hoàn tiền</td></tr>
</tbody>
</table>
<h3>Bộ lọc</h3>
<ul>
<li>Tìm kiếm (mã booking, tên, SĐT)</li>
<li>Trạng thái booking</li>
<li>Nguồn booking (Mini App, Hotline, Agent)</li>
<li>Ngày chơi (Từ ngày — Đến ngày)</li>
</ul>
<h3>Nguồn Booking</h3>
<table class=""table table-bordered"">
<thead><tr><th>Nguồn</th><th>Mô tả</th></tr></thead>
<tbody>
<tr><td>MiniApp</td><td>Khách tự đặt qua Zalo Mini App</td></tr>
<tr><td>Hotline</td><td>Nhân viên đặt hộ qua điện thoại</td></tr>
<tr><td>Agent</td><td>Đại lý đặt hộ</td></tr>
</tbody>
</table>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Đặt chỗ Golf sẽ được bổ sung</em></p></div>",
            FeatureName = FeatBookings,
            TenantPermissionName = PermAppBookings,
            HostPermissionName = PermHostAppBookings
        }
    };
}
