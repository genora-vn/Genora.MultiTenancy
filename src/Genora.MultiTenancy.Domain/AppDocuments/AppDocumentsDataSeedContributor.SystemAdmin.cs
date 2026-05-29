using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDocuments;

public partial class AppDocumentsDataSeedContributor
{
    private List<PageSeed> GetSystemAdminPages() => new()
    {
        new PageSeed
        {
            Slug = "gioi-thieu",
            Title = "Giới thiệu",
            DisplayOrder = 1,
            ContentHtml = @"<h2>Quản trị hệ thống</h2>
<p>Chuyên mục <strong>Quản trị hệ thống</strong> cho phép quản lý người dùng, phân quyền, cấu hình email và theo dõi nhật ký hoạt động của hệ thống.</p>
<h3>Các chức năng trong chuyên mục</h3>
<ul>
<li><strong>Vai trò</strong> — Quản lý vai trò và phân quyền</li>
<li><strong>Người dùng</strong> — Quản lý tài khoản người dùng hệ thống</li>
<li><strong>Cấu hình Email</strong> — Thiết lập SMTP gửi email</li>
<li><strong>Zalo ZNS/ZBS</strong> — Cấu hình template tin nhắn Zalo</li>
<li><strong>Mẫu Email</strong> — Quản lý nội dung mẫu email tự động</li>
<li><strong>Nhật ký Email</strong> — Theo dõi lịch sử gửi email</li>
</ul>",
            FeatureName = null,
            TenantPermissionName = null,
            HostPermissionName = null
        },
        new PageSeed
        {
            Slug = "vai-tro",
            Title = "Vai trò (Roles)",
            DisplayOrder = 2,
            ContentHtml = @"<h2>Vai trò (Roles)</h2>
<p>Trang <strong>Vai trò</strong> cho phép quản lý các vai trò trong hệ thống và phân quyền chi tiết cho từng vai trò. Mỗi người dùng có thể được gán một hoặc nhiều vai trò.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị các vai trò với tên, loại (static/dynamic)</li>
<li><strong>Thêm mới:</strong> Tạo vai trò mới</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật tên vai trò</li>
<li><strong>Phân quyền:</strong> Gán/bỏ quyền chi tiết cho vai trò</li>
<li><strong>Xóa:</strong> Xóa vai trò (trừ vai trò mặc định admin)</li>
</ul>
<h3>Hệ thống phân quyền</h3>
<p>Quyền được tổ chức theo nhóm chức năng:</p>
<ul>
<li>Mỗi module có nhóm quyền riêng (Xem, Tạo, Sửa, Xóa)</li>
<li>Quyền được kiểm soát bởi Feature (tính năng đã bật) + Permission (quyền được gán)</li>
<li>Vai trò <strong>admin</strong> mặc định có tất cả quyền</li>
</ul>
<h3>Hướng dẫn tạo vai trò mới</h3>
<ol>
<li>Nhấn <strong>Thêm mới</strong> để tạo vai trò</li>
<li>Nhập tên vai trò (ví dụ: ""Nhân viên bếp"", ""Quản lý Proshop"")</li>
<li>Sau khi tạo, nhấn <strong>Phân quyền</strong> để gán quyền phù hợp</li>
<li>Tick chọn các quyền cần thiết cho vai trò đó</li>
<li>Nhấn <strong>Lưu</strong> để áp dụng</li>
</ol>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Vai trò sẽ được bổ sung</em></p></div>",
            FeatureName = null,
            TenantPermissionName = null,
            HostPermissionName = null
        },
        new PageSeed
        {
            Slug = "nguoi-dung",
            Title = "Người dùng (Users)",
            DisplayOrder = 3,
            ContentHtml = @"<h2>Người dùng (Users)</h2>
<p>Trang <strong>Người dùng</strong> cho phép quản lý tài khoản đăng nhập hệ thống quản trị. Mỗi người dùng được gán vai trò để xác định quyền truy cập.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị người dùng với tên đăng nhập, email, SĐT, vai trò, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo tài khoản người dùng mới</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin người dùng</li>
<li><strong>Xóa:</strong> Xóa tài khoản (trừ admin mặc định)</li>
<li><strong>Gán vai trò:</strong> Gán một hoặc nhiều vai trò cho người dùng</li>
<li><strong>Đặt lại mật khẩu:</strong> Reset mật khẩu cho người dùng</li>
<li><strong>Khóa/Mở khóa:</strong> Khóa tài khoản tạm thời</li>
</ul>
<h3>Thông tin người dùng</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trường</th><th>Mô tả</th></tr></thead>
<tbody>
<tr><td>Tên đăng nhập</td><td>Username để đăng nhập hệ thống</td></tr>
<tr><td>Email</td><td>Email liên hệ (dùng để reset mật khẩu)</td></tr>
<tr><td>Số điện thoại</td><td>SĐT liên hệ</td></tr>
<tr><td>Họ tên</td><td>Tên hiển thị</td></tr>
<tr><td>Vai trò</td><td>Các vai trò được gán</td></tr>
<tr><td>Trạng thái</td><td>Hoạt động / Bị khóa</td></tr>
</tbody>
</table>
<h3>Hướng dẫn tạo người dùng mới</h3>
<ol>
<li>Nhấn <strong>Thêm mới</strong></li>
<li>Nhập thông tin: tên đăng nhập, email, mật khẩu, họ tên</li>
<li>Chọn vai trò phù hợp</li>
<li>Nhấn <strong>Lưu</strong> để tạo tài khoản</li>
<li>Gửi thông tin đăng nhập cho người dùng</li>
</ol>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Người dùng sẽ được bổ sung</em></p></div>",
            FeatureName = null,
            TenantPermissionName = null,
            HostPermissionName = null
        },
        new PageSeed
        {
            Slug = "cau-hinh-email",
            Title = "Cấu hình Email",
            DisplayOrder = 4,
            ContentHtml = @"<h2>Cấu hình Email</h2>
<p>Trang <strong>Cấu hình Email</strong> cho phép thiết lập thông tin SMTP server để hệ thống gửi email tự động (thông báo booking, xác nhận đơn hàng, reset mật khẩu...).</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Cấu hình SMTP:</strong> Thiết lập host, port, username, password</li>
<li><strong>Email gửi đi:</strong> Cấu hình địa chỉ email và tên hiển thị khi gửi</li>
<li><strong>Gửi email test:</strong> Kiểm tra cấu hình bằng cách gửi email thử</li>
</ul>
<h3>Thông tin cấu hình</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trường</th><th>Mô tả</th><th>Ví dụ</th></tr></thead>
<tbody>
<tr><td>SMTP Host</td><td>Địa chỉ server SMTP</td><td>smtp.gmail.com</td></tr>
<tr><td>SMTP Port</td><td>Cổng kết nối</td><td>587</td></tr>
<tr><td>Username</td><td>Tài khoản đăng nhập SMTP</td><td>noreply@company.com</td></tr>
<tr><td>Password</td><td>Mật khẩu ứng dụng</td><td>***</td></tr>
<tr><td>Enable SSL</td><td>Bật mã hóa SSL/TLS</td><td>Có</td></tr>
<tr><td>Sender Email</td><td>Địa chỉ email gửi đi</td><td>noreply@company.com</td></tr>
<tr><td>Sender Name</td><td>Tên hiển thị</td><td>Genora System</td></tr>
</tbody>
</table>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Cấu hình Email sẽ được bổ sung</em></p></div>",
            FeatureName = null,
            TenantPermissionName = null,
            HostPermissionName = null
        },
        new PageSeed
        {
            Slug = "zalo-zns-zbs",
            Title = "Zalo ZNS/ZBS",
            DisplayOrder = 5,
            ContentHtml = @"<h2>Zalo ZNS/ZBS</h2>
<p>Trang <strong>Zalo ZNS/ZBS</strong> cho phép cấu hình các template tin nhắn Zalo tự động gửi cho khách hàng trong các sự kiện quan trọng.</p>
<h3>Các loại tin nhắn</h3>
<table class=""table table-bordered"">
<thead><tr><th>Loại</th><th>Mô tả</th><th>Khi nào gửi</th></tr></thead>
<tbody>
<tr><td>ZNS (Zalo Notification Service)</td><td>Tin nhắn thông báo</td><td>Xác nhận booking, thông báo thanh toán</td></tr>
<tr><td>ZBS (Zalo Business Service)</td><td>Tin nhắn chăm sóc</td><td>Nhắc lịch hẹn, đánh giá dịch vụ</td></tr>
</tbody>
</table>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Cấu hình Template ID:</strong> Gán mã template Zalo cho từng loại sự kiện</li>
<li><strong>Bật/tắt gửi tin:</strong> Kích hoạt hoặc tắt gửi tin cho từng sự kiện</li>
<li><strong>Xem mẫu tin:</strong> Preview nội dung template</li>
</ul>
<h3>Các sự kiện gửi tin tự động</h3>
<ul>
<li><strong>Booking Created:</strong> Khi có booking mới (Golf/Salon)</li>
<li><strong>Booking Confirmed:</strong> Khi booking được xác nhận</li>
<li><strong>Service Review:</strong> Khi dịch vụ hoàn thành, gửi link đánh giá</li>
<li><strong>Payment Success:</strong> Khi thanh toán thành công</li>
</ul>
<h3>Lưu ý</h3>
<ul>
<li>Template ID phải được tạo và duyệt trên Zalo OA trước khi sử dụng</li>
<li>Các tham số template (customer_name, booking_code...) được hệ thống tự động điền</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Zalo ZNS/ZBS sẽ được bổ sung</em></p></div>",
            FeatureName = null,
            TenantPermissionName = null,
            HostPermissionName = null
        },
        new PageSeed
        {
            Slug = "mau-email",
            Title = "Mẫu Email",
            DisplayOrder = 6,
            ContentHtml = @"<h2>Mẫu Email (Email Templates)</h2>
<p>Trang <strong>Mẫu Email</strong> cho phép quản lý nội dung các mẫu email tự động gửi cho khách hàng và quản trị viên.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị các mẫu email với tên, sự kiện kích hoạt, trạng thái</li>
<li><strong>Chỉnh sửa nội dung:</strong> Soạn thảo nội dung email bằng trình soạn thảo HTML</li>
<li><strong>Biến động (Variables):</strong> Sử dụng biến Scriban để chèn thông tin động</li>
<li><strong>Bật/tắt:</strong> Kích hoạt hoặc tắt gửi email cho từng mẫu</li>
</ul>
<h3>Các mẫu email phổ biến</h3>
<ul>
<li><strong>Booking New Request:</strong> Thông báo có booking mới cho quản trị viên</li>
<li><strong>Booking Confirmed:</strong> Xác nhận booking cho khách hàng</li>
<li><strong>Order Created:</strong> Thông báo đơn hàng mới</li>
<li><strong>Payment Success:</strong> Xác nhận thanh toán thành công</li>
</ul>
<h3>Biến Scriban thường dùng</h3>
<table class=""table table-bordered"">
<thead><tr><th>Biến</th><th>Mô tả</th></tr></thead>
<tbody>
<tr><td>{{ customer_name }}</td><td>Tên khách hàng</td></tr>
<tr><td>{{ booking_code }}</td><td>Mã booking</td></tr>
<tr><td>{{ total_amount }}</td><td>Tổng tiền</td></tr>
<tr><td>{{ play_date }}</td><td>Ngày chơi/hẹn</td></tr>
</tbody>
</table>
<h3>Lưu ý</h3>
<ul>
<li>Sử dụng cú pháp Scriban: <code>{{ variable_name }}</code></li>
<li>Kiểm tra null bằng <code>!= null</code> (không dùng <code>!= empty</code>)</li>
<li>Gửi email test trước khi kích hoạt chính thức</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Mẫu Email sẽ được bổ sung</em></p></div>",
            FeatureName = null,
            TenantPermissionName = null,
            HostPermissionName = null
        },
        new PageSeed
        {
            Slug = "nhat-ky-email",
            Title = "Nhật ký Email",
            DisplayOrder = 7,
            ContentHtml = @"<h2>Nhật ký Email</h2>
<p>Trang <strong>Nhật ký Email</strong> cho phép theo dõi lịch sử tất cả email đã gửi từ hệ thống, bao gồm trạng thái gửi thành công/thất bại.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị email đã gửi với người nhận, tiêu đề, thời gian, trạng thái</li>
<li><strong>Xem chi tiết:</strong> Xem nội dung email đã gửi</li>
<li><strong>Lọc theo trạng thái:</strong> Xem email thành công hoặc thất bại</li>
<li><strong>Gửi lại:</strong> Gửi lại email bị thất bại</li>
</ul>
<h3>Thông tin hiển thị</h3>
<table class=""table table-bordered"">
<thead><tr><th>Cột</th><th>Mô tả</th></tr></thead>
<tbody>
<tr><td>Người nhận</td><td>Địa chỉ email nhận</td></tr>
<tr><td>Tiêu đề</td><td>Subject của email</td></tr>
<tr><td>Thời gian gửi</td><td>Ngày giờ gửi email</td></tr>
<tr><td>Trạng thái</td><td>Thành công / Thất bại</td></tr>
<tr><td>Lỗi</td><td>Thông tin lỗi (nếu thất bại)</td></tr>
</tbody>
</table>
<h3>Hướng dẫn xử lý email thất bại</h3>
<ol>
<li>Lọc danh sách theo trạng thái <strong>Thất bại</strong></li>
<li>Kiểm tra thông tin lỗi để xác định nguyên nhân</li>
<li>Nếu do cấu hình SMTP: sửa lại tại <strong>Cấu hình Email</strong></li>
<li>Nếu do địa chỉ email sai: liên hệ khách hàng cập nhật</li>
<li>Nhấn <strong>Gửi lại</strong> sau khi khắc phục</li>
</ol>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Nhật ký Email sẽ được bổ sung</em></p></div>",
            FeatureName = null,
            TenantPermissionName = PermAppEmails,
            HostPermissionName = PermHostAppEmails
        }
    };
}
