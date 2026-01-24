using Genora.MultiTenancy.DomainModels.AppEmails;
using Genora.MultiTenancy.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.AppServices.AppEmails.Jobs;

public class SendEmailJob : AsyncBackgroundJob<SendEmailJobArgs>, ITransientDependency
{
    private const int MaxTry = 5;

    private readonly IRepository<Email, Guid> _repo;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<SendEmailJob> _logger;
    private readonly ICurrentTenant _currentTenant;

    public SendEmailJob(
        IRepository<Email, Guid> repo,
        IEmailSender emailSender,
        ILogger<SendEmailJob> logger,
        ICurrentTenant currentTenant)
    {
        _repo = repo;
        _emailSender = emailSender;
        _logger = logger;
        _currentTenant = currentTenant;
    }

    [UnitOfWork(true)]
    public override async Task ExecuteAsync(SendEmailJobArgs args)
    {
        using (_currentTenant.Change(args.TenantId))
        {
            var mail = await _repo.GetAsync(args.EmailId);

            if (mail.Status == EmailStatus.Sent || mail.Status == EmailStatus.Abandoned)
                return;

            mail.Status = EmailStatus.Sending;
            mail.LastTryTime = DateTime.UtcNow;
            await _repo.UpdateAsync(mail, autoSave: true);

            try
            {
                var tos = mail.ToEmails.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var to in tos)
                {
                    await _emailSender.SendAsync(
                        to,
                        mail.Subject,
                        mail.Body,
                        isBodyHtml: false
                    );
                }

                mail.Status = EmailStatus.Sent;
                mail.SentTime = DateTime.UtcNow;
                mail.LastError = null;
                mail.NextTryTime = null;

                await _repo.UpdateAsync(mail, autoSave: true);
            }
            catch (Exception ex)
            {
                mail.TryCount += 1;
                mail.LastError = ex.ToString();

                if (mail.TryCount >= MaxTry)
                {
                    mail.Status = EmailStatus.Abandoned;
                    mail.NextTryTime = null;
                }
                else
                {
                    mail.Status = EmailStatus.Failed;
                    var minutes = mail.TryCount switch
                    {
                        1 => 1,
                        2 => 5,
                        3 => 15,
                        4 => 60,
                        _ => 180
                    };
                    mail.NextTryTime = DateTime.UtcNow.AddMinutes(minutes);
                }

                await _repo.UpdateAsync(mail, autoSave: true);

                _logger.LogError(ex,
                    "Send email failed. TenantId={TenantId}, EmailId={EmailId}, Try={TryCount}",
                    args.TenantId, mail.Id, mail.TryCount);

                throw;
            }
        }
    }
}
