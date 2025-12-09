using AutoMapper;
using Genora.MultiTenancy.Apps.AppSettings;
using Genora.MultiTenancy.AuditLogs;
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
