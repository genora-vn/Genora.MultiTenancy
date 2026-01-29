using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Volo.Abp;

namespace Genora.MultiTenancy.Helpers;

public class ErrorHelper
{
    private static readonly Regex NamedTokenRegex = new(@"\{([A-Za-z_][A-Za-z0-9_]*)\}", RegexOptions.Compiled);

    private static string GetTemplate(IStringLocalizer localizer, string code)
    {
        var template = localizer[code].Value;

        if (string.IsNullOrWhiteSpace(template))
            return code;

        return template;
    }

    private static IDictionary<string, object?> ToNamedArgs(object args)
    {
        if (args is IDictionary<string, object?> dictObjNullable)
            return dictObjNullable;

        if (args is IDictionary<string, object> dictObj)
            return dictObj.ToDictionary(k => k.Key, v => (object?)v.Value);

        // anonymous object / POCO => reflect properties
        return args.GetType()
                   .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                   .Where(p => p.CanRead)
                   .ToDictionary(p => p.Name, p => (object?)p.GetValue(args));
    }

    private static string FormatTemplate(string template, object? args)
    {
        if (args == null) return template;

        // If args is object[] => positional formatting {0}...
        if (args is object[] arr)
        {
            if (arr.Length == 0) return template;

            try
            {
                return string.Format(CultureInfo.CurrentCulture, template, arr);
            }
            catch
            {
                return template;
            }
        }

        // Named tokens formatting: {PhoneNumber}, {CustomerCode}, {MaxMB}...
        try
        {
            var named = ToNamedArgs(args);

            // Replace {Name} tokens
            var replaced = NamedTokenRegex.Replace(template, m =>
            {
                var key = m.Groups[1].Value;
                if (named.TryGetValue(key, out var val) && val != null)
                {
                    // format using current culture for numbers, etc.
                    return Convert.ToString(val, CultureInfo.CurrentCulture) ?? "";
                }
                return m.Value; // keep original if missing
            });

            return replaced;
        }
        catch
        {
            return template;
        }
    }

    // Format by localization code with either:
    // - positional args: new object[] { a, b }
    // - named args: new { PhoneNumber = "...", MaxMB = 15 }
    private static string Format(IStringLocalizer localizer, string code, object? args = null)
    {
        var template = GetTemplate(localizer, code);
        return FormatTemplate(template, args);
    }

    // ===== Import Errors =====

    // Message chung: KHÔNG nhét rowNumber vào message nữa, chỉ đưa vào Data
    public static BusinessException ImportError(
        IStringLocalizer localizer,
        string code,
        int rowNumber,
        string? detailCode = null,
        object? detailArgs = null)
    {
        var message = Format(localizer, code);

        var ex = new BusinessException(code, message)
            .WithData("RowNumber", rowNumber);

        if (!string.IsNullOrWhiteSpace(detailCode))
        {
            var detail = Format(localizer, detailCode!, detailArgs);
            if (!string.IsNullOrWhiteSpace(detail) && detail != detailCode)
                ex.WithData("ExceptionMessage", detail);
        }

        return ex;
    }

    // Message chung, chi tiết để trong Data (Field/Value/ExceptionMessage)
    public static BusinessException ImportError(
        IStringLocalizer localizer,
        string code,
        int rowNumber,
        string field,
        object? value = null,
        string? detailCode = null,
        object? detailArgs = null)
    {
        var message = Format(localizer, code);

        var ex = new BusinessException(code, message)
            .WithData("RowNumber", rowNumber)
            .WithData("Field", field);

        if (value != null)
            ex.WithData("Value", value);

        if (!string.IsNullOrWhiteSpace(detailCode))
        {
            var detail = Format(localizer, detailCode!, detailArgs);
            if (!string.IsNullOrWhiteSpace(detail) && detail != detailCode)
                ex.WithData("ExceptionMessage", detail);
        }

        return ex;
    }

    // ===== Business Errors =====

    // message: lấy từ localization theo code
    // ExceptionMessage: dòng chi tiết (FE đang render dòng 2 theo key này)
    public static BusinessException BusinessError(
        IStringLocalizer localizer,
        string code,
        object? messageArgs = null,
        string? detailCode = null,
        object? detailArgs = null)
    {
        var message = Format(localizer, code, messageArgs);

        var ex = new BusinessException(code, message);

        if (!string.IsNullOrWhiteSpace(detailCode))
        {
            var detail = Format(localizer, detailCode!, detailArgs);
            if (!string.IsNullOrWhiteSpace(detail) && detail != detailCode)
                ex.WithData("ExceptionMessage", detail);
        }

        return ex;
    }
}
