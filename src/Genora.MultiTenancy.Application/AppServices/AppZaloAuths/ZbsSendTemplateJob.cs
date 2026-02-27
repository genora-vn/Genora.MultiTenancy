using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

public class ZbsSendTemplateJob : AsyncBackgroundJob<ZbsSendJobArgs>, ITransientDependency
{
    private readonly IZaloZbsClient _zbsClient;
    private readonly IZaloZbsTemplateResolver _resolver;
    private readonly ICurrentTenant _currentTenant;
    private readonly IZaloZbsToggleProvider _toggleProvider;
    private readonly ILogger<ZbsSendTemplateJob> _logger;

    public ZbsSendTemplateJob(
        IZaloZbsClient zbsClient,
        IZaloZbsTemplateResolver resolver,
        ICurrentTenant currentTenant,
        IZaloZbsToggleProvider toggleProvider,
        ILogger<ZbsSendTemplateJob> logger)
    {
        _zbsClient = zbsClient;
        _resolver = resolver;
        _currentTenant = currentTenant;
        _toggleProvider = toggleProvider;
        _logger = logger;
    }

    public override async Task ExecuteAsync(ZbsSendJobArgs args)
    {
        if (string.IsNullOrWhiteSpace(args.TemplateKey)) return;
        if (string.IsNullOrWhiteSpace(args.Phone)) return;

        using (_currentTenant.Change(args.TenantId))
        {
            // ✅ Check bật/tắt theo tenant (settings)
            var enabled = await _toggleProvider.IsEnabledAsync();
            if (!enabled)
            {
                _logger.LogDebug("ZBS disabled. Skip TemplateKey={TemplateKey}, TrackingId={TrackingId}, TenantId={TenantId}",
                    args.TemplateKey, args.TrackingId, args.TenantId);
                return;
            }

            var key = args.TemplateKey?.Trim();
            var templateId = await _resolver.ResolveAsync(key!);
            if (string.IsNullOrWhiteSpace(templateId))
            {
                _logger.LogWarning("ZBS template id empty. TemplateKey={TemplateKey}, TenantId={TenantId}",
                    args.TemplateKey, args.TenantId);
                return;
            }

            var req = new ZaloZbsCallRequest
            {
                Api = "zns",
                Method = "POST",
                Path = "/message/template",
                Body = new
                {
                    phone = args.Phone,
                    template_id = templateId,
                    template_data = args.TemplateData ?? new { },
                    tracking_id = string.IsNullOrWhiteSpace(args.TrackingId)
                        ? Guid.NewGuid().ToString("N")
                        : args.TrackingId
                }
            };

            // Nếu Zalo lỗi -> throw để Hangfire retry
            var res = await _zbsClient.CallAsync(req, default);

            _logger.LogInformation("ZBS sent. TemplateKey={TemplateKey}, Phone={Phone}, TrackingId={TrackingId}, TenantId={TenantId}",
                args.TemplateKey, args.Phone, args.TrackingId, args.TenantId);
        }
    }
}