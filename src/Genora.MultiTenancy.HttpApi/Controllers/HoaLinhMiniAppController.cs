using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.AppServices.HoaLinh;
using Genora.MultiTenancy.Controllers;
using Genora.MultiTenancy.DomainModels.AppHlGiftExchanges;
using Genora.MultiTenancy.DomainModels.AppHlOrders;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public HoaLinhMiniAppController(
        IHlApiClientService hlApi,
        IRepository<HlOrder, Guid> orderRepo,
        IRepository<HlGiftExchange, Guid> giftRepo,
        ICurrentTenant currentTenant,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _hlApi = hlApi;
        _orderRepo = orderRepo;
        _giftRepo = giftRepo;
        _currentTenant = currentTenant;
        _asyncExecuter = asyncExecuter;
    }

    #region Auth

    /// <summary>
    /// Check khách hàng tồn tại trên DMS Hoa Linh bằng SĐT
    /// Mini App gọi sau khi decode-phone thành công
    /// </summary>
    [HttpGet("auth/{phone}")]
    public async Task<IActionResult> CheckCustomer(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return BadRequest(HlApiResult<object>.Fail("Thiếu số điện thoại"));

        var result = await _hlApi.GetCustomerByPhoneAsync(phone);

        if (!result.Success)
            return Ok(HlApiResult<object>.Fail(result.Error ?? "Lỗi khi kiểm tra khách hàng"));

        if (result.Data == null || result.Data.Count == 0)
            return Ok(HlApiResult<object>.Fail(
                "Số điện thoại của bạn chưa có trong hệ thống. Quý Khách vui lòng liên hệ Nhân viên Kinh doanh phụ trách địa bàn hoặc Hotline Công ty để được hỗ trợ."));

        var customer = result.Data[0];
        if (customer.IsCustomer == false)
            return Ok(HlApiResult<object>.Fail(
                "Số điện thoại của bạn chưa có trong hệ thống. Quý Khách vui lòng liên hệ Nhân viên Kinh doanh phụ trách địa bàn hoặc Hotline Công ty để được hỗ trợ."));

        return Ok(HlApiResult<HlCustomerDto>.Ok(customer));
    }

    /// <summary>
    /// Lấy thông tin chi tiết khách hàng sau khi đăng nhập thành công
    /// </summary>
    [HttpGet("customer/{phone}")]
    public async Task<IActionResult> GetCustomer(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return BadRequest(HlApiResult<object>.Fail("Thiếu số điện thoại"));

        var result = await _hlApi.GetCustomerDetailAsync(phone);
        return Ok(result);
    }

    #endregion

    #region Products

    /// <summary>
    /// Lấy danh sách danh mục sản phẩm (Brands)
    /// </summary>
    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands([FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        var result = await _hlApi.GetBrandsAsync(page, limit);
        return Ok(result);
    }

    /// <summary>
    /// Lấy sản phẩm theo danh mục (brand_code)
    /// </summary>
    [HttpGet("brands/{brandCode}/products")]
    public async Task<IActionResult> GetProductsByBrand(string brandCode)
    {
        if (string.IsNullOrWhiteSpace(brandCode))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã danh mục"));

        var result = await _hlApi.GetProductsByBrandAsync(brandCode);
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
    /// Lấy lịch sử đơn hàng của KH theo customer_code (từ DMS Hoa Linh)
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] string? customerCode, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(customerCode))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã khách hàng"));

        // Dùng API get-order-header-zalo theo customer_code
        var result = await _hlApi.GetOrderHeaderZaloAsync(customerCode, "");
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết đơn hàng từ API HL
    /// </summary>
    [HttpGet("orders/{orderNumber}")]
    public async Task<IActionResult> GetOrderDetail(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return BadRequest(HlApiResult<object>.Fail("Thiếu mã đơn hàng"));

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

        var exchangeCode = $"HLGE-{DateTime.Now:yyMMdd}{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

        var entity = new HlGiftExchange(
            Guid.NewGuid(), exchangeCode, request.GiftName, request.PointsRequired, _currentTenant.Id)
        {
            CustomerCode = request.CustomerCode,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            GiftCode = request.GiftCode,
            GiftImageUrl = request.GiftImageUrl,
            Quantity = request.Quantity > 0 ? request.Quantity : 1,
            TotalPointsUsed = request.PointsRequired * (request.Quantity > 0 ? request.Quantity : 1),
            Note = request.Note,
            DeliveryAddress = request.DeliveryAddress,
            Status = HlGiftExchangeStatus.Pending
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
}
