using System;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppEmails;

public interface IAppEmailService :
        ICrudAppService<
            AppEmailDto,
            Guid,
            GetEmailListInput,
            CreateUpdateEmailDto>
{
}