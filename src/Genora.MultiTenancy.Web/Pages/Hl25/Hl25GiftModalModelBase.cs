using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.Hl25;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.Web.Pages.Hl25;

public abstract class Hl25GiftModalModelBase : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateHl25GiftDto Gift { get; set; } = new();

    [BindProperty]
    public string? GiftValue { get; set; }

    [BindProperty]
    public IFormFile? GiftImageFile { get; set; }

    [BindProperty]
    public IFormFile? WheelImageFile { get; set; }

    protected readonly IHl25GiftAppService GiftService;

    protected Hl25GiftModalModelBase(IHl25GiftAppService giftService) => GiftService = giftService;

    public static decimal? ParseVndValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Trim();
        if (!Regex.IsMatch(value, @"^(?:\d+|\d{1,3}(?:\.\d{3})+)(?:,\d{1,2})?$") ||
            !decimal.TryParse(value.Replace(".", "").Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var amount) || amount > 9999999999999999.99m)
            throw new BusinessException("Hl25:InvalidGiftValue");
        return amount;
    }

    protected async Task PrepareGiftAsync()
    {
        ValidateModel();
        Gift.Value = ParseVndValue(GiftValue);
        ValidateImage(GiftImageFile);
        ValidateImage(WheelImageFile);
        if (GiftImageFile != null)
            Gift.ImageUrl = await UploadAsync(GiftImageFile);
        if (WheelImageFile != null)
            Gift.WheelImageUrl = await UploadAsync(WheelImageFile);
    }

    private static void ValidateImage(IFormFile? file)
    {
        if (file == null) return;
        if (file.Length <= 0 || (file.ContentType != "image/png" && file.ContentType != "image/jpeg"))
            throw new BusinessException("Hl25:InvalidGiftImage");
        if (file.Length > Hl25Consts.MaxCardImageSizeBytes)
            throw new BusinessException("Hl25:AssetTooLarge");
    }

    private async Task<string> UploadAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        using var content = new RemoteStreamContent(stream, file.FileName, file.ContentType, file.Length);
        return await GiftService.UploadGiftImageAsync(content);
    }
}
