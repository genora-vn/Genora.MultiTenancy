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

namespace Genora.MultiTenancy.AppServices.AppEmails;

public class AppEmailSenderService : MultiTenancyAppService, IAppEmailSenderService
{
    private readonly IRepository<Email, Guid> _repo;
    private readonly IBackgroundJobManager _jobManager;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IFeatureChecker _featureChecker;

    public AppEmailSenderService(
        ITenantRepository tenantRepo,
        IDistributedCache<TenantConfigurationCacheItem> tenantCache,
        IRepository<Email, Guid> repo,
        IBackgroundJobManager jobManager,
        ITemplateRenderer templateRenderer,
        IFeatureChecker featureChecker
    ) : base(tenantRepo, tenantCache)
    {
        _repo = repo;
        _jobManager = jobManager;
        _templateRenderer = templateRenderer;
        _featureChecker = featureChecker;
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

        await _repo.InsertAsync(email, autoSave: true);
        await _jobManager.EnqueueAsync(new SendEmailJobArgs
        {
            EmailId = email.Id,
            TenantId = CurrentTenant.Id
        });
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
        // ✅ Nếu muốn bật/tắt theo feature thì bỏ comment:
        await _featureChecker.CheckEnabledAsync(Features.AppEmails.AppEmailFeatures.Management);

        // ✅ Ép Scriban giữ PascalCase + biến tên "model"
        var scriptModel = new ScriptObject();
        scriptModel.Import(model!, renamer: m => m.Name);

        var body = await _templateRenderer.RenderAsync(
            templateName,
            model: null, // không dùng model mặc định của ABP
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
        await _jobManager.EnqueueAsync(new AppServices.AppEmails.Jobs.SendEmailJobArgs { EmailId = email.Id });
        return email.Id;
    }
}
