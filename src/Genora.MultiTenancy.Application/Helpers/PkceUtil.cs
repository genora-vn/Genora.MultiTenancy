using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Helpers;

public static class PkceUtil
{
    public static string CreateCodeVerifier()
        => Base64Url(RandomNumberGenerator.GetBytes(32));

    public static string CreateCodeChallengeS256(string verifier)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.ASCII.GetBytes(verifier));
        return Base64Url(bytes);
    }

    private static string Base64Url(byte[] input)
        => Convert.ToBase64String(input)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
}