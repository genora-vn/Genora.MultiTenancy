using System;

namespace Genora.MultiTenancy.AppServices.AppEmails.Jobs;
[Serializable]
public class SendEmailJobArgs
{
    public Guid EmailId { get; set; }
}