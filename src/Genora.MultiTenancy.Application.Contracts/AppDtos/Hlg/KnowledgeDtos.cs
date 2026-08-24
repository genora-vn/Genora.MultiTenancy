using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>Danh mục kiến thức. Khớp contract KnowledgeCategory.</summary>
public class KnowledgeCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int ProductCount { get; set; }
}

/// <summary>Bài học/sản phẩm kiến thức. Khớp contract Product.</summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public List<string> Images { get; set; } = new();
    public bool IsCompleted { get; set; }
}
