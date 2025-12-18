using System.Threading;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public interface IZaloApiClient
{
    Task<string> SendZnsAsync(object payload);
    Task<string> SendOaMessageAsync(object payload);

    Task<ZaloMeResponse> GetMeAsync();
    Task<ZaloPhoneResponse> DecodePhoneAsync(string code, string accessToken, CancellationToken ct);
}

public record ZaloMeResponse(string Id, string Name, string? PictureUrl);