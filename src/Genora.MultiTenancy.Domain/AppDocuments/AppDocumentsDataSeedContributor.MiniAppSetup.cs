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
<p>Chuyên mục <strong>Cài đặt Mini App</strong> cho phép quản trị viên cấu hình các thông số cơ bản cho ứng dụng Zalo Mini App, bao gồm thông tin hiển thị, phương thức thanh toán, giao diện trang chủ và kết nối tài khoản Zalo OA.</p>
<h3>Các chức năng trong chuyên mục</h3>
<ul>
<li><strong>Cài đặt chung</strong> — Cấu hình thông tin cơ bản của Mini App (tên, logo, thông tin liên hệ...)</li>
<li><strong>Cấu hình thanh toán</strong> — Thiết lập tài khoản ngân hàng, mã QR thanh toán</li>
<li><strong>Cấu hình trang chủ</strong> — Quản lý banner, menu và bố cục trang chủ Mini App</li>
<li><strong>Kết nối Zalo</strong> — Liên kết tài khoản Zalo OA, quản lý token và xem nhật ký gửi tin</li>
</ul>",
            FeatureName = FeatAppSettings,
            TenantPermissionName = PermAppSettings,
            HostPermissionName = PermHostAppSettings
        },
        new PageSeed
        {
            Slug = "cai-dat-chung",
            Title = "Cài đặt chung",
            DisplayOrder = 2,
            ContentHtml = @"<h2>Cài đặt chung</h2>
<p>Trang <strong>Cài đặt chung</strong> cho phép cấu hình các thông tin cơ bản hiển thị trên Zalo Mini App của doanh nghiệp.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Thông tin doanh nghiệp:</strong> Cấu hình tên, địa chỉ, số điện thoại, email liên hệ</li>
<li><strong>Logo &amp; Hình ảnh:</strong> Upload logo, ảnh bìa hiển thị trên Mini App</li>
<li><strong>Cấu hình thanh toán:</strong> Bật/tắt phương thức thanh toán tại quầy, chuyển khoản ngân hàng</li>
<li><strong>Cấu hình ZNS:</strong> Thiết lập template ID cho các loại tin nhắn ZNS tự động</li>
<li><strong>Cấu hình đặt chỗ:</strong> Thiết lập ServiceReview Template ID để gửi đánh giá sau dịch vụ</li>
</ul>
<h3>Hướng dẫn sử dụng</h3>
<ol>
<li>Truy cập menu <strong>Cài đặt Mini App → Cài đặt chung</strong></li>
<li>Điền đầy đủ thông tin vào các trường tương ứng</li>
<li>Nhấn <strong>Lưu</strong> để cập nhật cấu hình</li>
</ol>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Cài đặt chung sẽ được bổ sung</em></p></div>",
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
<li><strong>Thêm tài khoản thanh toán:</strong> Nhập thông tin ngân hàng, số tài khoản, tên chủ tài khoản</li>
<li><strong>Mã QR thanh toán:</strong> Tự động tạo mã VietQR từ thông tin tài khoản</li>
<li><strong>Bật/tắt tài khoản:</strong> Kích hoạt hoặc vô hiệu hóa từng tài khoản thanh toán</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin tài khoản đã tạo</li>
<li><strong>Xóa:</strong> Xóa tài khoản thanh toán không còn sử dụng</li>
</ul>
<h3>Hướng dẫn sử dụng</h3>
<ol>
<li>Truy cập menu <strong>Cài đặt Mini App → Cấu hình thanh toán</strong></li>
<li>Nhấn <strong>Thêm mới</strong> để tạo tài khoản thanh toán</li>
<li>Điền thông tin: Ngân hàng, Số tài khoản, Tên chủ tài khoản</li>
<li>Nhấn <strong>Lưu</strong> để hoàn tất</li>
</ol>
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
</ul>
<h3>Hướng dẫn sử dụng</h3>
<ol>
<li>Truy cập menu <strong>Cài đặt Mini App → Cấu hình trang chủ</strong></li>
<li>Chọn khối nội dung cần chỉnh sửa</li>
<li>Cập nhật hình ảnh, liên kết, thứ tự hiển thị</li>
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
            Title = "Kết nối Zalo",
            DisplayOrder = 5,
            ContentHtml = @"<h2>Kết nối Zalo</h2>
<p>Trang <strong>Kết nối Zalo</strong> cho phép liên kết tài khoản Zalo Official Account (OA) với hệ thống, quản lý token xác thực và theo dõi nhật ký gửi tin nhắn ZNS/ZBS.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Kết nối Zalo OA:</strong> Xác thực và liên kết tài khoản Zalo OA với hệ thống</li>
<li><strong>Quản lý Token:</strong> Xem trạng thái token, làm mới token khi hết hạn</li>
<li><strong>Nhật ký Zalo:</strong> Xem lịch sử gửi tin nhắn ZNS (thông báo), ZBS (chăm sóc khách hàng)</li>
<li><strong>Trạng thái gửi tin:</strong> Theo dõi tin nhắn thành công/thất bại</li>
</ul>
<h3>Hướng dẫn sử dụng</h3>
<ol>
<li>Truy cập menu <strong>Cài đặt Mini App → Zalo</strong></li>
<li>Nhấn <strong>Kết nối</strong> để xác thực tài khoản Zalo OA</li>
<li>Sau khi kết nối thành công, hệ thống sẽ tự động gửi tin nhắn ZNS theo cấu hình</li>
<li>Xem <strong>Nhật ký Zalo</strong> để theo dõi trạng thái gửi tin</li>
</ol>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Kết nối Zalo sẽ được bổ sung</em></p></div>",
            FeatureName = FeatAppSettings,
            TenantPermissionName = PermAppZaloAuths,
            HostPermissionName = PermHostAppZaloAuths
        }
    };
}
