using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Genora.MultiTenancy.Localization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Genora.MultiTenancy.Enums.Hlg;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
namespace Genora.MultiTenancy.AppDtos.Hlg.Admin;

public class GetHlgAdminListInput : GetHlgListInput
{
    public Guid? ParentId { get; set; }
    public Guid? BrandId { get; set; }
    public byte? CustomerType { get; set; }
    public bool? IsRegistered { get; set; }
    public byte? Status { get; set; }
}
public class HlgCategoryInput
{
    [Required, StringLength(250)] public string Name { get; set; } = "";
    [StringLength(1000)] public string? Description { get; set; }
    [StringLength(1000)] public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
public class HlgProductInput
{
    public Guid? BrandId { get; set; }
    public Genora.MultiTenancy.Hlg.HlgProductContent Details { get; set; } = new();
    public Guid CategoryId { get; set; }
    [Required, StringLength(250)] public string Name { get; set; } = "";
    [StringLength(1000)] public string? ThumbnailUrl { get; set; }
    [StringLength(1000)] public string? Summary { get; set; }
    public string? Content { get; set; }
    // One image URL per line in the admin editor; stored as a JSON array.
    public string? ImageUrls { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
public class HlgRankingInput : IValidatableObject
{
    public Guid? GameId { get; set; }
    [Required, StringLength(250)] public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateTime StartAt { get; set; } = DateTime.Today;
    public DateTime EndAt { get; set; } = DateTime.Today.AddDays(30);
    public bool IsActive { get; set; } = true;
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (StartAt >= EndAt) yield return HlgInputValidation.Error(context, "Hlg:DateRangeInvalid", "End time must be after start time.", nameof(EndAt));
    }
}
public class HlgGameInput : IValidatableObject
{
    [StringLength(100)] public string? BadgeText { get; set; }
    [StringLength(1000)] public string? BannerUrl { get; set; }
    [Required, StringLength(250)] public string Name { get; set; } = "";
    [EnumDataType(typeof(HlgGameType))] public HlgGameType Type { get; set; } = HlgGameType.Quiz;
    [StringLength(1000)] public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public string? Rules { get; set; }
    public string? RewardDescription { get; set; }
    [EnumDataType(typeof(HlgGameStatus))] public HlgGameStatus Status { get; set; } = HlgGameStatus.Upcoming;
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    [Range(0, 1000000)] public int BaseScorePerQuestion { get; set; } = 100;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (StartAt.HasValue && EndAt.HasValue && StartAt >= EndAt)
            yield return HlgInputValidation.Error(context, "Hlg:DateRangeInvalid", "End time must be after start time.", nameof(EndAt));
    }
}
// No CorrectKey or answer options on the list/read DTO.
public class HlgQuestionAdminDto : EntityDto<Guid>
{
    public Guid GameId { get; set; }
    public int Index { get; set; }
    public string Content { get; set; } = "";
    public string? ImageUrl { get; set; }
    public int TimeLimitSec { get; set; }
    public decimal ScoreMultiplier { get; set; }
    public bool IsActive { get; set; }
}
public class HlgQuestionInput : IValidatableObject
{
    public Guid GameId { get; set; }
    [Range(0, int.MaxValue)] public int Index { get; set; }
    [Required] public string Content { get; set; } = "";
    [StringLength(1000)] public string? ImageUrl { get; set; }
    [Range(1, 3600)] 
    public int TimeLimitSec { get; set; } = 30;

