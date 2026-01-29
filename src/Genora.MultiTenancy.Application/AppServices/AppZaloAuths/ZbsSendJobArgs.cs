using System;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

[BackgroundJobName("Genora.SendZbsTemplate")]
public class ZbsSendJobArgs : IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Key map vào Zalo:Zbs:Templates:* (vd: RegisterSuccess, BookingCreated...)
    /// </summary>
    public string TemplateKey { get; set; } = "";

    /// <summary>
    /// Phone nhận ZNS/ZBS
    /// </summary>
    public string Phone { get; set; } = "";

    /// <summary>
    /// tracking_id để đối soát
    /// </summary>
    public string TrackingId { get; set; } = "";

    /// <summary>
    /// template_data (object serialize)
    /// </summary>
    public object TemplateData { get; set; } = new { };
}