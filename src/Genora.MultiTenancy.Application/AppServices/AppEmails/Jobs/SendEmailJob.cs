using Genora.MultiTenancy.DomainModels.AppEmails;
using Genora.MultiTenancy.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mail;
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
            var mail = await _repo.FirstOrDefaultAsync(x => x.Id == args.EmailId);
            if (mail == null)
            {
                _logger.LogError("[SendEmailJob] Email not found (or filtered). TenantId={TenantId} EmailId={EmailId}",
                    args.TenantId, args.EmailId);
                return;
            }

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
                    .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var ccs = (mail.CcEmails ?? "")
                    .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var bccs = (mail.BccEmails ?? "")
                    .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                _logger.LogWarning("[SendEmailJob] SENDING ToCount={ToCount} CcCount={CcCount} BccCount={BccCount} Subject={Subject} TenantId={TenantId} EmailId={EmailId}",
                    tos.Length, ccs.Length, bccs.Length, mail.Subject, args.TenantId, mail.Id);

                if (tos.Length == 0)
                {
                    _logger.LogWarning("[SendEmailJob] No recipients. TenantId={TenantId} EmailId={EmailId}", args.TenantId, mail.Id);
                    mail.Status = EmailStatus.Abandoned;
                    mail.LastError = "No recipients (ToEmails is empty)";
                    await _repo.UpdateAsync(mail, autoSave: true);
                    return;
                }

                // Gửi email với MailMessage để hỗ trợ đầy đủ To, Cc, Bcc
                // Người nhận thấy được danh sách To và Cc (Bcc thì ẩn)
                var message = new MailMessage();

                foreach (var to in tos)
                {
                    message.To.Add(new MailAddress(to));
                }

                foreach (var cc in ccs)
                {
                    message.CC.Add(new MailAddress(cc));
                }

                foreach (var bcc in bccs)
                {
                    message.Bcc.Add(new MailAddress(bcc));
                }

                message.Subject = mail.Subject;
                message.Body = mail.Body;
                message.IsBodyHtml = true;

                await _emailSender.SendAsync(message);

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
