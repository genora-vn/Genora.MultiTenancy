namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// DTO khách hàng từ API Hoa Linh DMS
/// </summary>
public class HlCustomerDto
{
    public string? CustCode { get; set; }
    public string? DistributorCode { get; set; }
    public string? DistributorName { get; set; }
    public string? CustName { get; set; }
    public string? Address { get; set; }
    public string? Birthday { get; set; }
    public string? CustPhone { get; set; }
    public string? CustChannel { get; set; }
    public string? CustSubChannel { get; set; }
    public string? CustGroup { get; set; }
    public string? DsrCode { get; set; }
    public string? DsrName { get; set; }
    public bool? IsGkhl { get; set; }
    public string? GkhlContractStatus { get; set; }
    public decimal? AccumulatedSales { get; set; }
    public int? AccumulatedPoints { get; set; }
    public string? MembershipTier { get; set; }
    public int? PointsToNextTier { get; set; }
    public string? NextMembershipTier { get; set; }

    // Chỉ có trong API get-customer-by-phone
    public string? Phone { get; set; }
    public bool? IsCustomer { get; set; }
    public string? LoyaltyTier { get; set; }
    public int? LoyaltyPoint { get; set; }

    /// <summary>Nguồn khách hàng (CustomerSource): 1=ZaloMiniApp/Genora, 5=HoaLinh... Dùng cho filter admin.</summary>
    public int? Source { get; set; }

    /// <summary>Tên nguồn hiển thị (Genora (Mini App), Hoa Linh (DMS)...).</summary>
    public string? SourceText { get; set; }

    /// <summary>Đánh dấu bản ghi có tồn tại trong dbo.AppCustomers (Genora) hay không.</summary>
    public bool ExistsInGenora { get; set; }

    /// <summary>
    /// Số dư tiền thưởng (BonusAmount) từ dbo.AppCustomers — dùng đổi quà UrBox.
    /// Chỉ có giá trị khi: tồn tại custCode + custChannel="OTC" + isGkhl=true; ngược lại = 0.
    /// </summary>
    public decimal BonusAmount { get; set; }
}
