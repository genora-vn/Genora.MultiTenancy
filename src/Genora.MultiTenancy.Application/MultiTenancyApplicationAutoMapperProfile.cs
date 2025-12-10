using AutoMapper;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppMembershipTiers;
using Genora.MultiTenancy.Apps.AppSettings;
using Genora.MultiTenancy.AuditLogs;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppMembershipTiers;
using Volo.Abp.AuditLogging;

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
