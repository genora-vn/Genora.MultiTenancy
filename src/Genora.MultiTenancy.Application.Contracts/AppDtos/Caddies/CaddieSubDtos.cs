using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Caddies;

public class CaddieLanguageDto : EntityDto<Guid>
{
    public string LanguageCode { get; set; } = null!;
    public string LanguageName { get; set; } = null!;
    public string? NativeName { get; set; }
    public byte Status { get; set; }
    public int SortOrder { get; set; }
}

public class CreateUpdateLanguageDto
{
    public string LanguageCode { get; set; } = null!;
    public string LanguageName { get; set; } = null!;
    public string? NativeName { get; set; }
    public byte Status { get; set; } = 1;
    public int SortOrder { get; set; }
}

public class CaddieSkillDto : EntityDto<Guid>
{
    public string SkillCode { get; set; } = null!;
    public string SkillName { get; set; } = null!;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public byte Status { get; set; }
}

public class CreateUpdateCaddieSkillDto
{
    public string SkillCode { get; set; } = null!;
    public string SkillName { get; set; } = null!;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public byte Status { get; set; } = 1;
}
