using System.ComponentModel.DataAnnotations;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppBookings;
public class ImportBookingExcelInput
{
    public IRemoteStreamContent? File { get; set; }
}