using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppServices.AppPayments;

/// <summary>
/// Mapping tên ngân hàng (do admin nhập) → mã ngân hàng chuẩn VietQR.
/// Danh sách tham khảo: https://api.vietqr.io/v2/banks
///
/// Tra cứu: dùng GetCode(bankName) → trả về mã ngắn (VD: "TPB", "VCB", "TCB").
/// Nếu không tìm thấy → trả về null, caller bỏ qua việc tạo QR.
/// </summary>
public static class VietQrBankCodeMap
{
    /// <summary>
    /// Key   = tên ngân hàng thường gặp (case-insensitive, không dấu)
    /// Value = bin (mã VietQR chính thức) + shortCode (dùng trong image URL)
    /// </summary>
    private static readonly Dictionary<string, (string Bin, string ShortCode)> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        // ── Ngân hàng thương mại nhà nước ──────────────────────────────────
        ["vietcombank"]           = ("970436", "VCB"),
        ["vcb"]                   = ("970436", "VCB"),
        ["vietinbank"]            = ("970415", "VIETINBANK"),
        ["ctg"]                   = ("970415", "CTG"),
        ["bidv"]                  = ("970418", "BIDV"),
        ["agribank"]              = ("970405", "AGR"),
        ["agr"]                   = ("970405", "AGR"),

        // ── Ngân hàng thương mại cổ phần lớn ───────────────────────────────
        ["techcombank"]           = ("970407", "TCB"),
        ["tcb"]                   = ("970407", "TCB"),
        ["mbbank"]                = ("970422", "MB"),
        ["mb"]                    = ("970422", "MB"),
        ["mb bank"]               = ("970422", "MB"),
        ["vpbank"]                = ("970432", "VPB"),
        ["vpb"]                   = ("970432", "VPB"),
        ["acb"]                   = ("970416", "ACB"),
        ["sacombank"]             = ("970403", "STB"),
        ["stb"]                   = ("970403", "STB"),
        ["tpbank"]                = ("970423", "TPB"),
        ["tpb"]                   = ("970423", "TPB"),
        ["tp bank"]               = ("970423", "TPB"),
        ["hdbank"]                = ("970437", "HDB"),
        ["hdb"]                   = ("970437", "HDB"),
        ["vib"]                   = ("970441", "VIB"),
        ["ocb"]                   = ("970448", "OCB"),
        ["msb"]                   = ("970426", "MSB"),
        ["maritime bank"]         = ("970426", "MSB"),
        ["abbank"]                = ("970425", "ABB"),
        ["abb"]                   = ("970425", "ABB"),
        ["seabank"]               = ("970440", "SEAB"),
        ["seab"]                  = ("970440", "SEAB"),
        ["shb"]                   = ("970443", "SHB"),
        ["bac a bank"]            = ("970409", "BAB"),
        ["bab"]                   = ("970409", "BAB"),
        ["pvcombank"]             = ("970412", "PVCB"),
        ["pvcb"]                  = ("970412", "PVCB"),
        ["eximbank"]              = ("970431", "EIB"),
        ["eib"]                   = ("970431", "EIB"),
        ["nam a bank"]            = ("970428", "NAB"),
        ["nab"]                   = ("970428", "NAB"),
        ["lpbank"]                = ("970449", "LPB"),
        ["lpb"]                   = ("970449", "LPB"),
        ["lien viet post bank"]   = ("970449", "LPB"),
        ["ncb"]                   = ("970419", "NCB"),
        ["vietbank"]              = ("970433", "VBB"),
        ["vbb"]                   = ("970433", "VBB"),
        ["kienlong bank"]         = ("970452", "KLB"),
        ["klb"]                   = ("970452", "KLB"),
        ["kienlongbank"]          = ("970452", "KLB"),
        ["pgbank"]                = ("970430", "PGB"),
        ["pgb"]                   = ("970430", "PGB"),
        ["saigonbank"]            = ("970400", "SGB"),
        ["sgb"]                   = ("970400", "SGB"),
        ["vietabank"]             = ("970427", "VAB"),
        ["vab"]                   = ("970427", "VAB"),
        ["indovina bank"]         = ("970434", "IVB"),
        ["ivb"]                   = ("970434", "IVB"),

        // ── Ngân hàng số / Fintech ──────────────────────────────────────────
        ["timo"]                  = ("963388", "TIMO"),
        ["cake"]                  = ("546034", "CAKE"),
        ["ubank"]                 = ("546035", "Ubank"),
        ["momo"]                  = ("970437", "HDB"), // MoMo dùng HDB làm backend

        // ── Ngân hàng nước ngoài ────────────────────────────────────────────
        ["hsbc"]                  = ("458761", "HSBC"),
        ["standard chartered"]    = ("970410", "SCVN"),
        ["scvn"]                  = ("970410", "SCVN"),
        ["citibank"]              = ("533948", "CITIBANK"),
    };

    /// <summary>
    /// Lấy (Bin, ShortCode) từ tên ngân hàng.
    /// Tự động chuẩn hoá: trim, lowercase, bỏ "ngân hàng" / "bank" prefix.
    /// Trả về null nếu không tìm thấy — caller bỏ qua QR generation.
    /// </summary>
    public static (string Bin, string ShortCode)? GetCode(string? bankName)
    {
        if (string.IsNullOrWhiteSpace(bankName))
            return null;

        // Chuẩn hoá: bỏ prefix "Ngân hàng" / "NH" / "ngan hang"
        var normalized = bankName
            .Trim()
            .ToLowerInvariant()
            .Replace("ngân hàng", "")
            .Replace("ngan hang", "")
            .Replace("ngan hàng", "")
            .Replace("ngân hang", "")
            .Trim();

        if (_map.TryGetValue(normalized, out var result))
            return result;

        // Thử khớp tên gốc chưa chuẩn hoá
        if (_map.TryGetValue(bankName.Trim(), out result))
            return result;

        return null;
    }
}
