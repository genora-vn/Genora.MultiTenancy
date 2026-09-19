using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Genora.MultiTenancy.Hlg;
// Value collections owned by the product aggregate, stored using HLG's JSON convention.
public class HlgProductContent
{
    [StringLength(100)] public string? BadgeText { get; set; }
    public Guid? GameId { get; set; }
    [StringLength(150)] public string? CallToActionText { get; set; }
    [StringLength(1000)] public string? PromotionText { get; set; }
    public List<HlgKnowledgeSection> Knowledge { get; set; } = new();
    public List<HlgProductMedia> Media { get; set; } = new();
    public List<Guid> RelatedProductIds { get; set; } = new();
}
public class HlgKnowledgeSection
{
    [Required, StringLength(500)] public string Title { get; set; } = "";
    [Required, StringLength(50000)] public string Content { get; set; } = "";
}
public enum HlgMediaKind { Image = 1, Video = 2 }
public enum HlgMediaPlacement { Hero = 1, Information = 2, Knowledge = 3, Related = 4 }
public class HlgProductMedia
{
    [EnumDataType(typeof(HlgMediaKind))] public HlgMediaKind Kind { get; set; } = HlgMediaKind.Image;
    [EnumDataType(typeof(HlgMediaPlacement))] public HlgMediaPlacement Placement { get; set; } = HlgMediaPlacement.Hero;
    [Required, StringLength(1000)] public string Url { get; set; } = "";
    [StringLength(1000)] public string? PosterUrl { get; set; }
    [StringLength(250)] public string? AltText { get; set; }
}
public enum HlgContentSlot { HomeBanner = 1, KnowledgeCard = 2, GamesCard = 3, RankingCard = 4, ShareLink = 5 }
