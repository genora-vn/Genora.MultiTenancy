using AutoMapper;
using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppEmails;
using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppMembershipTiers;
using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.AppDtos.AppOptionExtend;
using Genora.MultiTenancy.AppDtos.AppPromotionTypes;
using Genora.MultiTenancy.AppDtos.AppSettings;
using Genora.MultiTenancy.AppDtos.AppSpecialDates;
using Genora.MultiTenancy.AppDtos.ZaloAuths;
using Genora.MultiTenancy.Apps.AppSettings;
using Genora.MultiTenancy.AuditLogs;
using Genora.MultiTenancy.DomainModels.AppBookingPlayers;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppEmails;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppMembershipTiers;
using Genora.MultiTenancy.DomainModels.AppNews;
using Genora.MultiTenancy.DomainModels.AppOptionExtend;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Genora.MultiTenancy.DomainModels.AppSpecialDates;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using System.Linq;
using Volo.Abp.AuditLogging;
using static Genora.MultiTenancy.Permissions.MultiTenancyPermissions;

namespace Genora.MultiTenancy;

public class MultiTenancyApplicationAutoMapperProfile : Profile
{
    public MultiTenancyApplicationAutoMapperProfile()
    {
        #region AppSetting auto mapper profile
        CreateMap<AppSetting, AppSettingDto>();
        CreateMap<CreateUpdateAppSettingDto, AppSetting>();
        #endregion

        #region CustomerType auto mapper profile
        CreateMap<CustomerType, AppCustomerTypeDto>();
        CreateMap<CreateUpdateAppCustomerTypeDto, CustomerType>();
        #endregion

        #region AppGolfCourse auto mapper profile
        CreateMap<GolfCourse, AppGolfCourseDto>();
        CreateMap<GolfCourse, GolfCourseListData>();
        CreateMap<CreateUpdateAppGolfCourseDto, GolfCourse>();
        #endregion

        #region AppMembershipTier auto mapper profile
        CreateMap<MembershipTier, AppMembershipTierDto>();
        CreateMap<CreateUpdateAppMembershipTierDto, MembershipTier>();
        #endregion

        #region AppCustomer auto mapper profile
        CreateMap<Customer, AppCustomerDto>();
        CreateMap<CreateUpdateAppCustomerDto, Customer>();
        #endregion

        #region AppCalendarSlot auto mapper profile
        CreateMap<CalendarSlot, AppCalendarSlotDto>();
        CreateMap<CreateUpdateAppCalendarSlotDto, CalendarSlot>();
        #endregion

        #region News auto mapper profile
        CreateMap<News, AppNewsDto>()
        .ForMember(d => d.RelatedNewsIds,
            opt => opt.MapFrom(s => s.RelatedNewsLinks.Select(x => x.RelatedNewsId).ToList()));
        CreateMap<CreateUpdateAppNewsDto, News>();
        CreateMap<MiniAppNewsData, News>();
        CreateMap<News, MiniAppNewsData>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => (NewsStatus)s.Status));
        CreateMap<News, MiniAppRelatedNewsData>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => (NewsStatus)s.Status));

        #endregion

        #region Booking auto mapper profile
        CreateMap<Booking, AppBookingDto>();
        CreateMap<Booking, BookingDetailData>();
        CreateMap<Booking, BookingListData>();
        CreateMap<CreateUpdateAppBookingDto, Booking>();
        CreateMap<BookingPlayer, AppBookingPlayerDto>();
        #endregion

        #region ZaloAuth, ZaloLog auto mapper profile
        CreateMap<ZaloAuth, AppZaloAuthDto>();
        CreateMap<CreateUpdateZaloAuthDto, ZaloAuth>()
            .ForMember(x => x.Id, opt => opt.Ignore());

        CreateMap<ZaloLog, AppZaloLogDto>();
        #endregion

        #region MiniAppCustomer auto mapper profile
        CreateMap<Customer, MiniAppCustomerDto>();
        CreateMap<Customer, CustomerData>();

        #endregion

        CreateMap<AuditLog, AuditLogListDto>()
            .ForMember(d => d.HasException, o => o.MapFrom(s => !string.IsNullOrEmpty(s.Exceptions)))
            .ForMember(d => d.TenantId, o => o.MapFrom(s => s.TenantId));   // ★

        CreateMap<AuditLog, AuditLogDetailDto>();
        CreateMap<AuditLogAction, AuditLogActionDto>();
        CreateMap<EntityChange, EntityChangeDto>()
            .ForMember(d => d.ChangeType, o => o.MapFrom(s => s.ChangeType.ToString()));
        CreateMap<EntityPropertyChange, EntityPropertyChangeDto>();

        CreateMap<OptionExtend, AppOptionExtendDto>();
        CreateMap<Genora.MultiTenancy.DomainModels.AppPromotionTypes.PromotionType, AppPromotionTypeDto>();
        CreateMap<AppPromotionTypeDto, Genora.MultiTenancy.DomainModels.AppPromotionTypes.PromotionType>();
        CreateMap<AppPromotionTypeDto, CreateUpdatePromotionTypeDto>();
        CreateMap<CreateUpdatePromotionTypeDto, Genora.MultiTenancy.DomainModels.AppPromotionTypes.PromotionType>();

        #region SpecialDate auto mapper profile
        CreateMap<SpecialDate, SpecialDateDto>()
            .ForMember(d => d.Dates, opt => opt.MapFrom(s => FormatDateTimeHelper.DeserializeDates(s.DatesJson)));

        CreateMap<CreateUpdateSpecialDateDto, SpecialDate>()
            .ForMember(d => d.DatesJson, opt => opt.MapFrom(s => FormatDateTimeHelper.SerializeDates(s.Dates)))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => (s.Name ?? "").Trim()))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.GolfCourseId, opt => opt.MapFrom(s => s.GolfCourseId))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive))
            // các field hệ thống không map
            .ForMember(d => d.TenantId, opt => opt.Ignore())
            .ForMember(d => d.ExtraProperties, opt => opt.Ignore())
            .ForMember(d => d.ConcurrencyStamp, opt => opt.Ignore());
        #endregion

        #region AppEmail auto mapper profile
        CreateMap<Email, AppEmailDto>();

        CreateMap<CreateUpdateEmailDto, Email>()
            // Không map các field hệ thống / audit
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.TenantId, opt => opt.Ignore())
            .ForMember(x => x.ExtraProperties, opt => opt.Ignore())
            .ForMember(x => x.ConcurrencyStamp, opt => opt.Ignore())
            .ForMember(x => x.CreationTime, opt => opt.Ignore())
            .ForMember(x => x.CreatorId, opt => opt.Ignore())
            .ForMember(x => x.LastModificationTime, opt => opt.Ignore())
            .ForMember(x => x.LastModifierId, opt => opt.Ignore())
            .ForMember(x => x.IsDeleted, opt => opt.Ignore())
            .ForMember(x => x.DeleterId, opt => opt.Ignore())
            .ForMember(x => x.DeletionTime, opt => opt.Ignore());
        #endregion
    }
}
