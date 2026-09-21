
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.Hlg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.Hlg
{
    public class HlgBrandAppService : ApplicationService, IHlgBrandAppService
    {
        private readonly IRepository<HlgBrand, Guid> _brandRepository;

        public HlgBrandAppService(IRepository<HlgBrand, Guid> brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public async Task<BrandKnowledgeDto> GetBrandAsync(Guid id, CancellationToken ct = default)
        {
            var brand = await _brandRepository.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
            var products = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgProduct, Guid>>().GetQueryableAsync();
            var brandProducts = products.Where(p => p.Id == id && p.IsActive).ToList();
            return new BrandKnowledgeDto
            {
                Id = brand.Id,
                Name = brand.Name,
                CategoryId = brand.CategoryId,
                Products = products.Select(p => new BrandProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Summary,
                    ImageUrl = p.ThumbnailUrl
                }).ToList()
            };
        }

        public Task<List<BrandKnowledgeDto>> GetBrandsAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
        public async Task<HlgProductDto> GetProductAsync(Guid id, CancellationToken ct = default)
        {
            var productRepository =await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgProduct, Guid>>().GetQueryableAsync();
            var product = productRepository.ToList().FirstOrDefault(p => p.Id == id);
            var categoryRepository = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgKnowledgeCategory, Guid>>().GetQueryableAsync();
            var brandRepository = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgBrand, Guid>>().GetQueryableAsync();
            var category = categoryRepository.ToList().FirstOrDefault(c => c.Id == product.CategoryId);
            var brand = brandRepository.ToList().FirstOrDefault(b => b.Id == product.BrandId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }
            if (!product.IsActive)
            {
                throw new Exception("Product is not active");
            }
            var relatedProducts = productRepository.Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive).ToList();
            var detail = string.IsNullOrEmpty(product.DetailsJson) ? null : JsonSerializer.Deserialize<HlgProductContent>(product.DetailsJson);
            
            var result = new HlgProductDto
            {
                Id = product.Id,
                Name = product.Name,
                CategoryId = product.CategoryId,
                Summary = product.Summary,
                BrandName = brand?.Name,
                CategoryName = category?.Name,
                ThumbnailUrl = detail?.Media.FirstOrDefault(x => x.Placement == HlgMediaPlacement.Hero)?.Url ?? product.ThumbnailUrl,
                BrandId = product.BrandId,
                ProductInformation = new ProductInformationDto
                {
                    ProductInformation = product.Content,
                    ImageUrl = detail?.Media.FirstOrDefault(x => x.Placement == HlgMediaPlacement.Information)?.Url
                },
                Knowledge = new KnowledgeProductDto
                {
                    QuestionAndAnswers = detail?.Knowledge.Select(kn => new QuestionProductDto
                    {
                        Question = kn.Title,
                        Answer = kn.Content
                    }).ToList(),
                    ImageUrl = detail?.Media.FirstOrDefault(x => x.Placement == HlgMediaPlacement.Knowledge)?.Url
                },
                RelatedProducts = new RolationsDto{
                    RolationProductDtos = relatedProducts.Select(rp => new RolationProductDto
                    {
                        Id = rp.Id,
                        Name = rp.Name,
                        Description = rp.Summary,
                        ImageUrl = rp.ThumbnailUrl
                    }).ToList(),
                    ImageUrl = detail?.Media.FirstOrDefault(x => x.Placement == HlgMediaPlacement.Related)?.Url
                }
            };
            return result;
        }
    }
}
