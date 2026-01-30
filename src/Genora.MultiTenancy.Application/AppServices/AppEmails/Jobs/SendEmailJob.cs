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
        _logger.LogWarning("[SendEmailJob] START TenantId={TenantId} EmailId={EmailId}", args.TenantId, args.EmailId);

        using (_currentTenant.Change(args.TenantId))
        {
            // ✅ tránh throw mù mờ, log rõ mail có tồn tại không
            var mail = await _repo.FirstOrDefaultAsync(x => x.Id == args.EmailId);
            if (mail == null)
            {
                _logger.LogError("[SendEmailJob] Email not found (or filtered). TenantId={TenantId} EmailId={EmailId}",
                    args.TenantId, args.EmailId);
                return;
            }

            // ✅ check lệch tenant (debug cực nhanh)
            if (mail.TenantId != args.TenantId)
            {
                _logger.LogWarning("[SendEmailJob] Tenant mismatch. ArgsTenantId={ArgsTenantId} MailTenantId={MailTenantId} EmailId={EmailId}",
                    args.TenantId, mail.TenantId, mail.Id);
            }

            if (mail.Status == EmailStatus.Sent || mail.Status == EmailStatus.Abandoned)
            {
                _logger.LogWarning("[SendEmailJob] SKIP Status={Status} TenantId={TenantId} EmailId={EmailId}",
                    mail.Status, args.TenantId, mail.Id);
                return;
            }

            mail.Status = EmailStatus.Sending;
            mail.LastTryTime = DateTime.UtcNow;
            await _repo.UpdateAsync(mail, autoSave: true);

            try
            {
                var tos = (mail.ToEmails ?? "")
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                _logger.LogWarning("[SendEmailJob] SENDING ToCount={ToCount} Subject={Subject} TenantId={TenantId} EmailId={EmailId}",
                    tos.Length, mail.Subject, args.TenantId, mail.Id);

                foreach (var to in tos)
                {
                    await _emailSender.SendAsync(
                        to,
                        mail.Subject,
                        mail.Body,
                        isBodyHtml: false // giữ nguyên như bạn đang dùng
                    );
                }

                mail.Status = EmailStatus.Sent;
                mail.SentTime = DateTime.UtcNow;
                mail.LastError = null;
                mail.NextTryTime = null;

                await _repo.UpdateAsync(mail, autoSave: true);

                _logger.LogWarning("[SendEmailJob] SENT OK TenantId={TenantId} EmailId={EmailId}", args.TenantId, mail.Id);
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
                    "[SendEmailJob] FAILED TenantId={TenantId} EmailId={EmailId} Try={TryCount} NextTry={NextTryTime}",
                    args.TenantId, mail.Id, mail.TryCount, mail.NextTryTime);

                throw; // giữ nguyên để ABP đánh fail job
            }
        }
    }
}
