using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genora.MultiTenancy.Web.Helpers;

public static class EnumSelectList
{
    public static List<SelectListItem> GetLocalizedEnumSelectList<TEnum>(IStringLocalizer localizer)
        where TEnum : struct, Enum
    {
        return Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .Select(e => new SelectListItem
            {
                Value = Convert.ToByte(e).ToString(),
                Text = localizer[$"{typeof(TEnum).Name}:{e}"]
            }).ToList();
    }
}