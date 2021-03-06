﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Web.Areas.Admin.Models.Hub;

namespace Web.Areas.Admin.Infrastructure.Hubs
{
    public interface IAdminPortableSensorClient
    {
        Task DispatchReadingAsync(PortableSensorReadingsDispatchModel reading);
        Task DispatchCoordinatesAsync(PortableSensorCoordinatesDispatchModel coordinates);
    }
}
