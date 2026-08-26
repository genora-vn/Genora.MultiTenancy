using System;
using System.ComponentModel.DataAnnotations;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Hl25;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>DTO ghi thông tin người tham gia (Admin sửa họ tên/SĐT/địa chỉ/consent/follow).</summary>
public class CreateUpdateHl25ParticipantDto
{
    [StringLength(256)]
    public string? FullName { get; set; }

    [StringLength(Hl25Consts.MaxPhoneLength)]
    [RegularExpression(Hl25Consts.PhoneRegex, ErrorMessage = "Số điện thoại không hợp lệ (bắt đầu bằng 0 hoặc 84).")]
    public string? PhoneNumber { get; set; }

    public DateTime? BirthDate { get; set; }

    public Hl25Gender Gender { get; set; } = Hl25Gender.Unknown;

    [StringLength(1024)]
    public string? ReceiveAddress { get; set; }

    public bool IsFollowingOa { get; set; }

    public bool HasConsent { get; set; }
}
