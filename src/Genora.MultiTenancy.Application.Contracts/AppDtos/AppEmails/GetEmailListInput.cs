using Genora.MultiTenancy.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppEmails;
public class GetEmailListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public EmailStatus? Status { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public string? BookingCode { get; set; }
}