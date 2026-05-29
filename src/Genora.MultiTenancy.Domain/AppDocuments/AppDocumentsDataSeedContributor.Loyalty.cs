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
<p>Chuyên mục <strong>Khách hàng trung thành</strong> cho phép quản lý chương trình loyalty, hạng thành viên và các chính sách ưu đãi dành cho khách hàng thân thiết. Đây là công cụ giúp doanh nghiệp xây dựng mối quan hệ lâu dài với khách hàng thông qua hệ thống tích điểm và nâng hạng tự động.</p>

<h3>Tổng quan các chức năng</h3>

<h4>1. Hạng thành viên</h4>
<p>Cấu hình các mức hạng thành viên trong chương trình loyalty (NEW → REGULAR → VIP → DIAMOND). Mỗi hạng có điều kiện nâng hạng riêng dựa trên tổng chi tiêu hoặc điểm tích lũy. Khách hàng sẽ tự động được nâng hạng khi đạt điều kiện. Hạng thành viên hiển thị trên trang chi tiết khách hàng Salon và xác định mức ưu đãi áp dụng.</p>

<h4>Các chức năng đang phát triển</h4>
<ul>
<li><strong>Nhóm quà tặng</strong> — Quản lý nhóm/danh mục quà tặng đổi điểm (Coming Soon)</li>
<li><strong>Quà tặng</strong> — Quản lý danh sách quà tặng có thể đổi bằng điểm (Coming Soon)</li>
<li><strong>Lịch sử đổi thưởng</strong> — Theo dõi lịch sử đổi điểm lấy quà (Coming Soon)</li>
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
<thead><tr><th>Hạng</th><th>Mô tả</th><th>Điều kiện nâng hạng</th></tr></thead>
<tbody>
<tr><td>NEW</td><td>Khách hàng mới đăng ký</td><td>Mặc định khi tạo tài khoản</td></tr>
<tr><td>REGULAR</td><td>Khách hàng thường xuyên</td><td>Đạt mức chi tiêu/điểm cấu hình</td></tr>
<tr><td>VIP</td><td>Khách hàng VIP</td><td>Đạt mức chi tiêu/điểm cao hơn</td></tr>
<tr><td>DIAMOND</td><td>Khách hàng cao cấp nhất</td><td>Đạt mức chi tiêu/điểm cao nhất</td></tr>
</tbody>
</table>
<h3>Cơ chế nâng hạng</h3>
<ul>
<li>Hệ thống tự động kiểm tra điều kiện nâng hạng khi khách hàng có giao dịch mới</li>
<li>Hạng thành viên hiển thị trên trang chi tiết khách hàng Salon (badge màu)</li>
<li>Trang chi tiết khách hàng hiển thị hạng hiện tại và điều kiện để đạt hạng tiếp theo (NextTier)</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Hạng thành viên sẽ được bổ sung</em></p></div>",
            FeatureName = FeatMembershipTier,
            TenantPermissionName = PermAppMembershipTiers,
            HostPermissionName = PermHostAppMembershipTiers
        }
    };
}
