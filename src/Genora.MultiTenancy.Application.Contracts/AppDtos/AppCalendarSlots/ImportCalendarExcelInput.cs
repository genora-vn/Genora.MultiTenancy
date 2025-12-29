using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots
{
    public class ImportCalendarExcelInput
    {
        public IRemoteStreamContent? File { get; set; }
    }
}
