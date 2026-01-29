using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.HangfireJobs;
public interface IBookingReminderZbsCronJob
{
    Task ExecuteAsync();
}