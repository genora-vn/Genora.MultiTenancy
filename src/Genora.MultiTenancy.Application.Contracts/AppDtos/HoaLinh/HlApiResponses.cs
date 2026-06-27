using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// Response phân trang chuẩn từ API Hoa Linh DMS
/// Property names map tự động qua SnakeCaseLower JsonOptions trong HlApiClientService
/// ABP serialize ra browser dùng camelCase (totalRecords, totalPages, data)
/// </summary>
public class HlPagedResponse<T>
{
    public int TotalRecords { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages { get; set; }
    public List<T> Data { get; set; } = new();
}

/// <summary>
/// Response wrapper cho tất cả call API HL trả về Admin/MiniApp
/// </summary>
public class HlApiResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }

    public static HlApiResult<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static HlApiResult<T> Fail(string error, string? message = null)
        => new() { Success = false, Error = error, Message = message };
}
