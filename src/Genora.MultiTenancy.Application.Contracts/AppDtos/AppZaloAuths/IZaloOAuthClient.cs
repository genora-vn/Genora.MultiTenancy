using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;

public record ZaloTokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);

public interface IZaloOAuthClient
{
    Task<ZaloTokenResponse> ExchangeCodeAsync(
        string appId, string appSecret, string code, string codeVerifier, string redirectUri, string? oaId);

    Task<ZaloTokenResponse> RefreshTokenAsync(
        string appId, string appSecret, string refreshToken, string? oaId);
}