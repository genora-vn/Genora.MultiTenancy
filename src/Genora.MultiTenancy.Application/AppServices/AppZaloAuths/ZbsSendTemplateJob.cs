using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    private readonly IOptionsSnapshot<ZaloZbsOptions> _opts;
    private readonly ILogger<ZbsSendTemplateJob> _logger;

    public ZbsSendTemplateJob(
        IZaloZbsClient zbsClient,
        IZaloZbsTemplateResolver resolver,
        ICurrentTenant currentTenant,
        IOptionsSnapshot<ZaloZbsOptions> opts,
        ILogger<ZbsSendTemplateJob> logger)
    {
        _zbsClient = zbsClient;
        _resolver = resolver;
        _currentTenant = currentTenant;
        _opts = opts;
        _logger = logger;
    }

    public override async Task ExecuteAsync(ZbsSendJobArgs args)
    {
        // Check bật / tắt gửi ZNS hay khôgn
        if (!_opts.Value.Enabled)
        {
            _logger.LogDebug("ZBS disabled. Skip TemplateKey={TemplateKey}, TrackingId={TrackingId}", args.TemplateKey, args.TrackingId);
            return;
        }

        if (string.IsNullOrWhiteSpace(args.TemplateKey)) return;
        if (string.IsNullOrWhiteSpace(args.Phone)) return;

        using (_currentTenant.Change(args.TenantId))
        {
            var templateId = _resolver.Resolve(args.TemplateKey);
            if (string.IsNullOrWhiteSpace(templateId))
            {
                _logger.LogWarning("ZBS template id empty. TemplateKey={TemplateKey}", args.TemplateKey);
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
                    template_data = args.TemplateData,
                    tracking_id = string.IsNullOrWhiteSpace(args.TrackingId)
                        ? Guid.NewGuid().ToString("N")
                        : args.TrackingId
                }
            };

            // Nếu Zalo lỗi -> throw để Hangfire retry
            var res = await _zbsClient.CallAsync(req, default);

            // Log sau khi gửi ZBS
            _logger.LogInformation("ZBS sent. TemplateKey={TemplateKey}, Phone={Phone}, TrackingId={TrackingId}",
                args.TemplateKey, args.Phone, args.TrackingId);
        }
    }
}