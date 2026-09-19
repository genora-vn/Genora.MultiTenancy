using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Genora.MultiTenancy.Hlg;
using Volo.Abp.Authorization;
namespace Genora.MultiTenancy.AppServices.Hlg;
public static class HlgContentValidation
{
    public static void Localized(Action action, Func<string,string> localize) {
        try { action(); } catch (ValidationException ex) when (ex.Message.StartsWith("Hlg:")) { throw new Volo.Abp.UserFriendlyException(localize(ex.Message)); }
    }
    public static void Scope(Guid? actual, Guid? expected) { if (actual != expected) throw new AbpAuthorizationException(); }
    public static void Validate(object input) => Validator.ValidateObject(input, new ValidationContext(input), true);
    public static void Url(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        if (value.Any(char.IsControl) || value.Contains('\\')) throw new ValidationException("Hlg:InvalidUrl");
        if (value.StartsWith("/") && !value.StartsWith("//")) return;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || (uri.Scheme != "https" && uri.Scheme != "http"))
            throw new ValidationException("Hlg:InvalidUrl");
    }
    public static void Product(HlgProductContent details, Guid? productId)
    {
        if (details == null || details.Knowledge == null || details.Media == null || details.RelatedProductIds == null)
            throw new ValidationException("Hlg:InvalidContent");
        Validate(details);
        if (details.Knowledge.Count > 50 || details.Media.Count > 100 || details.RelatedProductIds.Count > 100)
            throw new ValidationException("Hlg:TooManyContentItems");
        foreach (var section in details.Knowledge) { if (section == null) throw new ValidationException("Hlg:InvalidContent"); Validate(section); }
        foreach (var media in details.Media) { if (media == null) throw new ValidationException("Hlg:InvalidContent"); Validate(media); Url(media.Url); Url(media.PosterUrl); }
        if (details.RelatedProductIds.Any(x => x == Guid.Empty || x == productId) || details.RelatedProductIds.Distinct().Count() != details.RelatedProductIds.Count)
            throw new ValidationException("Hlg:InvalidRelatedProducts");
    }
}
