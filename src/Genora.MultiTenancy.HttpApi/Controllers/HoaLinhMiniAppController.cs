using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppDtos.AppPayments;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.AppServices.AppPayments;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Genora.MultiTenancy.AppServices.HoaLinh;
using Genora.MultiTenancy.Controllers;
using Genora.MultiTenancy.DomainModels.AppHlGiftExchanges;
using Genora.MultiTenancy.DomainModels.AppHlOrders;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.HttpApi.Controllers;

/// <summary>
/// Controller trung gian cho Zalo Mini App Hoa Linh
/// Mini App → Genora (HoaLinhMiniAppController) → API Hoa Linh DMS
/// </summary>
[IgnoreAntiforgeryToken]
[RemoteService(false)]
[Area("MultiTenancy")]
[Route("api/mini-app/hl")]
[AllowAnonymous]
public class HoaLinhMiniAppController : MultiTenancyController
{
    private readonly IHlApiClientService _hlApi;
    private readonly IRepository<HlOrder, Guid> _orderRepo;
    private readonly IRepository<HlGiftExchange, Guid> _giftRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IAsyncQueryableExecuter _asyncExecuter;
    private readonly IZaloApiClient _zaloApiClient;
    private readonly IHlPaymentService _paymentService;
    private readonly IHlCustomerAppService _hlCustomerService;
    private readonly IHlPointAppService _hlPointService;

    public HoaLinhMiniAppController(
        IHlApiClientService hlApi,
        IRepository<HlOrder, Guid> orderRepo,
        IRepository<HlGiftExchange, Guid> giftRepo,
        ICurrentTenant currentTenant,
        IAsyncQueryableExecuter asyncExecuter,
        IZaloApiClient zaloApiClient,
        IHlPaymentService paymentService,
        IHlCustomerAppService hlCustomerService,
        IHlPointAppService hlPointService)
    {
        _hlApi = hlApi;
        _orderRepo = orderRepo;
        _giftRepo = giftRepo;
        _currentTenant = currentTenant;
        _asyncExecuter = asyncExecuter;
        _zaloApiClient = zaloApiClient;
        _paymentService = paymentService;
        _hlCustomerService = hlCustomerService;
        _hlPointService = hlPointService;
    }

    #region Auth

