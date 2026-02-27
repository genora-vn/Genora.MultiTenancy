using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public interface IZaloRuntimeConfigProvider
{
    Task<ZaloRuntimeConfig> GetAsync();
}