    [Range(typeof(decimal), "0.01", "100", ParseLimitsInInvariantCulture = true)]
    public decimal ScoreMultiplier { get; set; } = 1;
    [EnumDataType(typeof(HlgAnswerKey))] public HlgAnswerKey CorrectKey { get; set; } = HlgAnswerKey.A;
    [Required] public string OptionA { get; set; } = "";
    [Required] public string OptionB { get; set; } = "";
    public string? OptionC { get; set; }
    public string? OptionD { get; set; }
    public bool IsActive { get; set; } = true;
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var answer = CorrectKey switch { HlgAnswerKey.A => OptionA, HlgAnswerKey.B => OptionB, HlgAnswerKey.C => OptionC, HlgAnswerKey.D => OptionD, _ => null };
        if (string.IsNullOrWhiteSpace(answer)) yield return HlgInputValidation.Error(context, "Hlg:CorrectOptionRequired", "The correct answer must refer to a non-empty option.", nameof(CorrectKey));
    }
}
public class HlgUserAdminDto : EntityDto<Guid>
{
    public string? PharmacyCode { get; set; }
    public string? Address { get; set; }
    public DateTime? Birthday { get; set; }
    public byte? Gender { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerCode { get; set; }
    public string FullName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string? ZaloId { get; set; }
    public byte? CustomerType { get; set; }
    public decimal BonusPoint { get; set; }
    public bool IsRegistered { get; set; }
    public bool IsActive { get; set; }
}
public interface IHlgUserAdminAppService : IApplicationService
{
    Task<PagedResultDto<HlgUserAdminDto>> GetListAsync(GetHlgAdminListInput input);
    Task<HlgUserDetailDto> GetAsync(Guid id);
}
public class HlgCategoryAdminDto : HlgCategoryInput, IEntityDto<Guid> { public Guid Id { get; set; } }
public class HlgProductAdminDto : HlgProductInput, IEntityDto<Guid> { public Guid Id { get; set; } }
public class HlgRankingAdminDto : HlgRankingInput, IEntityDto<Guid> { public Guid Id { get; set; } }
public class HlgGameAdminDto : HlgGameInput, IEntityDto<Guid> { public Guid Id { get; set; } }
public class CreateHlgCategoryInput : HlgCategoryInput { }
public class UpdateHlgCategoryInput : HlgCategoryInput { }
public interface IHlgCategoryAdminAppService : ICrudAppService<HlgCategoryAdminDto, Guid, GetHlgAdminListInput, CreateHlgCategoryInput, UpdateHlgCategoryInput>
{
}
public class CreateHlgProductInput : HlgProductInput { }
public class UpdateHlgProductInput : HlgProductInput { }
public interface IHlgProductAdminAppService : ICrudAppService<HlgProductAdminDto, Guid, GetHlgAdminListInput, CreateHlgProductInput, UpdateHlgProductInput>
{
}
public class CreateHlgRankingInput : HlgRankingInput { }
public class UpdateHlgRankingInput : HlgRankingInput { }
public interface IHlgRankingAdminAppService : ICrudAppService<HlgRankingAdminDto, Guid, GetHlgAdminListInput, CreateHlgRankingInput, UpdateHlgRankingInput>
{
}
public class CreateHlgGameInput : HlgGameInput { }
public class UpdateHlgGameInput : HlgGameInput { }
public interface IHlgGameAdminAppService : ICrudAppService<HlgGameAdminDto, Guid, GetHlgAdminListInput, CreateHlgGameInput, UpdateHlgGameInput>
{
}
public class CreateHlgQuestionInput : HlgQuestionInput { }
public class UpdateHlgQuestionInput : HlgQuestionInput { }
public interface IHlgQuestionAdminAppService : ICrudAppService<HlgQuestionAdminDto, Guid, GetHlgAdminListInput, CreateHlgQuestionInput, UpdateHlgQuestionInput>
{
    Task<HlgQuestionInput> GetEditorAsync(Guid id);
}

internal static class HlgInputValidation
{
    public static ValidationResult Error(ValidationContext context, string key, string fallback, string member)
    {
        var localizer = context.GetService(typeof(IStringLocalizer<MultiTenancyResource>)) as IStringLocalizer<MultiTenancyResource>;
        var text = localizer?[key];
        return new ValidationResult(text == null || text.ResourceNotFound ? fallback : text.Value, new[] { member });
    }
}

public class HlgUserDetailDto
{
    public HlgUserAdminDto Profile { get; set; } = new();
    public ProfileStatsDto Stats { get; set; } = new();
    public List<LearningHistoryItemDto> Learning { get; set; } = new();
    public List<GameHistoryDto> Games { get; set; } = new();
    public List<RewardHistoryItemDto> Rewards { get; set; } = new();
}
