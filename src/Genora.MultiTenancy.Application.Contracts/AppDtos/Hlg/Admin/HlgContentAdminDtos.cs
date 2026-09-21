using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Genora.MultiTenancy.Hlg;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
namespace Genora.MultiTenancy.AppDtos.Hlg.Admin;
public class HlgBrandInput
{
    public Guid CategoryId { get; set; }
    [Required, StringLength(250)] public string Name { get; set; } = "";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
public class HlgContentInput
{
    [EnumDataType(typeof(HlgContentSlot))] public HlgContentSlot Slot { get; set; } = HlgContentSlot.HomeBanner;
    [Required, StringLength(250)] public string Title { get; set; } = "";
    [StringLength(1000)] public string? Summary { get; set; }
    [StringLength(100)] public string? BadgeText { get; set; }
    [StringLength(1000)] public string? ImageUrl { get; set; }
    [StringLength(1000)] public string? TargetUrl { get; set; }
    public Guid? GameId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
public class HlgPrizeInput
{
    public Guid EventId { get; set; }
    public Guid RewardId { get; set; }
    [Required, StringLength(250)] public string Title { get; set; } = "";
    [Range(1,1000000)] public int Quantity { get; set; } = 1;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
public class HlgWinnerInput
{
    public Guid EventId { get; set; }
    public Guid PrizeId { get; set; }
    public Guid CustomerId { get; set; }
    public bool IsActive { get; set; }
}
public class HlgFulfillmentInput
{
    [EnumDataType(typeof(Genora.MultiTenancy.Enums.Hlg.HlgRewardHistoryStatus))]
    public Genora.MultiTenancy.Enums.Hlg.HlgRewardHistoryStatus Status { get; set; }
}
public class HlgFulfillmentDto : EntityDto<Guid>
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public string RewardName { get; set; } = "";
    public string? ReceiverName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public Genora.MultiTenancy.Enums.Hlg.HlgRewardHistoryStatus Status { get; set; }
    public DateTime CreationTime { get; set; }
}
public interface IHlgFulfillmentAdminAppService : IApplicationService
{
    Task<PagedResultDto<HlgFulfillmentDto>> GetListAsync(GetHlgAdminListInput input);
    Task<HlgFulfillmentDto> GetAsync(Guid id);
    Task UpdateAsync(Guid id, HlgFulfillmentInput input);
}
public class HlgLookupInput : PagedResultRequestDto
{
    [Required] public string Kind { get; set; } = "";
    public Guid? ParentId { get; set; }
    public Guid? Id { get; set; }
    public Guid? ExcludeId { get; set; }
    public string? FilterText { get; set; }
}
public class HlgLookupDto : EntityDto<Guid> { public string Name { get; set; } = ""; }
public interface IHlgLookupAdminAppService : IApplicationService
{ Task<PagedResultDto<HlgLookupDto>> GetListAsync(HlgLookupInput input); }

public class HlgBrandAdminDto : HlgBrandInput, IEntityDto<Guid> { public Guid Id { get; set; } }
public class CreateHlgBrandInput : HlgBrandInput { }
public class UpdateHlgBrandInput : HlgBrandInput { }
public interface IHlgBrandAdminAppService : ICrudAppService<HlgBrandAdminDto, Guid, GetHlgAdminListInput, CreateHlgBrandInput, UpdateHlgBrandInput> { }

public class HlgContentAdminDto : HlgContentInput, IEntityDto<Guid> { public Guid Id { get; set; } }
public class CreateHlgContentInput : HlgContentInput { }
public class UpdateHlgContentInput : HlgContentInput { }
public interface IHlgContentAdminAppService : ICrudAppService<HlgContentAdminDto, Guid, GetHlgAdminListInput, CreateHlgContentInput, UpdateHlgContentInput> { }

public class HlgPrizeAdminDto : HlgPrizeInput, IEntityDto<Guid> { public Guid Id { get; set; } }
public class CreateHlgPrizeInput : HlgPrizeInput { }
public class UpdateHlgPrizeInput : HlgPrizeInput { }
public interface IHlgPrizeAdminAppService : ICrudAppService<HlgPrizeAdminDto, Guid, GetHlgAdminListInput, CreateHlgPrizeInput, UpdateHlgPrizeInput> { }

public class HlgWinnerAdminDto : HlgWinnerInput, IEntityDto<Guid> { public Guid Id { get; set; } public int Rank { get; set; } public int Score { get; set; } public string CustomerName { get; set; } = ""; }
public class CreateHlgWinnerInput : HlgWinnerInput { }
public class UpdateHlgWinnerInput : HlgWinnerInput { }
public interface IHlgWinnerAdminAppService : ICrudAppService<HlgWinnerAdminDto, Guid, GetHlgAdminListInput, CreateHlgWinnerInput, UpdateHlgWinnerInput> { }
