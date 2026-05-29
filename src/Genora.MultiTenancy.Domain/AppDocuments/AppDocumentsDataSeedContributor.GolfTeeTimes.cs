using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDocuments;

public partial class AppDocumentsDataSeedContributor
{
    private List<PageSeed> GetGolfTeeTimesPages() => new()
    {
        new PageSeed
        {
            Slug = "gioi-thieu",
            Title = "Giới thiệu",
            DisplayOrder = 1,
            ContentHtml = @"<h2>Sân golf &amp; Giờ chơi</h2>
<p>Chuyên mục <strong>Sân golf &amp; Giờ chơi</strong> là trung tâm quản lý toàn bộ hoạt động vận hành sân golf trên hệ thống. Tại đây, quản trị viên có thể thiết lập thông tin sân golf, cấu hình lịch chơi (tee time), phân loại khách hàng, quản lý chương trình khuyến mãi và các ngày đặc biệt ảnh hưởng đến giá.</p>

<h3>Tổng quan các chức năng</h3>

<h4>1. Quản lý sân golf</h4>
<p>Tạo và quản lý thông tin các sân golf trong hệ thống. Mỗi sân golf là đơn vị cơ sở chính, được gắn với lịch chơi, loại khách hàng và chính sách khuyến mãi. Thông tin bao gồm: mã sân, tên, địa chỉ, số điện thoại, website, mô tả chi tiết và trạng thái hoạt động.</p>

<h4>2. Chính sách khuyến mãi</h4>
<p>Cấu hình chính sách hoãn/hủy booking theo từng tổ hợp (Sân golf + Loại khuyến mãi). Chính sách bao gồm: tiêu đề, số giờ hủy trước giờ chơi (ngày thường và cuối tuần), nội dung chi tiết hiển thị cho khách hàng trên Mini App khi đặt chỗ.</p>

<h4>3. Loại khách hàng</h4>
<p>Phân loại khách hàng theo các nhóm khác nhau (Visitor, Member, Member Guest...) với mức giá gốc riêng biệt. Hỗ trợ cấu hình giá theo loại ngày: Weekday, Weekend, Holiday, MemberDay. Mỗi loại có mã màu riêng để dễ nhận biết trên giao diện.</p>

<h4>4. Loại khuyến mãi</h4>
<p>Quản lý các loại chương trình khuyến mãi/deal áp dụng cho lịch chơi golf như: Early Bird, Twilight, Weekend Special, Holiday Deal... Mỗi loại khuyến mãi được gắn vào Calendar Slot để xác định mức giá và chính sách áp dụng.</p>

<h4>5. Lịch chơi (Calendar Slots)</h4>
<p>Chức năng quan trọng nhất để vận hành đặt chỗ. Quản lý các khung giờ chơi golf (tee time) với đầy đủ thông tin: ngày chơi, giờ bắt đầu/kết thúc, loại khuyến mãi, số chỗ tối đa, trạng thái. Hỗ trợ import hàng loạt từ Excel, quản lý dạng lịch (Calendar View), và thao tác hàng loạt (bulk activate/deactivate/delete).</p>

<h4>6. Ngày đặc biệt</h4>
<p>Cấu hình các loại ngày đặc biệt (Holiday, Weekend, MemberDay) để hệ thống tự động áp dụng mức giá tương ứng. Quy tắc ưu tiên: Holiday &gt; MemberDay &gt; Weekend &gt; Weekday. Hỗ trợ chọn ngày cụ thể (Holiday) hoặc thứ trong tuần (MemberDay, Weekend).</p>",
            FeatureName = FeatGolfCourse,
            TenantPermissionName = PermAppGolfCourses,
            HostPermissionName = PermHostAppGolfCourses
        },
        new PageSeed
        {
            Slug = "san-golf",
            Title = "Quản lý sân golf",
            DisplayOrder = 2,
            ContentHtml = @"<h2>Quản lý sân golf</h2>
<p>Trang <strong>Quản lý sân golf</strong> cho phép tạo và quản lý thông tin các sân golf trong hệ thống. Mỗi sân golf là đơn vị cơ sở chính để gắn lịch chơi, loại khách hàng và chính sách khuyến mãi.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị tất cả sân golf với mã, tên, tỉnh/thành, số điện thoại, website, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo sân golf mới với đầy đủ thông tin (mã, tên, địa chỉ, mô tả chi tiết)</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin sân golf đã tạo</li>
<li><strong>Xóa:</strong> Xóa sân golf không còn sử dụng</li>
<li><strong>Mô tả chi tiết:</strong> Soạn thảo mô tả sân golf bằng trình soạn thảo văn bản (Summernote)</li>
<li><strong>Trạng thái:</strong> Bật/tắt trạng thái hoạt động của sân golf</li>
</ul>
<h3>Thông tin hiển thị</h3>
<table class=""table table-bordered"">
<thead><tr><th>Cột</th><th>Mô tả</th></tr></thead>
<tbody>
<tr><td>Mã sân golf</td><td>Mã định danh duy nhất</td></tr>
<tr><td>Tên sân golf</td><td>Tên hiển thị</td></tr>
<tr><td>Tỉnh/Thành</td><td>Vị trí địa lý</td></tr>
<tr><td>Số điện thoại</td><td>Liên hệ</td></tr>
<tr><td>Website</td><td>Trang web sân golf</td></tr>
<tr><td>Trạng thái</td><td>Đang hoạt động / Ngừng hoạt động</td></tr>
</tbody>
</table>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Quản lý sân golf sẽ được bổ sung</em></p></div>",
            FeatureName = FeatGolfCourse,
            TenantPermissionName = PermAppGolfCourses,
            HostPermissionName = PermHostAppGolfCourses
        },
        new PageSeed
        {
            Slug = "chinh-sach-khuyen-mai",
            Title = "Chính sách khuyến mãi",
            DisplayOrder = 3,
            ContentHtml = @"<h2>Chính sách khuyến mãi</h2>
<p>Trang <strong>Chính sách khuyến mãi</strong> cho phép cấu hình chính sách hoãn/hủy booking theo từng sân golf và loại khuyến mãi. Chính sách này sẽ hiển thị cho khách hàng khi đặt chỗ trên Mini App.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị các chính sách với sân golf, loại khuyến mãi, tiêu đề, số giờ hủy</li>
<li><strong>Thêm mới:</strong> Tạo chính sách mới cho tổ hợp (Sân golf + Loại khuyến mãi)</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật nội dung chính sách</li>
<li><strong>Xóa:</strong> Xóa chính sách không còn áp dụng</li>
<li><strong>Nội dung chi tiết:</strong> Soạn thảo nội dung chính sách bằng trình soạn thảo văn bản</li>
</ul>
<h3>Thông tin cấu hình</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trường</th><th>Mô tả</th></tr></thead>
<tbody>
<tr><td>Sân golf</td><td>Sân golf áp dụng chính sách</td></tr>
<tr><td>Loại khuyến mãi</td><td>Loại deal áp dụng (Early Bird, Twilight...)</td></tr>
<tr><td>Tiêu đề chính sách</td><td>Tên hiển thị cho khách hàng</td></tr>
<tr><td>Số giờ hủy (ngày thường)</td><td>Thời hạn hủy trước giờ chơi (ngày thường)</td></tr>
<tr><td>Số giờ hủy (cuối tuần)</td><td>Thời hạn hủy trước giờ chơi (cuối tuần/lễ)</td></tr>
<tr><td>Nội dung</td><td>Chi tiết chính sách hiển thị trên Mini App</td></tr>
</tbody>
</table>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Chính sách khuyến mãi sẽ được bổ sung</em></p></div>",
            FeatureName = FeatGolfCourse,
            TenantPermissionName = PermAppPromotionPolicies,
            HostPermissionName = PermHostAppPromotionPolicies
        },
        new PageSeed
        {
            Slug = "loai-khach-hang",
            Title = "Loại khách hàng",
            DisplayOrder = 4,
            ContentHtml = @"<h2>Loại khách hàng</h2>
<p>Trang <strong>Loại khách hàng</strong> cho phép phân loại khách hàng theo các nhóm khác nhau (Visitor, Member, Member Guest...) với mức giá gốc riêng biệt cho từng loại ngày.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị các loại khách hàng với mã, tên, giá gốc, mã màu, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo loại khách hàng mới</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin loại khách hàng</li>
<li><strong>Xóa:</strong> Xóa loại khách hàng không còn sử dụng</li>
<li><strong>Giá theo loại ngày:</strong> Cấu hình giá gốc riêng cho Weekday, Weekend, Holiday, MemberDay</li>
</ul>
<h3>Thông tin hiển thị</h3>
<table class=""table table-bordered"">
<thead><tr><th>Cột</th><th>Mô tả</th></tr></thead>
<tbody>
<tr><td>Mã loại KH</td><td>Mã định danh (VIS, MEM, MBG...)</td></tr>
<tr><td>Tên loại KH</td><td>Tên hiển thị</td></tr>
<tr><td>Giá gốc</td><td>Giá mặc định (ngày thường)</td></tr>
<tr><td>Mã màu</td><td>Màu hiển thị trên giao diện</td></tr>
<tr><td>Trạng thái</td><td>Đang hoạt động / Ngừng</td></tr>
</tbody>
</table>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Loại khách hàng sẽ được bổ sung</em></p></div>",
            FeatureName = FeatGolfCourse,
            TenantPermissionName = PermAppCustomerTypes,
            HostPermissionName = PermHostAppCustomerTypes
        },
        new PageSeed
        {
            Slug = "loai-khuyen-mai",
            Title = "Loại khuyến mãi",
            DisplayOrder = 5,
            ContentHtml = @"<h2>Loại khuyến mãi</h2>
<p>Trang <strong>Loại khuyến mãi</strong> cho phép quản lý các loại chương trình khuyến mãi/deal áp dụng cho lịch chơi golf (Early Bird, Twilight, Weekend Special...).</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị các loại khuyến mãi với mã, tên, mô tả, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo loại khuyến mãi mới</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin</li>
<li><strong>Xóa:</strong> Xóa loại khuyến mãi không còn sử dụng</li>
</ul>
<h3>Lưu ý</h3>
<p>Loại khuyến mãi được gắn vào từng Calendar Slot để xác định mức giá và chính sách áp dụng cho khung giờ đó.</p>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Loại khuyến mãi sẽ được bổ sung</em></p></div>",
            FeatureName = FeatGolfCourse,
            TenantPermissionName = PermAppPromotionTypes,
            HostPermissionName = PermHostAppPromotionTypes
        },
        new PageSeed
        {
            Slug = "lich-choi",
            Title = "Lịch chơi (Calendar Slots)",
            DisplayOrder = 6,
            ContentHtml = @"<h2>Lịch chơi (Calendar Slots)</h2>
<p>Trang <strong>Lịch chơi</strong> cho phép quản lý các khung giờ chơi golf (tee time). Đây là chức năng quan trọng nhất để vận hành đặt chỗ trên Mini App.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị các slot với ngày chơi, giờ, loại khuyến mãi, số chỗ tối đa, ghi chú, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo slot chơi golf mới</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin slot</li>
<li><strong>Import Excel:</strong> Nhập hàng loạt slot từ file Excel</li>
<li><strong>Download Template:</strong> Tải file mẫu Excel để import</li>
<li><strong>Quản lý lịch (Calendar View):</strong> Xem và quản lý slot trên giao diện lịch</li>
<li><strong>Chọn nhiều slot:</strong> Tick chọn nhiều slot cùng lúc để thao tác hàng loạt</li>
<li><strong>Cập nhật trạng thái hàng loạt:</strong> Kích hoạt/Vô hiệu hóa nhiều slot đã chọn</li>
<li><strong>Xóa hàng loạt:</strong> Xóa nhiều slot đã chọn</li>
</ul>
<h3>Bộ lọc</h3>
<ul>
<li>Sân golf (bắt buộc chọn)</li>
<li>Từ ngày — Đến ngày</li>
</ul>
<h3>Lưu ý quan trọng</h3>
<ul>
<li>Phải chọn Sân golf trước khi xem danh sách slot</li>
<li>Slot có trạng thái Active mới hiển thị trên Mini App</li>
<li>Số chỗ còn lại (SlotAvailable) tự động giảm khi có booking mới</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Lịch chơi sẽ được bổ sung</em></p></div>",
            FeatureName = FeatGolfCourse,
            TenantPermissionName = PermAppCalendarSlots,
            HostPermissionName = PermHostAppCalendarSlots
        },
        new PageSeed
        {
            Slug = "ngay-dac-biet",
            Title = "Ngày đặc biệt",
            DisplayOrder = 7,
            ContentHtml = @"<h2>Ngày đặc biệt</h2>
<p>Trang <strong>Ngày đặc biệt</strong> cho phép cấu hình các loại ngày đặc biệt (Holiday, Weekend, MemberDay) để hệ thống tự động áp dụng mức giá tương ứng khi khách đặt chỗ.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị các ngày đặc biệt với tên, mô tả, ngày áp dụng, sân golf, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo cấu hình ngày đặc biệt mới</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin</li>
<li><strong>Xóa:</strong> Xóa cấu hình không còn sử dụng</li>
</ul>
<h3>Loại ngày đặc biệt</h3>
<table class=""table table-bordered"">
<thead><tr><th>Loại</th><th>Mô tả</th><th>Ưu tiên</th></tr></thead>
<tbody>
<tr><td>Holiday</td><td>Ngày lễ (chọn ngày cụ thể)</td><td>Cao nhất</td></tr>
<tr><td>MemberDay</td><td>Ngày hội viên (chọn thứ trong tuần)</td><td>Cao</td></tr>
<tr><td>Weekend</td><td>Cuối tuần (Thứ 7, Chủ nhật)</td><td>Trung bình</td></tr>
<tr><td>Weekday</td><td>Ngày thường (mặc định)</td><td>Thấp nhất</td></tr>
</tbody>
</table>
<p><strong>Quy tắc ưu tiên:</strong> Holiday &gt; MemberDay &gt; Weekend &gt; Weekday. Hệ thống sẽ áp dụng giá của loại ngày có ưu tiên cao nhất.</p>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Ngày đặc biệt sẽ được bổ sung</em></p></div>",
            FeatureName = FeatGolfCourse,
            TenantPermissionName = PermAppSpecialDates,
            HostPermissionName = PermHostAppSpecialDates
        }
    };
}
