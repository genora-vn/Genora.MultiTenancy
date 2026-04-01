using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppGolfCourses;

public class AppGolfCourseDto : AuditedEntityDto<Guid>
{
    public string Code { get; set; }              // Mã sân
    public string Name { get; set; }              // Tên sân

    public string Address { get; set; }
    public string Province { get; set; }
    public string Phone { get; set; }

    public string Website { get; set; }
    public string FanpageUrl { get; set; }

    public string ShortDescription { get; set; }
    public string AvatarUrl { get; set; }
    public string BannerUrl { get; set; }

    public string CancellationPolicy { get; set; }
    public string TermsAndConditions { get; set; }

    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }

    public byte BookingStatus { get; set; }       // 1 = Đang mở, 2 = Tạm ngừng...
    public bool IsActive { get; set; }
    public string? FrameTimes { get; set; }

    public string? NumberHoles { get; set; }

    public string? Utilities { get; set; }

    public string? PaymentQrText { get; set; }
    public string? PaymentQrBankCode { get; set; }
    public string? PaymentQrBankAccount { get; set; }
    public string? PaymentQrBankDisplay { get; set; }

    public short? CancellationPolicyHours { get; set; }
    public string? PromotionTypeIds { get; set; }

    public List<Guid> PromotionTypeIdList =>
        !string.IsNullOrWhiteSpace(PromotionTypeIds)
            ? PromotionTypeIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => Guid.TryParse(x, out var id) ? id : Guid.Empty)
                .Where(x => x != Guid.Empty)
                .ToList()
            : new List<Guid>();
}