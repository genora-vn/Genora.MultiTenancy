using System;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppBookings;

public interface IAppBookingService :
        ICrudAppService<
            AppBookingDto,
            Guid,
            GetBookingListInput,
            CreateUpdateAppBookingDto>
{
}