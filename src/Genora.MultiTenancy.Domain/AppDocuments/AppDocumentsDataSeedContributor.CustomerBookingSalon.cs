using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDocuments;

public partial class AppDocumentsDataSeedContributor
{
    private List<PageSeed> GetCustomerBookingSalonPages() => new()
    {
        new PageSeed
        {
            Slug = "gioi-thieu",
            Title = "Giới thiệu",
            DisplayOrder = 1,
            ContentHtml = @"<h2>Khách hàng &amp; Đặt chỗ (Salon)</h2>
<p>Chuyên mục <strong>Khách hàng &amp; Đặt chỗ (Salon)</strong> cho phép quản lý toàn bộ thông tin khách hàng Salon Beauty và các đơn đặt lịch hẹn dịch vụ.</p>

<h3>Tổng quan các chức năng</h3>

<h4>1. Khách hàng Salon</h4>
<p>Quản lý danh sách khách hàng Salon Beauty với thông tin chi tiết: avatar, mã khách hàng, số điện thoại, hạng thành viên (NEW/REGULAR/VIP/DIAMOND), tổng chi tiêu, lần đặt gần nhất. Hỗ trợ xuất CSV, phân loại theo nhóm/kênh nguồn, và trang chi tiết với KPI cards (Tổng nạp tiền, Số lần ghé, Chi tiêu trung bình, Điểm tích lũy), lịch sử mua hàng và lịch sử nạp tiền.</p>

<h4>2. Đặt lịch Salon</h4>
<p>Quản lý tất cả booking Salon Beauty. Hỗ trợ xem dạng danh sách và lịch (Calendar View), theo dõi trạng thái dịch vụ (Chờ xác nhận → Đã xác nhận → Đang thực hiện → Hoàn thành) và thanh toán. Tính năng nổi bật: thống kê realtime (tổng booking, tổng giá trị, tỷ lệ hoàn thành), auto-refresh, đổi nhân viên, hủy booking với lý do, cập nhật trạng thái inline.</p>",
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyCustomers,
            HostPermissionName = PermHostSalonBeautyCustomers
        },
        new PageSeed
        {
            Slug = "khach-hang-salon",
            Title = "Khách hàng Salon",
            DisplayOrder = 2,
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
<table class=""table table-bordered"">
<thead><tr><th>Thông tin</th><th>Mô tả</th></tr></thead>
<tbody>
<tr><td>KPI Cards</td><td>Tổng nạp tiền, Số lần ghé, Chi tiêu trung bình, Điểm tích lũy</td></tr>
<tr><td>Hạng thành viên</td><td>NEW → REGULAR → VIP → DIAMOND (tự động nâng hạng)</td></tr>
<tr><td>Lịch sử mua hàng</td><td>Danh sách booking đã hoàn thành</td></tr>
<tr><td>Lịch sử nạp tiền</td><td>Danh sách giao dịch nạp tiền (ledger)</td></tr>
</tbody>
</table>
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
            Slug = "dat-lich-salon",
            Title = "Đặt lịch Salon",
            DisplayOrder = 3,
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
<thead><tr><th>Trạng thái</th><th>Mô tả</th><th>Hành động tiếp theo</th></tr></thead>
<tbody>
<tr><td>Chờ xác nhận</td><td>Booking mới tạo</td><td>→ Đã xác nhận</td></tr>
<tr><td>Đã xác nhận</td><td>Nhân viên xác nhận lịch hẹn</td><td>→ Đang thực hiện</td></tr>
<tr><td>Đang thực hiện</td><td>Khách đang sử dụng dịch vụ</td><td>→ Hoàn thành</td></tr>
<tr><td>Hoàn thành</td><td>Dịch vụ đã hoàn tất</td><td>Kết thúc</td></tr>
<tr><td>Đã hủy</td><td>Booking bị hủy</td><td>—</td></tr>
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
<h3>Tính năng đặc biệt</h3>
<ul>
<li><strong>Đổi nhân viên:</strong> Chuyển booking sang nhân viên khác cùng cơ sở, hệ thống tự ghi chú nội bộ</li>
<li><strong>Lịch sử thao tác:</strong> Trang chi tiết hiển thị toàn bộ lịch sử thay đổi trạng thái</li>
<li><strong>Gửi ZNS tự động:</strong> Khi tạo booking → gửi thông báo; khi hoàn thành → gửi link đánh giá</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Đặt lịch Salon sẽ được bổ sung</em></p></div>",
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyBookings,
            HostPermissionName = PermHostSalonBeautyBookings
        }
    };
}
