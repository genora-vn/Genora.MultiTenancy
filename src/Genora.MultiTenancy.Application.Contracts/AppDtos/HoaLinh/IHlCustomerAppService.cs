using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// Service đăng ký/đồng bộ khách hàng Hoa Linh vào dbo.AppCustomers.
/// Idempotent theo số điện thoại.
/// </summary>
public interface IHlCustomerAppService
{
    /// <summary>
    /// Upsert khách hàng vào dbo.AppCustomers sau khi check bên HL DMS.
    /// - hlCustomer != null → khách tồn tại bên HL DMS: lưu mã KH + nguồn HoaLinh + thông tin trả về.
    /// - hlCustomer == null → chưa có bên HL DMS: tự sinh mã + nguồn ZaloMiniApp, lưu thông tin từ Mini App.
    /// Luôn trả về HlCustomerDto của khách đã lưu (nếu từ HL DMS thì trả nguyên thông tin DMS,
    /// nếu không thì build từ AppCustomers). Trường không có → null.
    /// Lưu ý: hlCustomer có default = null để ABP validation coi là optional (tránh AbpValidationException khi truyền null).
    /// </summary>
    Task<HlCustomerDto> UpsertFromHoaLinhAsync(HlCheckCustomerRequest request, HlCustomerDto? hlCustomer = null, CancellationToken ct = default);

    /// <summary>
    /// Lấy các bản ghi khách hàng đã lưu trong dbo.AppCustomers theo SĐT, map sang list HlCustomerDto.
    /// Mỗi bản ghi tương ứng 1 chi nhánh. Trả list rỗng nếu chưa có.
    /// Dùng cho fallback khi HL DMS không có dữ liệu.
    /// </summary>
    Task<List<HlCustomerDto>> GetFromAppCustomersAsync(string phone, CancellationToken ct = default);
}
