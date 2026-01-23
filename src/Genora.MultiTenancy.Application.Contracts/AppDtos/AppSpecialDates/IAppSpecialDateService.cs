using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppSpecialDates
{
    public interface IAppSpecialDateService :
        ICrudAppService<
            SpecialDateDto,
            Guid,
            GetSpecialDateListInput,
            CreateUpdateSpecialDateDto>
    {
    }
}
