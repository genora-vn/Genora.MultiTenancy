using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDocuments;

public partial class AppDocumentsDataSeedContributor
{
    private List<PageSeed> GetNewsPages() => new()
    {
        new PageSeed
        {
            Slug = "gioi-thieu",
            Title = "Giới thiệu",
            DisplayOrder = 1,
            ContentHtml = @"<h2>Tin tức</h2>
<p>Chuyên mục <strong>Tin tức</strong> cho phép quản lý các bài viết tin tức, thông báo hiển thị trên Zalo Mini App. Hỗ trợ soạn thảo nội dung HTML phong phú với hình ảnh, video.</p>
<h3>Các chức năng trong chuyên mục</h3>
<ul>
<li><strong>Quản lý tin tức</strong> — Tạo, chỉnh sửa, xuất bản và quản lý các bài viết</li>
</ul>",
            FeatureName = FeatNews,
            TenantPermissionName = PermAppNews,
            HostPermissionName = PermHostAppNews
        },
        new PageSeed
        {
            Slug = "quan-ly-tin-tuc",
            Title = "Quản lý tin tức",
            DisplayOrder = 2,
            ContentHtml = @"<h2>Quản lý tin tức</h2>
<p>Trang <strong>Quản lý tin tức</strong> cho phép tạo và quản lý các bài viết tin tức, thông báo, khuyến mãi hiển thị trên Zalo Mini App.</p>
<h3>Các tính năng chính</h3>
<ul>
<li><strong>Xem danh sách:</strong> Hiển thị bài viết với tiêu đề, ngày xuất bản, trạng thái, thứ tự hiển thị</li>
<li><strong>Thêm mới:</strong> Tạo bài viết mới với trình soạn thảo Summernote</li>
<li><strong>Chỉnh sửa:</strong> Cập nhật nội dung bài viết</li>
<li><strong>Xóa:</strong> Xóa bài viết</li>
</ul>
<h3>Trạng thái bài viết</h3>
<table class=""table table-bordered"">
<thead><tr><th>Trạng thái</th><th>Mô tả</th><th>Hiển thị Mini App</th></tr></thead>
<tbody>
<tr><td>Nháp (Draft)</td><td>Bài viết đang soạn, chưa xuất bản</td><td>Không</td></tr>
<tr><td>Đã xuất bản (Published)</td><td>Bài viết đã công khai</td><td>Có</td></tr>
<tr><td>Ẩn (Hidden)</td><td>Bài viết bị ẩn khỏi Mini App</td><td>Không</td></tr>
</tbody>
</table>
<h3>Bộ lọc</h3>
<ul>
<li>Tìm kiếm theo từ khóa (tiêu đề)</li>
<li>Trạng thái (Nháp / Đã xuất bản / Ẩn / Tất cả)</li>
</ul>
<h3>Hướng dẫn sử dụng</h3>
<ol>
<li>Truy cập menu <strong>Tin tức → Quản lý tin tức</strong></li>
<li>Nhấn <strong>Thêm mới</strong> để tạo bài viết</li>
<li>Nhập tiêu đề, chọn ngày xuất bản, soạn nội dung bằng trình soạn thảo</li>
<li>Chọn trạng thái <strong>Đã xuất bản</strong> để hiển thị trên Mini App</li>
<li>Nhấn <strong>Lưu</strong> để hoàn tất</li>
</ol>
<h3>Lưu ý</h3>
<ul>
<li>Nội dung bài viết hỗ trợ HTML: hình ảnh, video, bảng, liên kết</li>
<li>Nội dung được lazy load khi mở modal chỉnh sửa để tối ưu hiệu suất</li>
<li>Thứ tự hiển thị (Display Order) quyết định vị trí bài viết trên Mini App</li>
</ul>
<div class=""doc-screenshot-placeholder""><p><em>📷 Ảnh minh họa giao diện Quản lý tin tức sẽ được bổ sung</em></p></div>",
            FeatureName = FeatNews,
            TenantPermissionName = PermAppNews,
            HostPermissionName = PermHostAppNews
        }
    };
}
