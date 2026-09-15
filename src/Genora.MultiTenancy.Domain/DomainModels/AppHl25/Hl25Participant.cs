using Genora.MultiTenancy.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHl25;

/// <summary>
/// Người tham gia chương trình "Dược Phẩm Hoa Linh 25 Năm".
/// LƯU Ý (đã chốt): KHÔNG quản lý điểm số (Points) ở module này — điểm thuộc module gamification.
/// </summary>
[Table("AppHl25Participants", Schema = "hl25")]
public class Hl25Participant : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>First automatic turn only. Admin grants do not change EarnedCycles.</summary>
    public bool TryGrantFrameTurn() => TryGrantAutomaticTurn(0);

    /// <summary>Second automatic turn only, after a card has granted the first turn.</summary>
    public bool TryGrantShareTurn() => TryGrantAutomaticTurn(1);

    private bool TryGrantAutomaticTurn(int requiredEarnedTurns)
    {
        if (EarnedCycles != requiredEarnedTurns)
            return false;

        // Compute before assigning so overflow cannot partially modify the aggregate.
        var remaining = checked(RemainingSpinTurns + 1);
        var total = checked(TotalSpinTurns + 1);
        EarnedCycles++;
        RemainingSpinTurns = remaining;
        TotalSpinTurns = total;
        return true;
    }

    public Guid? TenantId { get; set; }

    /// <summary>ID người dùng Zalo (định danh trong Mini App).</summary>
    [StringLength(64)]
    public string? ZaloUserId { get; set; }

    /// <summary>Họ và tên.</summary>
    [StringLength(256)]
    public string? FullName { get; set; }

    /// <summary>Số điện thoại (regex ^(0\d{9,10}|84\d{9,10})$).</summary>
    [StringLength(Hl25.Hl25Consts.MaxPhoneLength)]
    public string? PhoneNumber { get; set; }

    /// <summary>Nhóm tuổi (Delta 2026-09: màn "Thông tin nhận quà" dùng nhóm tuổi thay ngày sinh).</summary>
    public Hl25AgeGroup AgeGroup { get; set; } = Hl25AgeGroup.Unknown;

    /// <summary>Giới tính.</summary>
    public Hl25Gender Gender { get; set; } = Hl25Gender.Unknown;

    /// <summary>Địa chỉ nhận quà.</summary>
    [StringLength(1024)]
    public string? ReceiveAddress { get; set; }

    /// <summary>Ngày tham gia chương trình.</summary>
    public DateTime JoinedTime { get; set; }

    /// <summary>Trạng thái Follow OA.</summary>
    public bool IsFollowingOa { get; set; }

    /// <summary>Đồng ý chia sẻ thông tin (consent).</summary>
    public bool HasConsent { get; set; }

    /// <summary>Thời điểm đồng ý.</summary>
    public DateTime? ConsentTime { get; set; }

    /// <summary>Số lượt quay còn lại.</summary>
    public int RemainingSpinTurns { get; set; }

    /// <summary>Tổng lượt đã nhận, gồm lượt tự nhận và lượt Admin cấp.</summary>
    public int TotalSpinTurns { get; set; }

    /// <summary>
    /// Số lượt tự nhận: 0 = chưa nhận; 1 = đã nhận lượt tạo thiệp; 2 = đã nhận lượt chia sẻ.
    /// Giữ tên cột/API để tương thích. Không tính lượt Admin cấp; giữ nguyên dữ liệu đã cấp trước 14/09.
    /// </summary>
    public int EarnedCycles { get; set; }

    /// <summary>Tổng số quà đã trúng.</summary>
    public int TotalGiftsWon { get; set; }

    /// <summary>Ảnh đại diện Zalo.</summary>
    [StringLength(1024)]
    public string? AvatarUrl { get; set; }

    protected Hl25Participant() { }

    public Hl25Participant(Guid id, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        JoinedTime = DateTime.Now;
    }
}
