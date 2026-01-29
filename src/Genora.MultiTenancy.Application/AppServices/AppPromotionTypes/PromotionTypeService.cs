using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.AppPromotionTypes;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Genora.MultiTenancy.Features.AppCustomerTypes;
using Genora.MultiTenancy.Features.AppPromotionTypes;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;

namespace Genora.MultiTenancy.AppServices.AppPromotionTypes
{
    [Authorize]
    public class PromotionTypeService : FeatureProtectedCrudAppService<PromotionType, AppPromotionTypeDto, Guid, PagedAndSortedResultRequestDto, CreateUpdatePromotionTypeDto>, IPromotionTypeService
    {
        protected override string FeatureName => AppPromotionTypeFeature.Management;

        protected override string TenantDefaultPermission => MultiTenancyPermissions.AppPromotionType.Default;

        protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppPromotionType.Default;
        private readonly IManageImageService _imageService;
        public PromotionTypeService(IRepository<PromotionType, Guid> repository, ICurrentTenant currentTenant, IFeatureChecker featureChecker, IManageImageService imageService) : base(repository, currentTenant, featureChecker)
        {
            GetPolicyName = MultiTenancyPermissions.AppPromotionType.Default;
            GetListPolicyName = MultiTenancyPermissions.AppPromotionType.Default;
            CreatePolicyName = MultiTenancyPermissions.AppPromotionType.Create;
            UpdatePolicyName = MultiTenancyPermissions.AppPromotionType.Edit;
            DeletePolicyName = MultiTenancyPermissions.AppPromotionType.Delete;
            _imageService = imageService;
        }



        public override async Task<PagedResultDto<AppPromotionTypeDto>> GetListAsync (PagedAndSortedResultRequestDto input)
        {
            await CheckGetListPolicyAsync();
            var queryable = await Repository.GetQueryableAsync();

            var sorting = string.IsNullOrWhiteSpace(input.Sorting)
                ? nameof(PromotionType.CreationTime) + " desc"
                : input.Sorting;
            var query = queryable
           .OrderBy(sorting)
           .Skip(input.SkipCount)
           .Take(input.MaxResultCount);

            var items = await AsyncExecuter.ToListAsync(query);
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            return new PagedResultDto<AppPromotionTypeDto>(totalCount, ObjectMapper.Map<List<PromotionType>, List<AppPromotionTypeDto>>(items));
        }
        public override async Task<AppPromotionTypeDto> CreateAsync (CreateUpdatePromotionTypeDto input)
        {
            await CheckGetListPolicyAsync();
            var exist = await Repository.FirstOrDefaultAsync(x => x.Code == input.Code);
            if (exist != null) throw new InvalidOperationException("Mã ưu đãi này đã tồn tại");
            //if (input.Images != null)
            //{
            //    var upload = await _imageService.UploadImageAsync(input.Images);
            //    input.IconUrl = upload;
            //}
            var promotion = new PromotionType
            {
                Code = input.Code,
                Name = input.Name,
                Description = input.Description,
                IconUrl = input.IconUrl ?? "",
                ColorCode = input.ColorCode,
                Status = input.Status,
            };
            var inserting = await Repository.InsertAsync(promotion);
            
            return ObjectMapper.Map<PromotionType, AppPromotionTypeDto>(inserting);
        }
        public override async Task<AppPromotionTypeDto> UpdateAsync (Guid id, CreateUpdatePromotionTypeDto input)
        {
            await CheckGetListPolicyAsync();
            var exist = await Repository.FirstOrDefaultAsync( x => x.Id == id);
            if (exist == null) throw new InvalidOperationException("Không tìm thấy loại ưu đãi này");
            //if (input.Images != null)
            //{
            //    await _imageService.DeleteFileAsync(exist.IconUrl);
            //    var upload = await _imageService.UploadImageAsync (input.Images);
            //    exist.IconUrl = upload;
            //}
            exist.IconUrl = input.IconUrl;
            exist.Name = input.Name;
            exist.Description = input.Description;
            exist.ColorCode = input.ColorCode;
            exist.Status = input.Status;
            await Repository.UpdateAsync(exist);
            return ObjectMapper.Map<PromotionType, AppPromotionTypeDto>(exist);
        }
    }
}
