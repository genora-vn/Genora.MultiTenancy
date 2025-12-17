using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppSettings;

public interface IAppSettingService :
    ICrudAppService< //Defines CRUD methods
        AppSettingDto, //Used to show AppSettings
        Guid, //Primary key of the AppSettings entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        CreateUpdateAppSettingDto> //Used to create/update a AppSettings
{

}