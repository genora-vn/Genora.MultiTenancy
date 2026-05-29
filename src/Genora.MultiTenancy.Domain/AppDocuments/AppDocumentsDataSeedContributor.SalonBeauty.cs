using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDocuments;

public partial class AppDocumentsDataSeedContributor
{
    private List<PageSeed> GetSalonBeautyPages() => new()
    {
        new PageSeed
        {
            Slug = "gioi-thieu",
            Title = "Giới thiệu",
            DisplayOrder = 1,
            ContentHtml = @"<h2>Salon Beauty</h2>
<p>Chuyên mục <strong>Salon Beauty</strong> cho phép quản lý toàn bộ hoạt động kinh doanh Salon: dịch vụ, nhân viên, nạp tiền và chương trình tích điểm khách hàng thân thiết.</p>
<h3>Các chức năng trong chuyên mục</h3>
<ul>
<li><strong>Danh mục dịch vụ</strong> — Quản lý nhóm/danh mục dịch vụ Salon</li>
<li><strong>Dịch vụ</strong> — Quản lý từng dịch vụ với giá, thời gian, nhân viên phù hợp</li>
<li><strong>Nhân viên</strong> — Quản lý thông tin nhân viên/stylist</li>
<li><strong>Nạp tiền</strong> — Quản lý giao dịch nạp tiền khách hàng</li>
<li><strong>Cấu hình tích điểm</strong> — Thiết lập tỷ lệ quy đổi và bậc thưởng</li>
</ul>",
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyBookings,
            HostPermissionName = PermHostSalonBeautyBookings
        },
        new PageSeed
        {
            Slug = "danh-muc-dich-vu",
            Title = "Danh mục dịch vụ",
            DisplayOrder = 2,
            ContentHtml = @"<h2>Danh mục dịch vụ</h2>
<p>Trang <strong>Danh mục dịch vụ</strong> cho phép quản lý các nhóm/danh mục dịch vụ Salon Beauty (Cắt tóc, Nhuộm, Spa, Nail...).</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị danh mục với icon, tên, mô tả, thứ tự sắp xếp, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo danh mục dịch vụ mới</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin danh mục</li>
<li><strong>Xóa:</strong> Xóa danh mục không còn sử dụng</li>
<li><strong>Xem chi tiết:</strong> Modal xem thông tin chi tiết danh mục</li>
</ul>
<h3>Bộ lọc</h3>
<ul>
<li>Tìm kiếm theo từ khóa</li>
<li>Trạng thái (Hoạt động / Ngừng / Tất cả)</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Danh mục dịch vụ sẽ được bổ sung</em></p></div>",
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyServiceCategories,
            HostPermissionName = PermHostSalonBeautyServiceCategories
        },
        new PageSeed
        {
            Slug = "dich-vu",
            Title = "Dịch vụ",
            DisplayOrder = 3,
            ContentHtml = @"<h2>Dịch vụ</h2>
<p>Trang <strong>Dịch vụ</strong> cho phép quản lý từng dịch vụ Salon Beauty với đầy đủ thông tin: danh mục, giá, thời gian thực hiện, cấp độ nhân viên phù hợp.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị dịch vụ với danh mục, tên, thời gian, giá, vai trò NV, cấp độ NV, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo dịch vụ mới</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin dịch vụ</li>
<li><strong>Xóa:</strong> Xóa dịch vụ</li>
<li><strong>Xem chi tiết:</strong> Modal xem thông tin chi tiết</li>
<li><strong>Bật/tắt trạng thái:</strong> Toggle trạng thái hoạt động trực tiếp</li>
<li><strong>Hiển thị trên App:</strong> Toggle hiển thị dịch vụ trên Mini App</li>
</ul>
<h3>Thông tin dịch vụ</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trường</th><th>Mô tả</th></tr></thead>
<tbody>
<tr><td>Danh mục</td><td>Nhóm dịch vụ thuộc về</td></tr>
<tr><td>Tên dịch vụ</td><td>Tên hiển thị</td></tr>
<tr><td>Thời gian (phút)</td><td>Thời gian thực hiện dịch vụ</td></tr>
<tr><td>Giá</td><td>Giá dịch vụ (VNĐ)</td></tr>
<tr><td>Vai trò NV</td><td>Vai trò nhân viên phù hợp (Stylist, Technician...)</td></tr>
<tr><td>Cấp độ NV</td><td>Cấp độ tối thiểu (Junior, Senior, Master...)</td></tr>
</tbody>
</table>
<h3>Bộ lọc</h3>
<ul>
<li>Tìm kiếm theo từ khóa</li>
<li>Danh mục (dropdown)</li>
<li>Trạng thái (Hoạt động / Ngừng / Tất cả)</li>
<li>Hiển thị trên App (Có / Không / Tất cả)</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Dịch vụ sẽ được bổ sung</em></p></div>",
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyServices,
            HostPermissionName = PermHostSalonBeautyServices
        },
        new PageSeed
        {
            Slug = "nhan-vien",
            Title = "Nhân viên (Stylists)",
            DisplayOrder = 4,
            ContentHtml = @"<h2>Nhân viên (Stylists)</h2>
<p>Trang <strong>Nhân viên</strong> cho phép quản lý thông tin nhân viên/stylist của Salon Beauty, bao gồm avatar, cơ sở làm việc, cấp độ, vai trò và trạng thái hiển thị trên Mini App.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị nhân viên với avatar, tên, mã NV, cơ sở, cấp độ, giới tính, kinh nghiệm, vai trò, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo nhân viên mới với avatar upload</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin nhân viên</li>
<li><strong>Xóa:</strong> Xóa nhân viên</li>
<li><strong>Xem chi tiết:</strong> Modal xem thông tin chi tiết</li>
<li><strong>Bật/tắt trạng thái:</strong> Toggle trạng thái hoạt động trực tiếp</li>
<li><strong>Hiển thị trên App:</strong> Toggle hiển thị nhân viên trên Mini App</li>
</ul>
<h3>Thông tin nhân viên</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trường</th><th>Mô tả</th></tr></thead>
<tbody>
<tr><td>Avatar</td><td>Ảnh đại diện nhân viên</td></tr>
<tr><td>Họ tên</td><td>Tên hiển thị</td></tr>
<tr><td>Mã NV</td><td>Mã định danh nội bộ</td></tr>
<tr><td>Cơ sở</td><td>Chi nhánh làm việc</td></tr>
<tr><td>Cấp độ</td><td>Junior / Senior / Master / Director</td></tr>
<tr><td>Vai trò</td><td>Stylist / Technician / Therapist...</td></tr>
<tr><td>Giới tính</td><td>Nam / Nữ</td></tr>
<tr><td>Số năm kinh nghiệm</td><td>Kinh nghiệm làm việc</td></tr>
</tbody>
</table>
<h3>Bộ lọc</h3>
<ul>
<li>Tìm kiếm theo từ khóa</li>
<li>Cơ sở (dropdown)</li>
<li>Cấp độ (dropdown)</li>
<li>Vai trò (dropdown)</li>
<li>Trạng thái (Hoạt động / Ngừng / Tất cả)</li>
<li>Hiển thị trên App (Có / Không / Tất cả)</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Nhân viên sẽ được bổ sung</em></p></div>",
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyStylists,
            HostPermissionName = PermHostSalonBeautyStylists
        },
        new PageSeed
        {
            Slug = "nap-tien",
            Title = "Nạp tiền (Deposits)",
            DisplayOrder = 5,
            ContentHtml = @"<h2>Nạp tiền (Deposits)</h2>
<p>Trang <strong>Nạp tiền</strong> cho phép quản lý các giao dịch nạp tiền của khách hàng Salon Beauty. Hỗ trợ quy trình duyệt 2 bước (Chờ duyệt → Thành công/Hủy) và tự động tích điểm theo tỷ lệ cấu hình.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị giao dịch với mã, khách hàng, số tiền, điểm tích lũy, phương thức, mã tham chiếu, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo giao dịch nạp tiền mới</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật thông tin (chỉ khi Chờ duyệt)</li>
<li><strong>Duyệt:</strong> Xác nhận giao dịch thành công (chỉ khi Chờ duyệt)</li>
<li><strong>Hủy:</strong> Hủy giao dịch với lý do (chỉ khi Chờ duyệt)</li>
<li><strong>Xóa:</strong> Xóa giao dịch (chỉ khi chưa Thành công)</li>
<li><strong>Xem chi tiết:</strong> Modal xem thông tin chi tiết giao dịch</li>
</ul>
<h3>Trạng thái giao dịch</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trạng thái</th><th>Mô tả</th><th>Hành động</th></tr></thead>
<tbody>
<tr><td>Chờ duyệt</td><td>Giao dịch mới tạo, chờ xác nhận</td><td>Duyệt / Hủy / Sửa / Xóa</td></tr>
<tr><td>Thành công</td><td>Đã xác nhận, tiền và điểm đã cộng</td><td>Chỉ xem</td></tr>
<tr><td>Đã hủy</td><td>Giao dịch bị hủy</td><td>Xóa</td></tr>
</tbody>
</table>
<h3>Tính điểm tích lũy</h3>
<p>Khi giao dịch được duyệt, hệ thống tự động:</p>
<ul>
<li>Cộng số tiền vào tài khoản khách hàng</li>
<li>Tính điểm theo tỷ lệ quy đổi (ExchangeRate) + điểm thưởng theo bậc (BonusTier)</li>
<li>Ghi nhận lịch sử ledger (BalanceBefore/After)</li>
</ul>
<h3>Bộ lọc</h3>
<ul>
<li>Tìm kiếm theo từ khóa</li>
<li>Khách hàng (Select2 dropdown)</li>
<li>Trạng thái (Chờ duyệt / Thành công / Đã hủy)</li>
<li>Phương thức thanh toán (Tiền mặt / Chuyển khoản / Ví điện tử)</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Nạp tiền sẽ được bổ sung</em></p></div>",
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyDeposits,
            HostPermissionName = PermHostSalonBeautyDeposits
        },
        new PageSeed
        {
            Slug = "cau-hinh-loyalty",
            Title = "Cấu hình tích điểm",
            DisplayOrder = 6,
            ContentHtml = @"<h2>Cấu hình tích điểm (Loyalty Config)</h2>
<p>Trang <strong>Cấu hình tích điểm</strong> cho phép thiết lập tỷ lệ quy đổi tiền → điểm và các bậc thưởng bonus khi nạp tiền.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Tỷ lệ quy đổi (Exchange Rate):</strong> Cấu hình bao nhiêu VNĐ = 1 điểm</li>
<li><strong>Bậc thưởng (Bonus Tier):</strong> Cấu hình % điểm thưởng thêm theo mức nạp</li>
</ul>
<h3>Ví dụ cấu hình</h3>
<table class=""table table-bordered"">
<thead><tr><th>Cấu hình</th><th>Giá trị</th><th>Ý nghĩa</th></tr></thead>
<tbody>
<tr><td>Exchange Rate</td><td>10.000</td><td>Mỗi 10.000đ nạp = 1 điểm</td></tr>
<tr><td>Bonus Tier 1</td><td>Nạp từ 500.000đ: +5%</td><td>Nạp 500K được thêm 5% điểm</td></tr>
<tr><td>Bonus Tier 2</td><td>Nạp từ 1.000.000đ: +10%</td><td>Nạp 1M được thêm 10% điểm</td></tr>
</tbody>
</table>
<h3>Hướng dẫn sử dụng</h3>
<ol>
<li>Truy cập menu <strong>Salon Beauty → Cấu hình tích điểm</strong></li>
<li>Thiết lập tỷ lệ quy đổi phù hợp với chính sách doanh nghiệp</li>
<li>Thêm các bậc thưởng bonus để khuyến khích nạp nhiều</li>
<li>Nhấn <strong>Lưu</strong> để áp dụng</li>
</ol>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Cấu hình tích điểm sẽ được bổ sung</em></p></div>",
            FeatureName = FeatSalonBeauty,
            TenantPermissionName = PermSalonBeautyLoyaltyConfig,
            HostPermissionName = PermHostSalonBeautyLoyaltyConfig
        }
    };
}
