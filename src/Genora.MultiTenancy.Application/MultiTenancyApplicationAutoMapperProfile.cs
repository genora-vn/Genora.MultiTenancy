using AutoMapper;
using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppMembershipTiers;
using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.AppDtos.AppSettings;
using Genora.MultiTenancy.AppDtos.ZaloAuths;
using Genora.MultiTenancy.Apps.AppSettings;
using Genora.MultiTenancy.AuditLogs;
using Genora.MultiTenancy.DomainModels.AppBookingPlayers;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppMembershipTiers;
using Genora.MultiTenancy.DomainModels.AppNews;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
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
        CreateMap<News, AppNewsDto>();
        CreateMap<CreateUpdateAppNewsDto, News>();
        CreateMap<MiniAppNewsData, News>();
        CreateMap<News, MiniAppNewsData>();
        #endregion

        #region Booking auto mapper profile
        CreateMap<Booking, AppBookingDto>();
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
    }
}
