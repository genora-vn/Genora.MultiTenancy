using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public interface IZaloZbsTemplateResolver
{
    Task<string?> ResolveAsync(string key);
}