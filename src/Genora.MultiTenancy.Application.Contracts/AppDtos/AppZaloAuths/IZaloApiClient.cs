using System.Threading;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public interface IZaloApiClient
{
    Task<string> SendZnsAsync(object payload, CancellationToken ct);
    Task<string> SendOaMessageAsync(object payload, CancellationToken ct);

    Task<ZaloMeResponse> GetZaloMeAsync(string accessToken, CancellationToken ct);
    Task<ZaloDecodePhoneResponse> DecodePhoneAsync(string code, string accessToken, CancellationToken ct);
}