using Genora.MultiTenancy.AppDtos.AppEmails;
using Genora.MultiTenancy.AppServices.AppEmails.Jobs;
using Genora.MultiTenancy.DomainModels.AppEmails;
using Genora.MultiTenancy.Enums;
using Microsoft.Extensions.Logging;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.TextTemplating;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.AppServices.AppEmails;

public class AppEmailSenderService : MultiTenancyAppService, IAppEmailSenderService
{
    private readonly IRepository<Email, Guid> _repo;
    private readonly IBackgroundJobManager _jobManager;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IFeatureChecker _featureChecker;
    private readonly ILogger<AppEmailSenderService> _logger;

    public AppEmailSenderService(
        ITenantRepository tenantRepo,
        IDistributedCache<TenantConfigurationCacheItem> tenantCache,
        IRepository<Email, Guid> repo,
        IBackgroundJobManager jobManager,
        ITemplateRenderer templateRenderer,
        IFeatureChecker featureChecker,
        ILogger<AppEmailSenderService> logger
    ) : base(tenantRepo, tenantCache)
    {
        _repo = repo;
        _jobManager = jobManager;
        _templateRenderer = templateRenderer;
        _featureChecker = featureChecker;
        _logger = logger;
    }

    [UnitOfWork(true)]
    public async Task<Guid> EnqueueRawAsync(
        string toEmails,
        string subject,
        string body,
        string? cc = null,
        string? bcc = null,
        Guid? bookingId = null,
        string? bookingCode = null)
    {
        _logger.LogWarning("[AppEmailSenderService] EnqueueRawAsync START TenantId={TenantId} To={To} Subject={Subject}",
            CurrentTenant.Id, toEmails, subject);

        var email = new Email(GuidGenerator.Create())
        {
            TenantId = CurrentTenant.Id,
            TemplateName = "",
            Subject = subject,
            Body = body,
            ToEmails = toEmails,
            CcEmails = cc,
            BccEmails = bcc,
            ModelJson = null,
            Status = EmailStatus.Pending,
            TryCount = 0,
            BookingId = bookingId,
            BookingCode = bookingCode
        };

        try
        {
            await _repo.InsertAsync(email, autoSave: true);

            // ✅ ép commit UoW ngay để loại trừ case chưa flush ra DB
            await CurrentUnitOfWork.SaveChangesAsync();

            _logger.LogWarning("[AppEmailSenderService] Inserted EmailId={EmailId} TenantId={TenantId}",
                email.Id, CurrentTenant.Id);

            await _jobManager.EnqueueAsync(new SendEmailJobArgs
            {
                EmailId = email.Id,
                TenantId = CurrentTenant.Id
            });

            _logger.LogWarning("[AppEmailSenderService] Enqueued SendEmailJob EmailId={EmailId} TenantId={TenantId}",
                email.Id, CurrentTenant.Id);

            return email.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppEmailSenderService] EnqueueRawAsync FAILED TenantId={TenantId}", CurrentTenant.Id);
            throw;
        }
    }

    [UnitOfWork(true)]
    public async Task<Guid> EnqueueTemplateAsync<TModel>(
         string templateName,
         TModel model,
         string toEmails,
         string subject,
         string? cc = null,
         string? bcc = null,
         Guid? bookingId = null,
         string? bookingCode = null)
    {
        _logger.LogWarning("[AppEmailSenderService] EnqueueTemplateAsync START TenantId={TenantId} Template={Template} To={To} Subject={Subject}",
            CurrentTenant.Id, templateName, toEmails, subject);

        try
        {
            // ✅ feature check (nếu fail là biết ngay nhờ log catch)
            await _featureChecker.CheckEnabledAsync(Features.AppEmails.AppEmailFeatures.Management);

            var scriptModel = new ScriptObject();
            scriptModel.Import(model!, renamer: m => m.Name);

            var body = await _templateRenderer.RenderAsync(
                templateName,
                model: null,
                globalContext: new Dictionary<string, object>
                {
                    ["model"] = scriptModel
                }
            );

            var email = new Email(GuidGenerator.Create())
            {
                TenantId = CurrentTenant.Id,
                TemplateName = templateName,
                Subject = subject,
                Body = body,
                ToEmails = toEmails,
                CcEmails = cc,
                BccEmails = bcc,
                ModelJson = JsonSerializer.Serialize(model),
                Status = EmailStatus.Pending,
                TryCount = 0,
                BookingId = bookingId,
                BookingCode = bookingCode
            };

            await _repo.InsertAsync(email, autoSave: true);
            await CurrentUnitOfWork.SaveChangesAsync();

            _logger.LogWarning("[AppEmailSenderService] Inserted EmailId={EmailId} TenantId={TenantId}",
                email.Id, CurrentTenant.Id);

            // ✅ FIX: 반드시 truyền TenantId vào args (trước bạn thiếu chỗ này)
            await _jobManager.EnqueueAsync(new SendEmailJobArgs
            {
                EmailId = email.Id,
                TenantId = CurrentTenant.Id
            });

            _logger.LogWarning("[AppEmailSenderService] Enqueued SendEmailJob EmailId={EmailId} TenantId={TenantId}",
                email.Id, CurrentTenant.Id);

            return email.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppEmailSenderService] EnqueueTemplateAsync FAILED TenantId={TenantId} Template={Template}",
                CurrentTenant.Id, templateName);
            throw;
        }
    }
}
