using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDocuments;

public partial class AppDocumentsDataSeedContributor
{
    private List<PageSeed> GetSalonLocationSchedulePages() => new()
    {
        new PageSeed
        {
            Slug = "gioi-thieu",
            Title = "Giới thiệu",
            DisplayOrder = 1,
            ContentHtml = @"<h2>Cơ sở &amp; Lịch làm việc</h2>
<p>Chuyên mục <strong>Cơ sở &amp; Lịch làm việc</strong> cho phép quản lý các chi nhánh/cơ sở kinh doanh và lịch làm việc của nhân viên Salon Beauty. Đây là nền tảng để vận hành đặt lịch hẹn trên Mini App.</p>

<h3>Tổng quan các chức năng</h3>

<h4>1. Quản lý cơ sở</h4>
<p>Tạo và quản lý thông tin các chi nhánh/cơ sở kinh doanh. Mỗi cơ sở có thông tin riêng về địa chỉ, giờ hoạt động, hình ảnh và cấu hình khung giờ phục vụ. Cơ sở là đơn vị gốc để gắn nhân viên, khung giờ làm việc và booking. Hỗ trợ cấu hình: SlotDuration (thời lượng mỗi khung giờ), BufferTime (thời gian nghỉ giữa 2 khung), MaxCapacityPerSlot (số khách tối đa mỗi khung).</p>

<h4>2. Lịch làm việc</h4>
<p>Quản lý khung giờ làm việc của nhân viên theo từng cơ sở. Hỗ trợ tạo thủ công hoặc tự động sinh khung giờ dựa trên cấu hình cơ sở. Mỗi khung giờ có trạng thái riêng (On/Off/Full/PeakHour) và sức chứa (Capacity/BookedCount). Khi khách đặt lịch, hệ thống tự động cập nhật BookedCount và chuyển trạng thái Full khi hết chỗ. Hỗ trợ xem dạng lịch (FullCalendar) để quản lý trực quan.</p>",
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyLocations,
            HostPermissionName = PermHostSalonBeautyLocations
        },
        new PageSeed
        {
            Slug = "co-so",
            Title = "Quản lý cơ sở",
            DisplayOrder = 2,
            ContentHtml = @"<h2>Quản lý cơ sở</h2>
<p>Trang <strong>Quản lý cơ sở</strong> cho phép tạo và quản lý các chi nhánh/cơ sở kinh doanh của Salon Beauty. Mỗi cơ sở có thông tin riêng về địa chỉ, giờ hoạt động và cấu hình khung giờ.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị tất cả cơ sở với hình ảnh, tên, địa chỉ, liên hệ, giờ hoạt động, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo cơ sở mới với thông tin chi tiết</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin cơ sở</li>
<li><strong>Xóa:</strong> Xóa cơ sở không còn hoạt động</li>
<li><strong>Bật/tắt trạng thái:</strong> Toggle trạng thái hoạt động trực tiếp trên danh sách</li>
<li><strong>Hiển thị trên App:</strong> Toggle hiển thị cơ sở trên Mini App</li>
<li><strong>Xem chi tiết:</strong> Modal xem thông tin chi tiết cơ sở</li>
</ul>
<h3>Cấu hình khung giờ</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trường</th><th>Mô tả</th><th>Ví dụ</th></tr></thead>
<tbody>
<tr><td>SlotDuration</td><td>Thời lượng mỗi khung giờ (phút)</td><td>60 phút</td></tr>
<tr><td>BufferTime</td><td>Thời gian nghỉ giữa 2 khung (phút)</td><td>15 phút</td></tr>
<tr><td>MaxCapacityPerSlot</td><td>Số khách tối đa mỗi khung giờ</td><td>3 khách</td></tr>
<tr><td>Giờ mở cửa</td><td>Giờ bắt đầu hoạt động</td><td>08:00</td></tr>
<tr><td>Giờ đóng cửa</td><td>Giờ kết thúc hoạt động</td><td>20:00</td></tr>
</tbody>
</table>
<h3>Bộ lọc</h3>
<ul>
<li>Tìm kiếm theo từ khóa</li>
<li>Lọc theo trạng thái (Hoạt động / Ngừng)</li>
<li>Lọc theo hiển thị trên App (Có / Không)</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Quản lý cơ sở sẽ được bổ sung</em></p></div>",
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyLocations,
            HostPermissionName = PermHostSalonBeautyLocations
        },
        new PageSeed
        {
            Slug = "lich-lam-viec",
            Title = "Lịch làm việc",
            DisplayOrder = 3,
            ContentHtml = @"<h2>Lịch làm việc</h2>
<p>Trang <strong>Lịch làm việc</strong> cho phép quản lý khung giờ làm việc của nhân viên Salon Beauty. Hệ thống hỗ trợ tạo thủ công hoặc tự động sinh khung giờ theo cấu hình cơ sở.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị khung giờ theo cơ sở, nhân viên, ngày, giờ, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo khung giờ làm việc thủ công</li>
<li><strong>Tự sinh khung giờ:</strong> Tự động tạo khung giờ dựa trên cấu hình cơ sở (SlotDuration, BufferTime)</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật trạng thái, sức chứa khung giờ</li>
<li><strong>Xóa:</strong> Xóa khung giờ</li>
<li><strong>Calendar View:</strong> Xem lịch làm việc dạng lịch (FullCalendar)</li>
</ul>
<h3>Trạng thái khung giờ</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trạng thái</th><th>Mô tả</th><th>Hiển thị Mini App</th></tr></thead>
<tbody>
<tr><td>On (Mở)</td><td>Khung giờ có thể đặt lịch</td><td>Có — màu xanh</td></tr>
<tr><td>Off (Tắt)</td><td>Khung giờ không hoạt động</td><td>Không hiển thị</td></tr>
<tr><td>Full (Đầy)</td><td>Đã hết chỗ</td><td>Có — màu xám (không đặt được)</td></tr>
<tr><td>PeakHour (Giờ cao điểm)</td><td>Giờ cao điểm, vẫn đặt được</td><td>Có — màu đỏ</td></tr>
</tbody>
</table>
<h3>Tự động cập nhật trạng thái</h3>
<ul>
<li>Khi có booking mới → BookedCount tăng</li>
<li>Khi BookedCount = Capacity → tự động chuyển sang Full (nếu không phải IsManualOverride)</li>
<li>Khi booking bị hủy → BookedCount giảm → tự động chuyển về On</li>
</ul>
<h3>Bộ lọc</h3>
<ul>
<li>Cơ sở (dropdown)</li>
<li>Từ ngày — Đến ngày</li>
<li>Tìm kiếm theo từ khóa</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Lịch làm việc sẽ được bổ sung</em></p></div>",
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyTimeSlots,
            HostPermissionName = PermHostSalonBeautyTimeSlots
        }
    };
}
