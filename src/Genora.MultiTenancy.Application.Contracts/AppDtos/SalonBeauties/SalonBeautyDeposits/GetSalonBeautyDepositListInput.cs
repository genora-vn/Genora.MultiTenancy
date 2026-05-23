using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyDeposits;

public class GetSalonBeautyDepositListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public Guid? CustomerId { get; set; }
    public byte? Status { get; set; }
    public byte? PaymentMethod { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
