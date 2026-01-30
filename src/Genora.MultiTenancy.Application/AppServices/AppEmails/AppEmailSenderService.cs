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
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.TextTemplating;

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

    public async Task<Guid> EnqueueRawAsync(
        string toEmails,
        string subject,
        string body,
        string? cc = null,
        string? bcc = null,
        Guid? bookingId = null,
        string? bookingCode = null)
    {
        var tenantId = CurrentTenant.Id; // nullable
        _logger.LogWarning("[AppEmail] EnqueueRawAsync START TenantId={TenantId} To={To} Subject={Subject} BookingCode={BookingCode}",
            tenantId, toEmails, subject, bookingCode);

        if (tenantId == null)
        {
            // Nếu gọi từ tenant mà TenantId=null => tenant resolve đang fail
            _logger.LogError("[AppEmail] TenantId is NULL in EnqueueRawAsync. This call will be treated as HOST.");
        }

        var email = new Email(GuidGenerator.Create())
        {
            TenantId = tenantId,
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

        await _repo.InsertAsync(email, autoSave: true);

        _logger.LogWarning("[AppEmail] Inserted EmailId={EmailId} TenantId={TenantId} Status={Status}",
            email.Id, email.TenantId, email.Status);

        await _jobManager.EnqueueAsync(new SendEmailJobArgs
        {
            EmailId = email.Id,
            TenantId = tenantId
        });

        _logger.LogWarning("[AppEmail] Enqueued SendEmailJob EmailId={EmailId} TenantId={TenantId}", email.Id, tenantId);

        return email.Id;
    }

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
        var tenantId = CurrentTenant.Id;

        _logger.LogWarning("[AppEmail] EnqueueTemplateAsync START TenantId={TenantId} Template={Template} To={To} Subject={Subject} BookingCode={BookingCode}",
            tenantId, templateName, toEmails, subject, bookingCode);

        // Nếu tenant chưa bật feature, nó sẽ throw => log để biết rõ
        try
        {
            await _featureChecker.CheckEnabledAsync(Features.AppEmails.AppEmailFeatures.Management);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[AppEmail] Feature disabled or feature check failed. TenantId={TenantId} Template={Template}",
                tenantId, templateName);
            throw;
        }

        // ✅ Ép Scriban giữ PascalCase + biến tên "model"
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
            TenantId = tenantId,
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

        _logger.LogWarning("[AppEmail] Inserted EmailId={EmailId} TenantId={TenantId} Template={Template} Status={Status}",
            email.Id, email.TenantId, templateName, email.Status);

        // ✅ FIX: luôn truyền TenantId để job chạy đúng scope tenant
        await _jobManager.EnqueueAsync(new SendEmailJobArgs
        {
            EmailId = email.Id,
            TenantId = tenantId
        });

        _logger.LogWarning("[AppEmail] Enqueued SendEmailJob EmailId={EmailId} TenantId={TenantId}", email.Id, tenantId);

        return email.Id;
    }
}
