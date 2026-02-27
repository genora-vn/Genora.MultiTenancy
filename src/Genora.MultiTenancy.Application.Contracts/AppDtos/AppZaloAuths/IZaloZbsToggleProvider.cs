using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public interface IZaloZbsToggleProvider
{
    Task<bool> IsEnabledAsync();
}