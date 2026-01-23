using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppEmails;

public interface IAppEmailSenderService
{
    Task<Guid> EnqueueRawAsync(string toEmails, string subject, string body, string? cc = null, string? bcc = null,
        Guid? bookingId = null, string? bookingCode = null);

    Task<Guid> EnqueueTemplateAsync<TModel>(string templateName, TModel model, string toEmails, string subject,
        string? cc = null, string? bcc = null, Guid? bookingId = null, string? bookingCode = null);
}