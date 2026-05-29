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
            ContentHtml = @"<h2>Khách hàng &amp; Đặt chỗ</h2>
<p>Chuyên mục <strong>Khách hàng &amp; Đặt chỗ</strong> cho phép quản lý toàn bộ thông tin khách hàng và các đơn đặt chỗ (booking) từ nhiều nguồn khác nhau: Mini App, Hotline, Agent.</p>
<h3>Các chức năng trong chuyên mục</h3>
<ul>
<li><strong>Khách hàng Golf</strong> — Quản lý thông tin khách hàng sân golf</li>
<li><strong>Khách hàng Salon</strong> — Quản lý thông tin khách hàng Salon Beauty</li>
<li><strong>Đặt chỗ Golf</strong> — Quản lý booking sân golf</li>
<li><strong>Đặt lịch Salon</strong> — Quản lý booking Salon Beauty</li>
</ul>",
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
            Slug = "khach-hang-salon",
            Title = "Khách hàng Salon",
            DisplayOrder = 3,
            ContentHtml = @"<h2>Khách hàng Salon</h2>
<p>Trang <strong>Khách hàng Salon</strong> cho phép quản lý danh sách khách hàng Salon Beauty với thông tin chi tiết về hạng thành viên, tổng chi tiêu, lịch sử đặt lịch.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị khách hàng với avatar, tên, mã, SĐT, hạng thành viên, tổng chi tiêu, lần đặt gần nhất</li>
<li><strong>Thêm mới:</strong> Tạo khách hàng mới</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin khách hàng</li>
<li><strong>Xóa:</strong> Xóa khách hàng</li>
<li><strong>Xuất danh sách (CSV):</strong> Export danh sách khách hàng</li>
<li><strong>Xem chi tiết:</strong> Trang chi tiết với KPI, lịch sử mua hàng, lịch sử nạp tiền</li>
</ul>
<h3>Trang chi tiết khách hàng</h3>
<ul>
<li><strong>KPI Cards:</strong> Tổng nạp tiền, Số lần ghé, Chi tiêu trung bình, Điểm tích lũy</li>
<li><strong>Hạng thành viên:</strong> NEW → REGULAR → VIP → DIAMOND (tự động nâng hạng)</li>
<li><strong>Lịch sử mua hàng:</strong> Danh sách booking đã hoàn thành</li>
<li><strong>Lịch sử nạp tiền:</strong> Danh sách giao dịch nạp tiền</li>
</ul>
<h3>Bộ lọc</h3>
<ul>
<li>Tìm kiếm theo từ khóa</li>
<li>Khoảng thời gian (Hôm nay, 7 ngày, 30 ngày, 90 ngày, Tất cả)</li>
<li>Nhóm khách hàng (Mới, Thường xuyên, VIP)</li>
<li>Kênh nguồn (Zalo Mini App, Thủ công...)</li>
<li>Trạng thái hoạt động</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Khách hàng Salon sẽ được bổ sung</em></p></div>",
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyCustomers,
            HostPermissionName = PermHostSalonBeautyCustomers
        },
        new PageSeed
        {
            Slug = "dat-cho-golf",
            Title = "Đặt chỗ Golf",
            DisplayOrder = 4,
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
<ul>
<li><strong>MiniApp:</strong> Khách tự đặt qua Zalo Mini App</li>
<li><strong>Hotline:</strong> Nhân viên đặt hộ qua điện thoại</li>
<li><strong>Agent:</strong> Đại lý đặt hộ</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Đặt chỗ Golf sẽ được bổ sung</em></p></div>",
            FeatureName = FeatBookings,
            TenantPermissionName = PermAppBookings,
            HostPermissionName = PermHostAppBookings
        },
        new PageSeed
        {
            Slug = "dat-lich-salon",
            Title = "Đặt lịch Salon",
            DisplayOrder = 5,
            ContentHtml = @"<h2>Đặt lịch Salon</h2>
<p>Trang <strong>Đặt lịch Salon</strong> cho phép quản lý tất cả booking Salon Beauty. Hỗ trợ xem dạng danh sách và lịch (Calendar View), theo dõi trạng thái dịch vụ và thanh toán.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị booking với mã, khách hàng, dịch vụ, nhân viên, ngày giờ, trạng thái, thanh toán, tổng tiền</li>
<li><strong>Thêm mới:</strong> Tạo booking mới cho khách</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin booking</li>
<li><strong>Xem chi tiết:</strong> Trang chi tiết đầy đủ với lịch sử thao tác</li>
<li><strong>Đổi nhân viên:</strong> Chuyển booking sang nhân viên khác (cùng cơ sở)</li>
<li><strong>Hủy booking:</strong> Hủy với lý do</li>
<li><strong>Cập nhật trạng thái:</strong> Chuyển trạng thái dịch vụ và thanh toán inline</li>
<li><strong>Calendar View:</strong> Xem booking dạng lịch theo ngày/tuần</li>
<li><strong>Auto Refresh:</strong> Tự động làm mới dữ liệu mỗi 30 giây</li>
<li><strong>Thống kê:</strong> Cards tổng quan (Tổng booking, Tổng giá trị, Tỷ lệ hoàn thành, Chưa xử lý)</li>
</ul>
<h3>Trạng thái Booking Salon</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trạng thái</th><th>Mô tả</th></tr></thead>
<tbody>
<tr><td>Chờ xác nhận</td><td>Booking mới tạo</td></tr>
<tr><td>Đã xác nhận</td><td>Nhân viên xác nhận lịch hẹn</td></tr>
<tr><td>Đang thực hiện</td><td>Khách đang sử dụng dịch vụ</td></tr>
<tr><td>Hoàn thành</td><td>Dịch vụ đã hoàn tất</td></tr>
<tr><td>Đã hủy</td><td>Booking bị hủy</td></tr>
</tbody>
</table>
<h3>Bộ lọc</h3>
<ul>
<li>Tìm kiếm (mã booking, tên khách, SĐT)</li>
<li>Cơ sở</li>
<li>Từ ngày — Đến ngày</li>
<li>Trạng thái</li>
<li>Nhân viên</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Đặt lịch Salon sẽ được bổ sung</em></p></div>",
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyBookings,
            HostPermissionName = PermHostSalonBeautyBookings
        }
    };
}
