using System;

namespace Genora.MultiTenancy.AppDtos.AppCustomers;
public class AppCustomerExcelRowDto
{
    public string? FullName { get; set; }
    public string? CustomerCode { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}