using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.Web.HangfireJobs;

public class BookingReminderZbsCronJob : IBookingReminderZbsCronJob
{
    private readonly IRepository<Booking, Guid> _bookingRepo;
    private readonly IZaloZbsClient _zbsClient;
    private readonly IZaloZbsTemplateResolver _resolver;
    private readonly IZaloZbsToggleProvider _zbsToggle;
    private readonly IClock _clock;
    private readonly IUnitOfWorkManager _uowManager;
    private readonly IAsyncQueryableExecuter _asyncExecuter;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<BookingReminderZbsCronJob> _logger;

    public BookingReminderZbsCronJob(
        IRepository<Booking, Guid> bookingRepo,
        IZaloZbsClient zbsClient,
        IZaloZbsTemplateResolver resolver,
        IZaloZbsToggleProvider zbsToggle,
        IClock clock,
        IUnitOfWorkManager uowManager,
        IAsyncQueryableExecuter asyncExecuter,
        ICurrentTenant currentTenant,
        ILogger<BookingReminderZbsCronJob> logger)
    {
        _bookingRepo = bookingRepo;
        _zbsClient = zbsClient;
        _resolver = resolver;
        _zbsToggle = zbsToggle;
        _clock = clock;
        _uowManager = uowManager;
        _asyncExecuter = asyncExecuter;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    private sealed class Candidate
    {
        public Guid BookingId { get; set; }
        public Guid? TenantId { get; set; }
        public string BookingCode { get; set; } = "";
        public string Phone { get; set; } = "";
        public DateTime PlayDate { get; set; }
        public TimeSpan TimeFrom { get; set; }
        public TimeSpan TimeTo { get; set; }
    }

    public async Task ExecuteAsync()
    {
        _logger.LogWarning(">>> BookingReminderZbsCronJob fired at {Now}", _clock.Now);

        var now = _clock.Now;
        var from = now.AddMinutes(55);
        var to = now.AddMinutes(65);

        List<Candidate> candidates;

        // ✅ Query trong UoW và chỉ project ra primitive => tránh ObjectDisposed
        using (var uow = _uowManager.Begin(requiresNew: true, isTransactional: false))
        {
            var q = await _bookingRepo.WithDetailsAsync(b => b.Customer, b => b.CalendarSlot);

            var projected = q.Where(b =>
                    b.PlayDate.Date == now.Date &&
                    b.CalendarSlotId != null &&
                    b.CalendarSlot != null &&
                    b.Status == BookingStatus.Processing &&
                    b.Customer != null &&
                    !string.IsNullOrWhiteSpace(b.Customer.PhoneNumber)
                )
                .Select(b => new Candidate
                {
                    BookingId = b.Id,
                    TenantId = b.TenantId,
                    BookingCode = b.BookingCode,
                    Phone = b.Customer!.PhoneNumber!,
                    PlayDate = b.PlayDate,
                    TimeFrom = b.CalendarSlot!.TimeFrom,
                    TimeTo = b.CalendarSlot!.TimeTo
                });

            candidates = await _asyncExecuter.ToListAsync(projected, CancellationToken.None);
            await uow.CompleteAsync(CancellationToken.None);
        }

        if (candidates.Count == 0) return;

        foreach (var tenantGroup in candidates.GroupBy(x => x.TenantId))
        {
            using (_currentTenant.Change(tenantGroup.Key))
            {
                // ✅ per-tenant enable switch
                var enabled = await _zbsToggle.IsEnabledAsync();
                if (!enabled)
                {
                    _logger.LogDebug("ZBS disabled. Skip tenant {TenantId}", tenantGroup.Key);
                    continue;
                }

                // ✅ per-tenant template id
                var templateId = await _resolver.ResolveAsync("BookingReminder");
                if (string.IsNullOrWhiteSpace(templateId))
                {
                    _logger.LogDebug("BookingReminder templateId empty. Skip tenant {TenantId}", tenantGroup.Key);
                    continue;
                }

                foreach (var c in tenantGroup)
                {
                    var slotTime = c.PlayDate.Date.Add(c.TimeFrom);
                    if (slotTime < from || slotTime > to) continue;

                    try
                    {
                        await _zbsClient.CallAsync(new ZaloZbsCallRequest
                        {
                            Api = "zns",
                            Method = "POST",
                            Path = "/message/template",
                            Body = new
                            {
                                phone = c.Phone,
                                template_id = templateId,
                                template_data = new
                                {
                                    booking_code = c.BookingCode,
                                    play_time = slotTime.ToString("HH:mm"),
                                    play_date = c.PlayDate.ToString("dd/MM/yyyy"),
                                    tee_time = $"{c.TimeFrom:hh\\:mm} - {c.TimeTo:hh\\:mm}"
                                },
                                tracking_id = $"REMIND-{c.BookingId}"
                            }
                        }, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "BookingReminder send failed. BookingId={BookingId}, TenantId={TenantId}",
                            c.BookingId, tenantGroup.Key);
                    }
                }
            }
        }
    }
}