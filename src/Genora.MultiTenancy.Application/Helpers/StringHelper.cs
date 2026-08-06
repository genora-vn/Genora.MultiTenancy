using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp.Security.Encryption;

namespace Genora.MultiTenancy.Helpers;

public class StringHelper
{
    public static string NormalizeBankTransferNote(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        input = input.Normalize();

        return Regex.Replace(input, @"[^a-zA-Z0-9\s]", "");
    }
}
