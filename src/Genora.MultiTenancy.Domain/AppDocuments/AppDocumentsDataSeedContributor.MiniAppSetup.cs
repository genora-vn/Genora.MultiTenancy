using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDocuments;

public partial class AppDocumentsDataSeedContributor
{
    private List<PageSeed> GetMiniAppSetupPages() => new()
    {
        new PageSeed
        {
            Slug = "gioi-thieu",
            Title = "Giới thiệu",
            DisplayOrder = 1,
            ContentHtml = @"<h2>Cài đặt Mini App</h2>
<p>Chuyên mục <strong>Cài đặt Mini App</strong> là trung tâm cấu hình toàn bộ ứng dụng Zalo Mini App của doanh nghiệp. Tại đây, quản trị viên thiết lập các thông số cơ bản, giao diện, phương thức thanh toán và kết nối với nền tảng Zalo OA.</p>

<h3>Tổng quan các chức năng</h3>

<h4>1. Cấu hình chung</h4>
<p>Thiết lập thông tin cơ bản của Mini App: tên doanh nghiệp, logo, ảnh bìa, thông tin liên hệ (địa chỉ, số điện thoại, email). Ngoài ra còn cấu hình các template ID cho tin nhắn ZNS/ZBS tự động, bật/tắt phương thức thanh toán (tại quầy, chuyển khoản), và thiết lập ServiceReview Template ID để gửi đánh giá sau dịch vụ.</p>

<h4>2. Cấu hình trang chủ</h4>
<p>Quản lý giao diện trang chủ Mini App: banner quảng cáo, menu điều hướng chính, các khối nội dung hiển thị. Hỗ trợ sắp xếp thứ tự, bật/tắt từng khối, upload hình ảnh banner và cấu hình liên kết điều hướng.</p>

<h4>3. Cấu hình thanh toán</h4>
<p>Thiết lập thông tin tài khoản ngân hàng để khách hàng thanh toán qua Mini App. Hỗ trợ tạo nhiều tài khoản, tự động sinh mã VietQR, bật/tắt từng tài khoản. Mã QR được hiển thị trên trang thanh toán của Mini App để khách chuyển khoản nhanh.</p>

<h4>4. Tích hợp Zalo OA</h4>
<p>Kết nối tài khoản Zalo Official Account (OA) với hệ thống. Sau khi kết nối, hệ thống có thể gửi tin nhắn ZNS (thông báo giao dịch) và ZBS (chăm sóc khách hàng) tự động cho khách hàng. Quản lý token xác thực, làm mới token khi hết hạn.</p>

<h4>5. Nhật ký Zalo</h4>
<p>Theo dõi lịch sử tất cả tin nhắn ZNS/ZBS đã gửi qua Zalo OA. Xem trạng thái gửi thành công/thất bại, nội dung tin nhắn, thời gian gửi và thông tin người nhận. Hỗ trợ debug khi tin nhắn không gửi được.</p>",
            FeatureName = FeatAppSettings,
            TenantPermissionName = PermAppSettings,
            HostPermissionName = PermHostAppSettings
        },
        new PageSeed
        {
            Slug = "cai-dat-chung",
            Title = "Cấu hình chung",
            DisplayOrder = 2,
            ContentHtml = @"<h2>Cấu hình chung</h2>
<p>Trang <strong>Cấu hình chung</strong> cho phép thiết lập các thông tin cơ bản hiển thị trên Zalo Mini App và cấu hình hệ thống gửi tin nhắn tự động.</p>

<h3>Các nhóm cấu hình</h3>

<h4>Thông tin doanh nghiệp</h4>
<ul>
<li><strong>Tên doanh nghiệp:</strong> Tên hiển thị trên Mini App</li>
<li><strong>Logo:</strong> Upload logo doanh nghiệp</li>
<li><strong>Ảnh bìa:</strong> Hình ảnh banner chính</li>
<li><strong>Địa chỉ:</strong> Địa chỉ liên hệ</li>
<li><strong>Số điện thoại:</strong> Hotline hỗ trợ</li>
<li><strong>Email:</strong> Email liên hệ</li>
</ul>

<h4>Cấu hình thanh toán</h4>
<ul>
<li><strong>Thanh toán tại quầy (IsPayAtCounterEnabled):</strong> Bật/tắt phương thức thanh toán trực tiếp</li>
<li><strong>Chuyển khoản ngân hàng (IsPayBankTransferEnabled):</strong> Bật/tắt phương thức chuyển khoản</li>
</ul>

<h4>Cấu hình ZNS/ZBS Template</h4>
<ul>
<li><strong>Booking Created Template ID:</strong> Mẫu tin gửi khi có booking mới</li>
<li><strong>Booking Confirmed Template ID:</strong> Mẫu tin gửi khi xác nhận booking</li>
<li><strong>ServiceReview Template ID:</strong> Mẫu tin gửi link đánh giá sau dịch vụ</li>
</ul>

<h3>Hướng dẫn sử dụng</h3>
<ol>
<li>Truy cập menu <strong>Cài đặt Mini App → Cài đặt chung</strong></li>
<li>Điền đầy đủ thông tin vào các trường tương ứng</li>
<li>Upload logo và ảnh bìa (định dạng PNG/JPG, khuyến nghị kích thước phù hợp)</li>
<li>Bật/tắt phương thức thanh toán theo nhu cầu</li>
<li>Nhập Template ID từ Zalo OA (nếu đã tạo template)</li>
<li>Nhấn <strong>Lưu</strong> để cập nhật cấu hình</li>
</ol>

<h3>Lưu ý</h3>
<ul>
<li>Template ID phải được tạo và duyệt trên Zalo OA trước khi nhập vào hệ thống</li>
<li>Thay đổi cấu hình thanh toán sẽ ảnh hưởng ngay lập tức đến Mini App</li>
<li>Logo khuyến nghị kích thước vuông (512x512px), ảnh bìa khuyến nghị 16:9</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Cấu hình chung sẽ được bổ sung</em></p></div>",
            FeatureName = FeatAppSettings,
            TenantPermissionName = PermAppSettings,
            HostPermissionName = PermHostAppSettings
        },
        new PageSeed
        {
            Slug = "cau-hinh-thanh-toan",
            Title = "Cấu hình thanh toán",
            DisplayOrder = 3,
            ContentHtml = @"<h2>Cấu hình thanh toán</h2>
<p>Trang <strong>Cấu hình thanh toán</strong> cho phép thiết lập thông tin tài khoản ngân hàng và mã QR để khách hàng thanh toán qua Mini App.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị các tài khoản thanh toán đã cấu hình</li>
<li><strong>Thêm tài khoản:</strong> Nhập thông tin ngân hàng, số tài khoản, tên chủ tài khoản</li>
<li><strong>Mã QR thanh toán:</strong> Tự động tạo mã VietQR từ thông tin tài khoản</li>
<li><strong>Bật/tắt tài khoản:</strong> Kích hoạt hoặc vô hiệu hóa từng tài khoản</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin tài khoản đã tạo</li>
<li><strong>Xóa:</strong> Xóa tài khoản thanh toán không còn sử dụng</li>
</ul>
<h3>Thông tin tài khoản</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trường</th><th>Mô tả</th><th>Bắt buộc</th></tr></thead>
<tbody>
<tr><td>Ngân hàng</td><td>Tên ngân hàng (VietcomBank, BIDV, Techcombank...)</td><td>Có</td></tr>
<tr><td>Số tài khoản</td><td>Số tài khoản ngân hàng</td><td>Có</td></tr>
<tr><td>Tên chủ tài khoản</td><td>Tên đăng ký tài khoản</td><td>Có</td></tr>
<tr><td>Chi nhánh</td><td>Chi nhánh ngân hàng</td><td>Không</td></tr>
</tbody>
</table>
<h3>Hướng dẫn sử dụng</h3>
<ol>
<li>Truy cập menu <strong>Cài đặt Mini App → Cấu hình thanh toán</strong></li>
<li>Nhấn <strong>Thêm mới</strong> để tạo tài khoản thanh toán</li>
<li>Điền thông tin: Ngân hàng, Số tài khoản, Tên chủ tài khoản</li>
<li>Nhấn <strong>Lưu</strong> để hoàn tất</li>
<li>Hệ thống tự động tạo mã VietQR hiển thị trên trang thanh toán Mini App</li>
</ol>
<h3>Lưu ý</h3>
<ul>
<li>Mã QR sử dụng chuẩn VietQR (https://dl.vietqr.io/pay) để tương thích với tất cả app ngân hàng</li>
<li>Có thể tạo nhiều tài khoản, chỉ tài khoản đang bật mới hiển thị trên Mini App</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Cấu hình thanh toán sẽ được bổ sung</em></p></div>",
            FeatureName = FeatAppSettings,
            TenantPermissionName = PermAppPaymentConfigurations,
            HostPermissionName = PermHostAppPaymentConfigurations
        },
        new PageSeed
        {
            Slug = "cau-hinh-trang-chu",
            Title = "Cấu hình trang chủ",
            DisplayOrder = 4,
            ContentHtml = @"<h2>Cấu hình trang chủ</h2>
<p>Trang <strong>Cấu hình trang chủ</strong> cho phép quản lý giao diện trang chủ của Zalo Mini App, bao gồm banner quảng cáo, menu điều hướng và các khối nội dung.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Quản lý Banner:</strong> Thêm, sửa, xóa banner quảng cáo hiển thị trên trang chủ</li>
<li><strong>Cấu hình Menu:</strong> Thiết lập các mục menu điều hướng chính</li>
<li><strong>Sắp xếp thứ tự:</strong> Kéo thả để sắp xếp thứ tự hiển thị các khối nội dung</li>
<li><strong>Bật/tắt khối:</strong> Ẩn/hiện từng khối nội dung trên trang chủ</li>
<li><strong>Upload hình ảnh:</strong> Tải lên hình ảnh cho banner và menu</li>
<li><strong>Cấu hình liên kết:</strong> Thiết lập URL điều hướng khi click vào banner/menu</li>
</ul>
<h3>Các khối nội dung trang chủ</h3>
<table class=""table table-bordered"">
<thead><tr><th>Khối</th><th>Mô tả</th><th>Cấu hình</th></tr></thead>
<tbody>
<tr><td>Banner Slider</td><td>Carousel hình ảnh quảng cáo</td><td>Hình ảnh + Liên kết</td></tr>
<tr><td>Menu nhanh</td><td>Các icon điều hướng chức năng</td><td>Icon + Tên + Liên kết</td></tr>
<tr><td>Sản phẩm nổi bật</td><td>Danh sách sản phẩm/dịch vụ hot</td><td>Tự động từ dữ liệu</td></tr>
<tr><td>Tin tức</td><td>Bài viết mới nhất</td><td>Tự động từ module Tin tức</td></tr>
</tbody>
</table>
<h3>Hướng dẫn sử dụng</h3>
<ol>
<li>Truy cập menu <strong>Cài đặt Mini App → Cấu hình trang chủ</strong></li>
<li>Chọn khối nội dung cần chỉnh sửa</li>
<li>Upload hình ảnh (khuyến nghị: Banner 750x400px, Menu icon 128x128px)</li>
<li>Cấu hình liên kết điều hướng</li>
<li>Sắp xếp thứ tự hiển thị</li>
<li>Nhấn <strong>Lưu</strong> để áp dụng thay đổi</li>
</ol>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Cấu hình trang chủ sẽ được bổ sung</em></p></div>",
            FeatureName = FeatAppSettings,
            TenantPermissionName = PermAppHomePageConfigs,
            HostPermissionName = PermHostAppHomePageConfigs
        },
        new PageSeed
        {
            Slug = "ket-noi-zalo",
            Title = "Tích hợp Zalo OA",
            DisplayOrder = 5,
            ContentHtml = @"<h2>Tích hợp Zalo OA</h2>
<p>Trang <strong>Tích hợp Zalo OA</strong> cho phép kết nối tài khoản Zalo Official Account (OA) với hệ thống, quản lý token xác thực và theo dõi nhật ký gửi tin nhắn ZNS/ZBS.</p>

<h3>Kết nối Zalo OA</h3>
<h4>Các tính năng chính</h4>
<ul>
<li><strong>Kết nối Zalo OA:</strong> Xác thực và liên kết tài khoản Zalo OA với hệ thống</li>
<li><strong>Quản lý Token:</strong> Xem trạng thái token, làm mới token khi hết hạn</li>
<li><strong>Thông tin OA:</strong> Hiển thị tên OA, ID, trạng thái kết nối</li>
</ul>

<h4>Hướng dẫn kết nối</h4>
<ol>
<li>Truy cập menu <strong>Cài đặt Mini App → Zalo → Kết nối Zalo</strong></li>
<li>Nhấn <strong>Kết nối</strong> để mở trang xác thực Zalo</li>
<li>Đăng nhập tài khoản Zalo quản trị OA</li>
<li>Cấp quyền cho ứng dụng</li>
<li>Sau khi kết nối thành công, hệ thống sẽ hiển thị thông tin OA</li>
</ol>

<h3>Nhật ký Zalo</h3>
<h4>Các tính năng chính</h4>
<ul>
<li><strong>Xem lịch sử:</strong> Danh sách tất cả tin nhắn ZNS/ZBS đã gửi</li>
<li><strong>Trạng thái gửi:</strong> Theo dõi tin nhắn thành công/thất bại</li>
<li><strong>Chi tiết tin nhắn:</strong> Xem nội dung, người nhận, thời gian gửi</li>
<li><strong>Mã lỗi:</strong> Xem mã lỗi Zalo khi gửi thất bại để debug</li>
</ul>

<h3>Các loại tin nhắn tự động</h3>
<table class=""table table-bordered"">
<thead><tr><th>Loại</th><th>Sự kiện kích hoạt</th><th>Nội dung</th></tr></thead>
<tbody>
<tr><td>ZNS - Booking Created</td><td>Khi có booking mới</td><td>Thông báo đặt chỗ thành công</td></tr>
<tr><td>ZNS - Booking Confirmed</td><td>Khi xác nhận booking</td><td>Xác nhận lịch hẹn</td></tr>
<tr><td>ZBS - Service Review</td><td>Khi dịch vụ hoàn thành</td><td>Gửi link đánh giá dịch vụ</td></tr>
<tr><td>ZNS - Payment Success</td><td>Khi thanh toán thành công</td><td>Xác nhận thanh toán</td></tr>
</tbody>
</table>

<h3>Lưu ý quan trọng</h3>
<ul>
<li>Token Zalo có thời hạn, cần làm mới định kỳ (hệ thống sẽ cảnh báo khi sắp hết hạn)</li>
<li>Template ZNS phải được tạo và duyệt trên Zalo OA Console trước khi sử dụng</li>
<li>Mỗi template có các tham số riêng (customer_name, booking_code, schedule_time...) được hệ thống tự động điền</li>
<li>Nếu gửi tin thất bại, kiểm tra: token còn hạn, template ID đúng, số điện thoại khách đã follow OA</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Tích hợp Zalo OA sẽ được bổ sung</em></p></div>",
            FeatureName = FeatAppSettings,
            TenantPermissionName = PermAppZaloAuths,
            HostPermissionName = PermHostAppZaloAuths
        }
    };
}
