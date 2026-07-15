using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.UrBox;
using Genora.MultiTenancy.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace Genora.MultiTenancy.HttpApi.Controllers;

/// <summary>
/// Controller trung gian cho Zalo Mini App gọi hệ thống UrBox (kho quà eVoucher).
/// Mini App → Genora (UrBoxMiniAppController) → API UrBox.
/// </summary>
[IgnoreAntiforgeryToken]
[RemoteService(false)]
[Area("MultiTenancy")]
[Route("api/mini-app/urbox")]
[AllowAnonymous]
public class UrBoxMiniAppController : MultiTenancyController
{
    private readonly IUrBoxService _urBox;

    public UrBoxMiniAppController(IUrBoxService urBox)
    {
        _urBox = urBox;
    }

    #region Catalog

    /// <summary>Lấy danh sách thương hiệu</summary>
    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands([FromQuery] int? catId, [FromQuery] int? perPage, [FromQuery] int? pageNo)
    {
        var result = await _urBox.GetBrandsAsync(catId, perPage, pageNo);
        return Ok(result);
    }

    /// <summary>Lấy danh sách danh mục theo danh mục cha</summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories([FromQuery] int? parentId, [FromQuery] string? lang)
    {
        var result = await _urBox.GetCategoriesAsync(parentId, lang);
        return Ok(result);
    }

    /// <summary>Lấy danh sách quà tặng (lọc theo brand_id / cat_id / title / mệnh giá)</summary>
    [HttpGet("gifts")]
    public async Task<IActionResult> GetGifts(
        [FromQuery] string? catId, [FromQuery] string? brandId, [FromQuery] string? field,
        [FromQuery] string? lang, [FromQuery] int? stock, [FromQuery] string? title,
        [FromQuery] int? perPage, [FromQuery] int? pageNo)
    {
        var result = await _urBox.GetGiftListAsync(catId, brandId, field, lang, stock, title, perPage, pageNo);
        return Ok(result);
    }

    /// <summary>Lấy chi tiết 1 quà tặng theo id</summary>
    [HttpGet("gifts/{giftId}")]
    public async Task<IActionResult> GetGiftDetail(string giftId, [FromQuery] string? lang)
    {
        if (string.IsNullOrWhiteSpace(giftId))
            return BadRequest("Thiếu mã quà tặng");

        var result = await _urBox.GetGiftDetailAsync(giftId, lang);
        return Ok(result);
    }

    #endregion

    #region Cart / History

    /// <summary>Lấy lịch sử đổi quà theo mã user (site_user_id)</summary>
    [HttpGet("carts")]
    public async Task<IActionResult> GetCartsByUser([FromQuery] string siteUserId)
    {
        if (string.IsNullOrWhiteSpace(siteUserId))
            return BadRequest("Thiếu mã người dùng");

        var result = await _urBox.GetCartListByUserAsync(siteUserId);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết đơn đổi quà cho Mini App theo Id bản ghi HL.AppHlGiftExchanges.
    /// Gộp dữ liệu Genora + UrBox getByTransaction (transaction_id từ InternalNote) + UrBox gift detail (Note + Office theo GiftCode).
    /// </summary>
    [HttpGet("carts/{id}")]
    public async Task<IActionResult> GetCartByTransaction(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Thiếu Id đơn đổi quà");

        var result = await _urBox.GetGiftTransactionDetailAsync(id);
        if (result == null)
            return NotFound("Không tìm thấy đơn đổi quà");

        return Ok(result);
    }

    #endregion

    #region Redeem

    /// <summary>
    /// Đổi quà eVoucher — ký Signature RSA và gọi UrBox cartPayVoucher.
    /// Lưu lịch sử vào AppHlGiftExchanges.
    /// </summary>
    [HttpPost("redeem")]
    public async Task<IActionResult> Redeem([FromBody] UrBoxRedeemInput input)
    {
        if (input == null || input.Items == null || input.Items.Count == 0)
            return BadRequest("Đơn đổi quà phải có ít nhất 1 quà tặng");

        if (string.IsNullOrWhiteSpace(input.SiteUserId))
            return BadRequest("Thiếu mã người dùng");

        var result = await _urBox.CreateOrderEvoucherAsync(input);
        return Ok(result);
    }

    #endregion
}
