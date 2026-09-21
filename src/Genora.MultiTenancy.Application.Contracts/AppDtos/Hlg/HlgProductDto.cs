

using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.Hlg
{
    public class HlgProductDto
    {
        public Guid? BrandId { get; set; }
        //public List<BrandProductDto> BrandProducts { get; set; } = new();
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public ProductInformationDto ProductInformation { get; set; } = new();
        public KnowledgeProductDto Knowledge { get; set; } = new();
        public RolationsDto RelatedProducts { get; set; } = new();
    }
    public class ProductInformationDto
    {
        public string? ProductInformation { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } = string.Empty;
    }
    public class KnowledgeProductDto
    {
        public List<QuestionProductDto>? QuestionAndAnswers { get; set; } = new();
        public string? ImageUrl { get; set; } = string.Empty;
    }
    public class QuestionProductDto
    {
        public string Question { get; set; } = string.Empty;
        public string? Answer { get; set; }
    }
    public class RolationsDto 
    {
        public List<RolationProductDto> RolationProductDtos { get; set; } = new();
        public string? ImageUrl { get; set; } = string.Empty;
    }
    public class RolationProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } = string.Empty;
    }
}
