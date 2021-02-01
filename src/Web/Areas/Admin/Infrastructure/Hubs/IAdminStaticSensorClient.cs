using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Web.Models.Hub;

namespace Web.Areas.Admin.Infrastructure.Hubs
{
    public interface IAdminStaticSensorClient
    {
        Task DispatchReadingAsync(StaticSensorReadingDispatchModel reading);
    }
}
