using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlApiLogs;

/// <summary>
/// Log mọi API call tới hệ thống Hoa Linh DMS — dùng để debug và đối soát
/// </summary>
[Table("AppHlApiLogs", Schema = "HL")]
public class HlApiLog : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>HTTP Method (GET, POST, PUT, DELETE)</summary>
    [Required]
    [StringLength(10)]
    public string HttpMethod { get; set; } = null!;

    /// <summary>URL endpoint đã gọi</summary>
    [Required]
    [StringLength(500)]
    public string RequestUrl { get; set; } = null!;

    /// <summary>Request body (JSON)</summary>
    public string? RequestBody { get; set; }

    /// <summary>Request headers (JSON, loại bỏ sensitive info)</summary>
    public string? RequestHeaders { get; set; }

    /// <summary>HTTP Status Code trả về</summary>
    public int? ResponseStatusCode { get; set; }

    /// <summary>Response body (JSON, truncate nếu quá lớn)</summary>
    public string? ResponseBody { get; set; }

    /// <summary>Thời gian xử lý (ms)</summary>
    public long DurationMs { get; set; }

    /// <summary>Có lỗi không</summary>
    public bool IsError { get; set; }

    /// <summary>Thông tin lỗi (exception message)</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Loại dữ liệu đang xử lý (Customer, Product, Order, Loyalty...)</summary>
    [StringLength(50)]
    public string? DataType { get; set; }

    /// <summary>Nguồn gọi (Admin, MiniApp, BackgroundJob)</summary>
    [StringLength(50)]
    public string? CallerSource { get; set; }

    protected HlApiLog() { }

    public HlApiLog(Guid id, string httpMethod, string requestUrl, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        HttpMethod = httpMethod;
        RequestUrl = requestUrl;
    }
}
