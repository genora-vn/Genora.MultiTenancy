using System;
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
}