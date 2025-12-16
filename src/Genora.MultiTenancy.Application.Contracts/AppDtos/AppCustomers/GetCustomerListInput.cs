using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppCustomers;

public class GetCustomerListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// Text chung: tìm trong tên / SĐT / mã khách
    /// </summary>
    public string FilterText { get; set; }

    public string PhoneNumber { get; set; }

    public string FullName { get; set; }

    public Guid? CustomerTypeId { get; set; }

    public bool? IsActive { get; set; }

    /// <summary>
    /// Filter khách sinh nhật từ ngày... đến ngày...
    /// (để làm campaign sinh nhật)
    /// </summary>
    public DateTime? BirthDateFrom { get; set; }
    public DateTime? BirthDateTo { get; set; }
}