using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDocuments;

public partial class AppDocumentsDataSeedContributor
{
    private List<PageSeed> GetLoyaltyPages() => new()
    {
        new PageSeed
        {
            Slug = "gioi-thieu",
            Title = "Giới thiệu",
            DisplayOrder = 1,
            ContentHtml = @"<h2>Khách hàng trung thành</h2>
<p>Chuyên mục <strong>Khách hàng trung thành</strong> cho phép quản lý chương trình loyalty, hạng thành viên và các chính sách ưu đãi dành cho khách hàng thân thiết.</p>
<h3>Các chức năng trong chuyên mục</h3>
<ul>
<li><strong>Hạng thành viên</strong> — Cấu hình các mức hạng và điều kiện nâng hạng</li>
<li><strong>Nhóm quà tặng</strong> — Quản lý nhóm quà tặng (đang phát triển)</li>
<li><strong>Quà tặng</strong> — Quản lý danh sách quà tặng (đang phát triển)</li>
<li><strong>Lịch sử đổi thưởng</strong> — Theo dõi lịch sử đổi điểm (đang phát triển)</li>
</ul>",
            FeatureName = FeatMembershipTier,
            TenantPermissionName = PermAppMembershipTiers,
            HostPermissionName = PermHostAppMembershipTiers
        },
        new PageSeed
        {
            Slug = "hang-thanh-vien",
            Title = "Hạng thành viên",
            DisplayOrder = 2,
            ContentHtml = @"<h2>Hạng thành viên</h2>
<p>Trang <strong>Hạng thành viên</strong> cho phép cấu hình các mức hạng thành viên trong chương trình loyalty. Khách hàng sẽ tự động được nâng hạng khi đạt điều kiện chi tiêu/điểm tích lũy.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị các hạng thành viên với tên, điều kiện, quyền lợi, trạng thái</li>
<li><strong>Thêm mới:</strong> Tạo hạng thành viên mới</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật điều kiện và quyền lợi</li>
<li><strong>Xóa:</strong> Xóa hạng thành viên</li>
</ul>
<h3>Các hạng mặc định</h3>
<table class=""table table-bordered"">
<thead><tr><th>Hạng</th><th>Mô tả</th></tr></thead>
<tbody>
<tr><td>NEW</td><td>Khách hàng mới đăng ký</td></tr>
<tr><td>REGULAR</td><td>Khách hàng thường xuyên</td></tr>
<tr><td>VIP</td><td>Khách hàng VIP</td></tr>
<tr><td>DIAMOND</td><td>Khách hàng cao cấp nhất</td></tr>
</tbody>
</table>
<h3>Lưu ý</h3>
<p>Hạng thành viên được sử dụng trong module Salon Beauty để hiển thị trên trang chi tiết khách hàng và xác định mức ưu đãi.</p>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Hạng thành viên sẽ được bổ sung</em></p></div>",
            FeatureName = FeatMembershipTier,
            TenantPermissionName = PermAppMembershipTiers,
            HostPermissionName = PermHostAppMembershipTiers
        }
    };
}
