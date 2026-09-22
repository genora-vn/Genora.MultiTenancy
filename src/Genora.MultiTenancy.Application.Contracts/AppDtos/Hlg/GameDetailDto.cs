

using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.Hlg
{
    public class GameDetailDto
    {
        public string? BadgeText { get; set; }
        public string? BannerUrl { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public string? Rules { get; set; }
        public string? RewardDescription { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public int TotalQuestions { get; set; }
        public List<TopPlayer> TopPlayers { get; set; } = new List<TopPlayer>();
        public List<Prizes> Prizes { get; set; } = new List<Prizes> { };
    }
    public class TopPlayer
    {
        public Guid Id {  set; get; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public decimal TotalPoint { get; set; }
    }
    public class Prizes
    {
        public int Order { get; set; }
        public string Name {  set; get; }
        public int Quantity { get; set; }
    }
}
