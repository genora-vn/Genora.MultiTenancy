using Microsoft.Extensions.Configuration;

namespace Genora.MultiTenancy.Helpers;

public static class ImageHelper
{
    public static string? NormalizeThumb(IConfiguration configuration, string? url)
    {
        if (!string.IsNullOrEmpty(url) && url.StartsWith("/uploads"))
        {
            return configuration["App:AppUrl"] + url;
        }
        return url;
    }
}