    /// <summary>
    /// Giải mã số điện thoại từ Zalo code + accessToken
    /// </summary>
    [HttpPost("decode-phone")]
    public async Task<IActionResult> DecodePhone([FromBody] ZaloDecodeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.AccessToken))
            return BadRequest("Missing code or accessToken");

        var result = await _zaloApiClient.DecodePhoneAsync(request.Code, request.AccessToken, ct);
        return Ok(result);
    }

    /// <summary>
    /// Check khách hàng tồn tại trên DMS Hoa Linh bằng SĐT (GET — không có dữ liệu Mini App bổ sung).
    /// Mini App gọi sau khi decode-phone thành công. Đồng thời đăng ký/đồng bộ vào dbo.AppCustomers.
    /// </summary>
    [HttpGet("auth/{phone}")]
    public async Task<IActionResult> CheckCustomer(string phone)
    {
        return await CheckAndRegisterAsync(new HlCheckCustomerRequest { PhoneNumber = phone });
    }

    /// <summary>
    /// Check + đăng ký khách hàng (POST — nhận thông tin Mini App: fullName, avatarUrl, zaloUserId, isFollower, note).
    /// - Tồn tại bên HL DMS → lưu mã KH + nguồn HoaLinh + thông tin trả về.
    /// - Chưa có bên HL DMS → tự sinh mã + nguồn ZaloMiniApp, lưu thông tin từ Mini App.
    /// </summary>
    [HttpPost("auth")]
    public async Task<IActionResult> CheckCustomerWithInfo([FromBody] HlCheckCustomerRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.PhoneNumber))
            return BadRequest(HlApiResult<object>.Fail("Thiếu số điện thoại"));

        return await CheckAndRegisterAsync(request);
    }

    /// <summary>
    /// Logic chung: check bên HL DMS + upsert vào dbo.AppCustomers.
    /// Luôn đăng ký khách vào AppCustomers dù có hay không bên HL DMS (nguồn khác nhau).
    /// </summary>
    private async Task<IActionResult> CheckAndRegisterAsync(HlCheckCustomerRequest request)
    {
        var phone = request.PhoneNumber;
        if (string.IsNullOrWhiteSpace(phone))
            return BadRequest(HlApiResult<object>.Fail("Thiếu số điện thoại"));

        var result = await _hlApi.GetCustomerByPhoneAsync(phone);

        if (!result.Success)
            return Ok(HlApiResult<object>.Fail(result.Error ?? "Lỗi khi kiểm tra khách hàng"));

        var hlCustomer = (result.Data != null && result.Data.Count > 0) ? result.Data[0] : null;
        var existsOnHl = hlCustomer != null && hlCustomer.IsCustomer != false;

        // Đăng ký/đồng bộ vào dbo.AppCustomers + lấy DTO trả về (luôn có dữ liệu KH).
        // Dù tồn tại hay không bên HL DMS, khách vẫn được lưu và trả về thông tin.
        var customerDto = await _hlCustomerService.UpsertFromHoaLinhAsync(request, existsOnHl ? hlCustomer : null);

        // Gán BonusAmount (điều kiện: custCode + custChannel=OTC + isGkhl=true, ngược lại = 0)
        var list = new List<HlCustomerDto> { customerDto };
        await _hlCustomerService.EnrichBonusAmountAsync(list);

        return Ok(HlApiResult<HlCustomerDto>.Ok(customerDto));
    }

    /// <summary>
    /// Lấy thông tin chi tiết khách hàng sau khi đăng nhập thành công.
    /// Ưu tiên dữ liệu bên Hoa Linh DMS; nếu không tồn tại thì lấy từ dbo.AppCustomers.
    /// </summary>
    [HttpGet("customer/{phone}")]
    public async Task<IActionResult> GetCustomer(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return BadRequest(HlApiResult<object>.Fail("Thiếu số điện thoại"));

        // 1. Check bên Hoa Linh DMS trước — mỗi bản ghi tương ứng 1 chi nhánh
        var hlResult = await _hlApi.GetCustomerDetailAsync(phone);
        var hlCustomers = (hlResult.Success && hlResult.Data != null) ? hlResult.Data : new List<HlCustomerDto>();
        if (hlCustomers.Count > 0)
        {
            // Gán BonusAmount (custCode + custChannel=OTC + isGkhl=true, ngược lại = 0)
            await _hlCustomerService.EnrichBonusAmountAsync(hlCustomers);
            return Ok(HlApiResult<List<HlCustomerDto>>.Ok(hlCustomers));
        }

        // 2. Fallback: lấy từ dbo.AppCustomers (cũng trả list — mỗi bản ghi 1 chi nhánh; đã enrich BonusAmount)
        var local = await _hlCustomerService.GetFromAppCustomersAsync(phone);
        return Ok(HlApiResult<List<HlCustomerDto>>.Ok(local));
    }

    #endregion

    #region Products

    /// <summary>
    /// Lấy danh sách danh mục sản phẩm (Brands)
    /// </summary>
    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 50)
    {
        var result = await _hlApi.GetBrandsAsync(page, limit);

        // Chỉ trả về thương hiệu đang hoạt động và có Seq > 0
        if (result.Success && result.Data?.Data != null)
        {
            result.Data.Data = result.Data.Data
                .Where(x => x.IsActive == true && x.Seq > 0)
                .OrderBy(x => x.Seq)
                .ToList();

            result.Data.TotalRecords = result.Data.Data.Count;
        }

        return Ok(result);
    }


    /// <summary>
    /// Lấy sản phẩm theo danh mục (brand_code) — chỉ sản phẩm đang hoạt động
    /// </summary>
    [HttpGet("brands/{brandCode}/products")]
    public async Task<IActionResult> GetProductsByBrand(string brandCode)
    {
        if (string.IsNullOrWhiteSpace(brandCode))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã danh mục"));

        var result = await _hlApi.GetProductsByBrandAsync(brandCode);

        // Chỉ trả về sản phẩm đang hoạt động (isActive = true)
        if (result.Success && result.Data != null)
        {
            var active = result.Data.Where(x => x.IsActive == true).ToList();
            return Ok(HlApiResult<List<HlProductByBrandDto>>.Ok(active));
        }

        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm
    /// </summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int limit = 20, [FromQuery] string? search = null)
    {
        var result = await _hlApi.GetProductsAsync(page, limit, search);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm
    /// </summary>
    [HttpGet("product-group")]
    public async Task<IActionResult> GetProductGroups([FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var result = await _hlApi.GetProductGroupsAsync(page, limit);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm bán chạy theo khách hàng (top mua nhiều)
    /// GET /api/TopCustomerProductsWithDetails/{customerCode}
    /// </summary>
    [HttpGet("best-seller-products/{customerCode}")]
    public async Task<IActionResult> GetBestSellerProducts(string customerCode)
    {
        if (string.IsNullOrWhiteSpace(customerCode))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã khách hàng"));

        var result = await _hlApi.GetTopProductsAsync(customerCode);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết sản phẩm (ProductGroup — có description, instruction)
    /// </summary>
    [HttpGet("products/{productCode}")]
    public async Task<IActionResult> GetProductDetail(string productCode)
    {
        if (string.IsNullOrWhiteSpace(productCode))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã sản phẩm"));

        var result = await _hlApi.GetProductGroupDetailAsync(productCode);
        return Ok(result);
    }

    #endregion

    #region Orders

    /// <summary>
    /// Tạo đơn hàng từ Mini App → lưu DB Genora + push sang API Hoa Linh
    /// </summary>
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] HlCreateOrderRequest request)
    {
        if (request == null || request.Items == null || request.Items.Count == 0)
            return BadRequest(HlApiResult<object>.Fail("Đơn hàng phải có ít nhất 1 sản phẩm"));

        // Generate order code
        var orderCode = $"HL-{DateTime.Now:yyMMdd}{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

        var order = new HlOrder(Guid.NewGuid(), orderCode, _currentTenant.Id)
        {
            CustomerCode = request.CustomerCode,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            BranchCode = request.BranchCode,
            BranchName = request.BranchName,
            DeliveryAddress = request.DeliveryAddress,
            ReceiverName = request.ReceiverName,
            ReceiverPhone = request.ReceiverPhone,
            ReceiverCode = request.ReceiveCode,
            DiscountCode = request.DiscountCode,
            DiscountAmount = request.DiscountAmount,
            SystemDiscount = request.SystemDiscount,
            PaymentMethod = (HlPaymentMethod)request.PaymentMethod,
            Note = request.Note,
            DeliveryStatus = HlOrderDeliveryStatus.PendingConfirmation,
            PaymentStatus = HlOrderPaymentStatus.Unpaid
        };

        decimal subTotal = 0;
        foreach (var item in request.Items)
        {
            var amount = item.Price * item.Quantity;
            subTotal += amount;

            order.Items.Add(new HlOrderItem(
                Guid.NewGuid(), order.Id, item.ProductCode, item.ProductName, item.Price, item.Quantity)
            {
                TenantId = _currentTenant.Id,
                ProductGroupCode = item.ProductGroupCode,
                ProductGroupName = item.ProductGroupName,
                BrandName = item.BrandName,
                ProductUnit = item.ProductUnit,
                ImageUrl = item.ImageUrl,
                OriginalPrice = item.OriginalPrice,
                Amount = amount,
                Note = item.Note
            });
        }

        order.SubTotal = subTotal;
        order.TotalAmount = subTotal - request.DiscountAmount - request.SystemDiscount;

        await _orderRepo.InsertAsync(order, autoSave: true);

        return Ok(HlApiResult<object>.Ok(new
        {
            order.Id,
            order.OrderCode,
            order.TotalAmount,
            order.DeliveryStatus,
            order.PaymentStatus,
            ItemCount = order.Items.Count
        }, "Đặt hàng thành công"));
    }

    /// <summary>
    /// Lấy lịch sử đơn hàng của KH.
    /// - customerCode (bắt buộc): lấy toàn bộ đơn theo mã khách hàng.
    /// - zaloOrderNumber (không bắt buộc): nếu truyền → lọc theo mã order Genora trên DMS.
    /// Merge đơn từ DMS Hoa Linh (Source=hoalinh) + đơn từ Genora DB (Source=genora).
    /// Nếu DMS trả về zalo_order_number = OrderCode trong Genora → sync trạng thái từ DMS,
    /// và đơn đó CHỈ hiển thị 1 lần dưới Source=genora (không nhân đôi).
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] string? customerCode, [FromQuery] string? zaloOrderNumber, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(customerCode))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã khách hàng"));

        // 1. Lấy đơn từ DMS Hoa Linh (truyền zaloOrderNumber nếu có → lọc theo mã order Genora)
        var hlResult = await _hlApi.GetOrderHeaderZaloAsync(customerCode, zaloOrderNumber);
        var hlOrders = (hlResult.Success && hlResult.Data != null) ? hlResult.Data : new List<HlOrderHeaderDto>();

        // 2. Lấy đơn từ Genora DB theo customerCode
        var queryable = await _orderRepo.GetQueryableAsync();
        var genoraQuery = queryable.Where(x => x.CustomerCode == customerCode);
        if (!string.IsNullOrWhiteSpace(zaloOrderNumber))
            genoraQuery = genoraQuery.Where(x => x.OrderCode == zaloOrderNumber);
        var genoraOrders = await _asyncExecuter.ToListAsync(
            genoraQuery.OrderByDescending(x => x.CreationTime));

        // 3. Sync status: nếu DMS có zalo_order_number = OrderCode Genora → cập nhật status Genora
        //    Các mã order Genora đã được DMS ghi nhận (để loại khỏi danh sách hoalinh, tránh nhân đôi)
        var matchedGenoraCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var hlOrder in hlOrders)
        {
            if (string.IsNullOrWhiteSpace(hlOrder.ZaloOrderNumber)) continue;
            var matchedOrder = genoraOrders.FirstOrDefault(x => x.OrderCode == hlOrder.ZaloOrderNumber);
            if (matchedOrder == null) continue;

            matchedGenoraCodes.Add(hlOrder.ZaloOrderNumber!);

            // Map DMS statusCode → Genora DeliveryStatus
            var newDeliveryStatus = MapHlStatusToDeliveryStatus(hlOrder.OrderStatusCode);
            if (matchedOrder.DeliveryStatus != newDeliveryStatus)
            {
                matchedOrder.DeliveryStatus = newDeliveryStatus;
                matchedOrder.ExternalOrderCode = hlOrder.OrderNumber;
                matchedOrder.IsSyncedToHl = true;
                matchedOrder.SyncedAt = DateTime.Now;
                await _orderRepo.UpdateAsync(matchedOrder, autoSave: true);
            }
        }

        // 4a. Đơn Genora (Source=genora)
        var genoraResult = genoraOrders.Select(o => new
        {
            Source = "genora",
            Id = (object)o.Id,
            o.OrderCode,
            o.CustomerCode,
            o.CustomerName,
            o.BranchName,
            o.DeliveryAddress,
            o.TotalAmount,
            DeliveryStatus = (int)o.DeliveryStatus,
            DeliveryStatusText = GetDeliveryStatusText(o.DeliveryStatus),
            PaymentStatus = (int)o.PaymentStatus,
            PaymentStatusText = GetPaymentStatusText(o.PaymentStatus),
            OrderDate = o.CreationTime.ToString("yyyy-MM-dd"),
            o.Note,
            o.ReceiverName
        });

        // 4b. Đơn thuần DMS Hoa Linh (Source=hoalinh) — loại các đơn đã map với Genora để tránh nhân đôi
        var hlOnlyResult = hlOrders
            .Where(h => string.IsNullOrWhiteSpace(h.ZaloOrderNumber)
                        || !matchedGenoraCodes.Contains(h.ZaloOrderNumber!))
            .Select(h => new
            {
                Source = "hoalinh",
                Id = (object?)null,
                OrderCode = h.OrderNumber,
                h.CustomerCode,
                h.CustomerName,
                BranchName = h.DistributorName,
                h.DeliveryAddress,
                TotalAmount = h.TotalAmount ?? 0,
                DeliveryStatus = h.OrderStatusCode ?? 0,
                DeliveryStatusText = h.OrderStatus ?? "Không xác định",
                PaymentStatus = 0,
                PaymentStatusText = "",
                OrderDate = h.OrderDate ?? "",
                Note = (string?)null,
                ReceiverName = h.DsrName
            });

        // 5. Gộp 2 nguồn, sắp xếp theo ngày giảm dần
        var combined = genoraResult.Cast<object>()
            .Concat(hlOnlyResult.Cast<object>())
            .ToList();

        return Ok(HlApiResult<object>.Ok(combined));
    }

    private static Enums.HlOrderDeliveryStatus MapHlStatusToDeliveryStatus(int? hlStatusCode)
    {
        return hlStatusCode switch
        {
            1 => Enums.HlOrderDeliveryStatus.PendingConfirmation, // Khởi tạo → Đơn mới
            2 => Enums.HlOrderDeliveryStatus.Processing,          // Đang xử lý
            3 => Enums.HlOrderDeliveryStatus.Completed,           // Hoàn thành
            4 => Enums.HlOrderDeliveryStatus.Completed,           // Đã thanh toán → Hoàn thành
            5 => Enums.HlOrderDeliveryStatus.Cancelled,           // Đã hủy
            6 => Enums.HlOrderDeliveryStatus.Cancelled,           // Từ chối → Đã hủy
            7 => Enums.HlOrderDeliveryStatus.Completed,           // Đã trả hàng → Hoàn thành
            _ => Enums.HlOrderDeliveryStatus.PendingConfirmation
        };
    }

    private static string GetDeliveryStatusText(Enums.HlOrderDeliveryStatus status)
    {
        return status switch
        {
            Enums.HlOrderDeliveryStatus.PendingConfirmation => "Đơn mới",
            Enums.HlOrderDeliveryStatus.Processing => "Đang xử lý",
            Enums.HlOrderDeliveryStatus.Delivering => "Đang giao",
            Enums.HlOrderDeliveryStatus.Completed => "Hoàn thành",
            Enums.HlOrderDeliveryStatus.Cancelled => "Đã hủy",
            _ => "Không xác định"
        };
    }

    private static string GetPaymentStatusText(Enums.HlOrderPaymentStatus status)
    {
        return status switch
        {
            Enums.HlOrderPaymentStatus.Unpaid => "Chưa thanh toán",
            Enums.HlOrderPaymentStatus.Paid => "Đã thanh toán",
            Enums.HlOrderPaymentStatus.Debt => "Công nợ",
            _ => "Không xác định"
        };
    }

    /// <summary>
    /// Lấy chi tiết đơn hàng theo nguồn (source).
    /// - source=genora: lấy chi tiết từ Genora DB (HL.AppHlOrders) theo OrderCode + customerCode.
    /// - source=hoalinh (mặc định): gọi API DMS Hoa Linh GetOrderDetailAsync theo orderNumber.
    /// </summary>
    [HttpGet("orders/{orderNumber}")]
    public async Task<IActionResult> GetOrderDetail(string orderNumber, [FromQuery] string? source, [FromQuery] string? customerCode)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã đơn hàng"));

        // Đơn Genora → đọc trực tiếp từ DB, map sang shape giống API Hoa Linh DMS (mỗi item = 1 record)
        if (string.Equals(source, "genora", StringComparison.OrdinalIgnoreCase))
        {
            var queryable = await _orderRepo.WithDetailsAsync(x => x.Items);
            var query = queryable.Where(x => x.OrderCode == orderNumber);
            if (!string.IsNullOrWhiteSpace(customerCode))
                query = query.Where(x => x.CustomerCode == customerCode);

            var order = await _asyncExecuter.FirstOrDefaultAsync(query);
            if (order == null)
                return Ok(HlApiResult<object>.Fail("Không tìm thấy đơn hàng"));

            var orderStatusText = GetDeliveryStatusText(order.DeliveryStatus);
            var orderDate = order.CreationTime.ToString("yyyy-MM-dd");
            var orderTime = order.CreationTime.ToString("HH:mm:ss");

            // Map từng item sang HlOrderDetailDto để đồng nhất trường với DMS
            var detailList = order.Items.Select(i => new HlOrderDetailDto
            {
                DistributorCode = order.BranchCode,        // Mã chi nhánh
                CustomerCode = order.CustomerCode,          // Mã khách hàng
                OrderNumber = order.ExternalOrderCode,      // Mã đơn hàng Hoa Linh (nếu đã đẩy DMS)
                ProductCode = i.ProductCode,                // Mã sản phẩm
                DistributorName = order.BranchName,         // Tên chi nhánh
                DsrCode = order.ReceiverCode,               // Mã trình dược viên
                DsrName = order.ReceiverName,               // Tên trình dược viên
                CustomerName = order.CustomerName,          // Tên khách hàng
                DeliveryAddress = order.DeliveryAddress,    // Địa chỉ giao hàng
                ProductGroupCode = i.ProductGroupCode,      // Mã danh mục sản phẩm
                ProductGroupName = i.ProductGroupName,      // Tên danh mục sản phẩm
                ProductName = i.ProductName,                // Tên sản phẩm
                ProductUnit = i.ProductUnit,                // Đơn vị tính
                ImageUrl = i.ImageUrl,                      // Ảnh sản phẩm
                ProductPrice = i.Price,                     // Giá sản phẩm
                Quantity = i.Quantity,                      // Số lượng
                TotalAmount = i.Amount,                     // Tổng giá trị
                NetValue = i.Amount,
                GrossValue = i.Amount,
                OrderStatus = orderStatusText,              // Trạng thái đơn hàng
                OrderDate = orderDate,                      // Ngày đặt hàng
                OrderTime = orderTime,                      // Giờ đặt hàng
                ZaloOrderNumber = order.OrderCode           // Mã đơn hàng Genora
            }).ToList();

            return Ok(HlApiResult<List<HlOrderDetailDto>>.Ok(detailList));
        }

        // Mặc định (source=hoalinh) → gọi API DMS
        var result = await _hlApi.GetOrderDetailAsync(orderNumber);
        return Ok(result);
    }

    /// <summary>
    /// Lấy đơn hàng Mini App (Genora DB) theo customer code
    /// </summary>
    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders([FromQuery] string customerCode, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        if (string.IsNullOrWhiteSpace(customerCode))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã khách hàng"));

        var queryable = await _orderRepo.WithDetailsAsync();
        var orders = await _asyncExecuter.ToListAsync(
            queryable
                .Where(x => x.CustomerCode == customerCode)
                .OrderByDescending(x => x.CreationTime)
                .Skip(skip)
                .Take(take)
        );

        var result = orders.Select(o => new
        {
            o.Id,
            o.OrderCode,
            o.CustomerName,
            o.BranchName,
            o.DeliveryAddress,
            o.SubTotal,
            o.DiscountAmount,
            o.SystemDiscount,
            o.TotalAmount,
            DeliveryStatus = (int)o.DeliveryStatus,
            DeliveryStatusText = o.DeliveryStatus.ToString(),
            PaymentStatus = (int)o.PaymentStatus,
            PaymentStatusText = o.PaymentStatus.ToString(),
            PaymentMethod = o.PaymentMethod.HasValue ? (int)o.PaymentMethod : (int?)null,
            o.Note,
            o.CreationTime,
            ItemCount = o.Items.Count,
            Items = o.Items.Select(i => new
            {
                i.ProductCode,
                i.ProductName,
                i.ImageUrl,
                i.Price,
                i.OriginalPrice,
                i.Quantity,
                i.Amount,
                i.ProductUnit
            })
        });

        return Ok(HlApiResult<object>.Ok(result));
    }

    /// <summary>
    /// Hủy đơn hàng Mini App
    /// </summary>
    [HttpPost("orders/{id}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] HlOrderCancelDto? input)
    {
        var order = await _orderRepo.FindAsync(id);
        if (order == null)
            return NotFound(HlApiResult<object>.Fail("Không tìm thấy đơn hàng"));

        if (order.DeliveryStatus == HlOrderDeliveryStatus.Cancelled)
            return Ok(HlApiResult<object>.Fail("Đơn hàng đã bị hủy trước đó"));

        if (order.DeliveryStatus == HlOrderDeliveryStatus.Completed)
            return Ok(HlApiResult<object>.Fail("Không thể hủy đơn hàng đã hoàn thành"));

        order.DeliveryStatus = HlOrderDeliveryStatus.Cancelled;
        order.CancelNote = input?.CancelNote;
        order.CancelledAt = DateTime.Now;

        await _orderRepo.UpdateAsync(order, autoSave: true);

        return Ok(HlApiResult<object>.Ok(new { order.Id, order.OrderCode, Status = "Cancelled" }, "Hủy đơn hàng thành công"));
    }

    #endregion

    #region Loyalty & Campaigns

    /// <summary>
    /// Lấy thông tin Loyalty (điểm, hạng) qua SĐT
    /// </summary>
    [HttpGet("loyalty/{phone}")]
    public async Task<IActionResult> GetLoyalty(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return BadRequest(HlApiResult<object>.Fail("Thiếu số điện thoại"));

        var result = await _hlApi.GetCustomerByPhoneAsync(phone);

        if (!result.Success || result.Data == null || result.Data.Count == 0)
            return Ok(HlApiResult<object>.Fail("Không tìm thấy thông tin loyalty"));

        var customer = result.Data[0];
        var loyalty = new
        {
            customer.CustCode,
            customer.CustName,
            customer.IsGkhl,
            customer.CustChannel,
            customer.LoyaltyTier,
            customer.LoyaltyPoint,
            customer.MembershipTier,
            customer.AccumulatedSales,
            customer.AccumulatedPoints,
            customer.PointsToNextTier,
            customer.NextMembershipTier
        };

        return Ok(HlApiResult<object>.Ok(loyalty));
    }

    /// <summary>
    /// Lấy danh sách chiến dịch
    /// </summary>
    [HttpGet("campaigns")]
    public async Task<IActionResult> GetCampaigns([FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var result = await _hlApi.GetCampaignsAsync(page, limit);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết chiến dịch theo mã khách hàng (custCode).
    /// GET /api/CustomerCampaigns/{custCode} — trả list (mỗi bản ghi 1 chiến dịch KH tham gia).
    /// </summary>
    [HttpGet("campaigns/{custCode}")]
    public async Task<IActionResult> GetCampaignDetail(string custCode)
    {
        if (string.IsNullOrWhiteSpace(custCode))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã khách hàng"));

        var result = await _hlApi.GetCampaignDetailAsync(custCode);
        return Ok(result);
    }

    /// <summary>
    /// Đổi điểm/tiền từ chiến dịch → cộng vào quỹ điểm thưởng (BonusPoint/BonusAmount).
    /// Mỗi (khách + chiến dịch) chỉ đổi 1 lần.
    /// </summary>
    [HttpPost("loyalty/redeem")]
    public async Task<IActionResult> RedeemPoint([FromBody] HlRedeemPointInput input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.CustomerCode) || string.IsNullOrWhiteSpace(input.CampaignCode))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã khách hàng hoặc mã chiến dịch"));

        try
        {
            var result = await _hlPointService.RedeemFromCampaignAsync(input);
            return Ok(HlApiResult<HlPointBatchDto>.Ok(result, "Đổi điểm thành công"));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlApiResult<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Lấy số dư điểm thưởng (điểm + tiền) + danh sách lô còn hiệu lực.
    /// </summary>
    [HttpGet("loyalty/balance/{customerCode}")]
    public async Task<IActionResult> GetLoyaltyBalance(string customerCode)
    {
        if (string.IsNullOrWhiteSpace(customerCode))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã khách hàng"));

        var result = await _hlPointService.GetBalanceAsync(customerCode);
        return Ok(HlApiResult<HlPointBalanceDto>.Ok(result));
    }

    /// <summary>
    /// Lấy lịch sử giao dịch điểm thưởng của khách.
    /// </summary>
    [HttpGet("loyalty/history/{customerCode}")]
    public async Task<IActionResult> GetLoyaltyHistory(string customerCode, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        if (string.IsNullOrWhiteSpace(customerCode))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã khách hàng"));

        var result = await _hlPointService.GetCustomerHistoryAsync(customerCode, skip, take);
        return Ok(HlApiResult<List<HlPointTransactionDto>>.Ok(result));
    }

    #endregion

    #region Gift Exchange

    /// <summary>
    /// Tạo yêu cầu đổi quà từ Mini App
    /// </summary>
    [HttpPost("gift-exchange")]
    public async Task<IActionResult> CreateGiftExchange([FromBody] HlCreateGiftExchangeRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.GiftName))
            return BadRequest(HlApiResult<object>.Fail("Thiếu thông tin quà tặng"));

        if (string.IsNullOrWhiteSpace(request.CustomerCode))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã khách hàng"));

        var quantity = request.Quantity > 0 ? request.Quantity : 1;
        var totalPointsUsed = request.PointsRequired * quantity;

        var exchangeCode = $"HLGE-{DateTime.Now:yyMMdd}{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

        // Tiêu điểm thưởng (FIFO) trước khi tạo yêu cầu đổi quà — guard số dư.
        if (totalPointsUsed > 0)
        {
            try
            {
                await _hlPointService.SpendAsync(
                    request.CustomerCode,
                    (int)HlPointUnit.Point,
                    totalPointsUsed,
                    exchangeCode,
                    $"Đổi quà: {request.GiftName}");
            }
            catch (UserFriendlyException ex)
            {
                // Không đủ điểm / không tìm thấy KH → trả lỗi, KHÔNG tạo yêu cầu đổi quà
                return Ok(HlApiResult<object>.Fail(ex.Message));
            }
        }

        var entity = new HlGiftExchange(
            Guid.NewGuid(), exchangeCode, request.GiftName, request.PointsRequired, _currentTenant.Id)
        {
            CustomerCode = request.CustomerCode,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            GiftCode = request.GiftCode,
            GiftImageUrl = request.GiftImageUrl,
            Quantity = quantity,
            TotalPointsUsed = totalPointsUsed,
            Note = request.Note,
            DeliveryAddress = request.DeliveryAddress,
            Status = HlGiftExchangeStatus.Processing
        };

        await _giftRepo.InsertAsync(entity, autoSave: true);

        return Ok(HlApiResult<object>.Ok(new
        {
            entity.Id,
            entity.ExchangeCode,
            entity.GiftName,
            entity.TotalPointsUsed,
            Status = entity.Status.ToString()
        }, "Yêu cầu đổi quà đã được gửi"));
    }

    /// <summary>
    /// Lấy lịch sử đổi quà của KH
    /// </summary>
    [HttpGet("gift-exchange")]
    public async Task<IActionResult> GetGiftExchangeHistory([FromQuery] string customerCode, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        if (string.IsNullOrWhiteSpace(customerCode))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã khách hàng"));

        var queryable = await _giftRepo.GetQueryableAsync();
        var items = await _asyncExecuter.ToListAsync(
            queryable
                .Where(x => x.CustomerCode == customerCode)
                .OrderByDescending(x => x.CreationTime)
                .Skip(skip)
                .Take(take)
        );

        var result = items.Select(e => new
        {
            e.Id,
            e.ExchangeCode,
            e.GiftName,
            e.GiftImageUrl,
            e.PointsRequired,
            e.Quantity,
            e.TotalPointsUsed,
            Status = (int)e.Status,
            StatusText = e.Status.ToString(),
            e.UrBoxVoucherCode,
            e.CreationTime,
            e.ApprovedAt
        });

        return Ok(HlApiResult<object>.Ok(result));
    }

    #endregion

    #region News (Zalo OA Articles)

    /// <summary>
    /// Lấy danh sách tin tức (bài viết Zalo OA).
    /// Access token lấy từ ZaloAuth active theo tenant (fallback test token nếu cấu hình).
    /// </summary>
    [HttpGet("news")]
    public async Task<IActionResult> GetNews([FromQuery] int offset = 0, [FromQuery] int limit = 10, [FromQuery] string type = "normal", CancellationToken ct = default)
    {
        var result = await _zaloApiClient.GetArticleListAsync(offset, limit, type, ct);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết 1 tin tức (bài viết Zalo OA) theo id.
    /// </summary>
    [HttpGet("news/{articleId}")]
    public async Task<IActionResult> GetNewsDetail(string articleId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(articleId))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã bài viết"));

        var result = await _zaloApiClient.GetArticleDetailAsync(articleId, ct);
        return Ok(result);
    }

    #endregion

    #region Salemans

    /// <summary>
    /// Lấy thông tin Sales phụ trách (cho trang account Mini App)
    /// </summary>
    [HttpGet("saleman/{dsrCode}")]
    public async Task<IActionResult> GetSaleman(string dsrCode)
    {
        if (string.IsNullOrWhiteSpace(dsrCode))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã nhân viên"));

        var result = await _hlApi.GetSalemanDetailAsync(dsrCode);
        return Ok(result);
    }

    #endregion

    #region Payment

    /// <summary>
    /// Chuẩn bị thanh toán đơn hàng — trả về MAC signature cho Zalo Checkout SDK
    /// </summary>
    [HttpPost("payment/prepare-order")]
    public async Task<IActionResult> PrepareOrder([FromBody] PrepareHlOrderInput input)
    {
        try
        {
            var result = await _paymentService.PrepareOrderAsync(input);
            return Ok(HlApiResult<PrepareOrderResult>.Ok(result));
        }
        catch (UserFriendlyException ex)
        {
            return Ok(HlApiResult<object>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Kiểm tra trạng thái giao dịch thanh toán
    /// </summary>
    [HttpGet("payment/check-transaction/{orderId}")]
    public async Task<IActionResult> CheckTransaction(string orderId)
    {
        var result = await _paymentService.CheckTransactionAsync(orderId);
        return Ok(HlApiResult<CheckTransactionResult>.Ok(result));
    }

    #endregion
}
