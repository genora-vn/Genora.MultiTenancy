
using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppGolfCourses
{
    public class MiniAppGolfCourseListDto : ZaloBaseResponse
    {
        public PagedResultDto<GolfCourseListData> Data { get; set; }
    }
    public class GolfCourseListData
    {
        public Guid Id { get; set; }
        public string Code { get; set; }              // Mã sân
        public string Name { get; set; }              // Tên sân

        public string Address { get; set; }
        public string Province { get; set; }
        public string Phone { get; set; }

        public string Website { get; set; }
        public string FanpageUrl { get; set; }

        public string ShortDescription { get; set; }
        public string AvatarUrl { get; set; }
        public string BannerUrl { get; set; }

        public string CancellationPolicy { get; set; }
        public string TermsAndConditions { get; set; }

        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }

        public byte BookingStatus { get; set; }    
        public string? FrameTimes { get; set; }

        public string? NumberHoles { get; set; }

        public string? Utilities { get; set; }

        public List<int> FrameTimeOfDay => !string.IsNullOrWhiteSpace(FrameTimes) ? FrameTimes.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToList() : new List<int>();
        public List<string> NumberHolesStr => !string.IsNullOrWhiteSpace(NumberHoles) ? NumberHoles.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToList() : new List<string>();
        public List<int> Ulitity => !string.IsNullOrWhiteSpace(Utilities) ? Utilities.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToList() : new List<int>();
    }
}
