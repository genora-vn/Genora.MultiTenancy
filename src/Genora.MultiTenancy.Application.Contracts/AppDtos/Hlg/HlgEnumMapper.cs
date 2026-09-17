using Genora.MultiTenancy.Enums.Hlg;
using System;

namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>
/// Map 2 chiều giữa enum nội bộ (byte) và chuỗi contract frontend.
/// Giữ tập trung để mọi nơi map nhất quán, khớp chính xác giá trị lowercase contract.
/// </summary>
public static class HlgEnumMapper
{
    // ===== Gender: "male" | "female" | "other" =====
    public static string? GenderToString(HlgGender? g) => g switch
    {
        HlgGender.Male => "male",
        HlgGender.Female => "female",
        HlgGender.Other => "other",
        _ => null
    };

    public static HlgGender? GenderFromString(string? s) => s?.Trim().ToLowerInvariant() switch
    {
        "male" => HlgGender.Male,
        "female" => HlgGender.Female,
        "other" => HlgGender.Other,
        _ => null
    };

    /// <summary>Customer.Gender lưu byte? (0 Unknown,1 Male,2 Female). Map sang string contract.</summary>
    public static string? GenderByteToString(byte? g) => g switch
    {
        1 => "male",
        2 => "female",
        3 => "other",
        _ => null
    };

    /// <summary>String contract → byte cho Customer.Gender.</summary>
    public static byte? GenderStringToByte(string? s) => s?.Trim().ToLowerInvariant() switch
    {
        "male" => (byte)1,
        "female" => (byte)2,
        "other" => (byte)3,
        _ => null
    };

    // ===== CustomerType: "pharmacy" | "consumer" =====
    public static string? CustomerTypeToString(HlgCustomerType? t) => t switch
    {
        HlgCustomerType.Pharmacy => "pharmacy",
        HlgCustomerType.Consumer => "consumer",
        _ => null
    };

    public static HlgCustomerType? CustomerTypeFromString(string? s) => s?.Trim().ToLowerInvariant() switch
    {
        "pharmacy" => HlgCustomerType.Pharmacy,
        "consumer" => HlgCustomerType.Consumer,
        _ => null
    };

    /// <summary>Birthday chuẩn ISO date (yyyy-MM-dd) cho contract.</summary>
    public static string? DateToIso(DateTime? d) => d?.ToString("yyyy-MM-dd");

    // ===== GameType (cấu hình động, BD-1) =====
    public static string GameTypeToString(HlgGameType t) => t switch
    {
        HlgGameType.Quiz => "quiz",
        HlgGameType.PictureToWord => "Picture-to-Word Puzzle",
        HlgGameType.KingOfVietnamese => "King of Vietnamese",
        HlgGameType.SpinWheel => "Spin Wheel - Lucky Wheel",
        HlgGameType.TileFlip => "Tile Flip / Reveal the Image",
        _ => "quiz"
    };

    public static HlgGameType GameTypeFromString(string? s) => s?.Trim() switch
    {
        "quiz" => HlgGameType.Quiz,
        "Picture-to-Word Puzzle" => HlgGameType.PictureToWord,
        "King of Vietnamese" => HlgGameType.KingOfVietnamese,
        "Spin Wheel - Lucky Wheel" => HlgGameType.SpinWheel,
        "Tile Flip / Reveal the Image" => HlgGameType.TileFlip,
        _ => HlgGameType.Quiz
    };

    // ===== GameStatus: "upcoming" | "ongoing" | "ended" =====
    public static string GameStatusToString(HlgGameStatus s) => s switch
    {
        HlgGameStatus.Upcoming => "upcoming",
        HlgGameStatus.Ongoing => "ongoing",
        HlgGameStatus.Ended => "ended",
        _ => "upcoming"
    };

    public static HlgGameStatus GameStatusFromString(string? s) => s?.Trim().ToLowerInvariant() switch
    {
        "upcoming" => HlgGameStatus.Upcoming,
        "ongoing" => HlgGameStatus.Ongoing,
        "ended" => HlgGameStatus.Ended,
        _ => HlgGameStatus.Upcoming
    };

    // ===== AnswerKey: "A" | "B" | "C" | "D" =====
    public static string AnswerKeyToString(HlgAnswerKey k) => k switch
    {
        HlgAnswerKey.A => "A",
        HlgAnswerKey.B => "B",
        HlgAnswerKey.C => "C",
        HlgAnswerKey.D => "D",
        _ => "A"
    };

    public static HlgAnswerKey? AnswerKeyFromString(string? s) => s?.Trim().ToUpperInvariant() switch
    {
        "A" => HlgAnswerKey.A,
        "B" => HlgAnswerKey.B,
        "C" => HlgAnswerKey.C,
        "D" => HlgAnswerKey.D,
        _ => null
    };

    // ===== RewardType: "physical" | "voucher" =====
    public static string RewardTypeToString(HlgRewardType t) => t switch
    {
        HlgRewardType.Physical => "physical",
        HlgRewardType.Voucher => "voucher",
        _ => "physical"
    };

    public static HlgRewardType RewardTypeFromString(string? s) => s?.Trim().ToLowerInvariant() switch
    {
        "physical" => HlgRewardType.Physical,
        "voucher" => HlgRewardType.Voucher,
        _ => HlgRewardType.Physical
    };

    // ===== RewardHistoryStatus: "pending" | "shipping" | "delivered" | "done" =====
    public static string RewardHistoryStatusToString(HlgRewardHistoryStatus s) => s switch
    {
        HlgRewardHistoryStatus.Pending => "pending",
        HlgRewardHistoryStatus.Shipping => "shipping",
        HlgRewardHistoryStatus.Delivered => "delivered",
        HlgRewardHistoryStatus.Done => "done",
        _ => "pending"
    };

    public static HlgRewardHistoryStatus RewardHistoryStatusFromString(string? s) => s?.Trim().ToLowerInvariant() switch
    {
        "pending" => HlgRewardHistoryStatus.Pending,
        "shipping" => HlgRewardHistoryStatus.Shipping,
        "delivered" => HlgRewardHistoryStatus.Delivered,
        "done" => HlgRewardHistoryStatus.Done,
        _ => HlgRewardHistoryStatus.Pending
    };
}
