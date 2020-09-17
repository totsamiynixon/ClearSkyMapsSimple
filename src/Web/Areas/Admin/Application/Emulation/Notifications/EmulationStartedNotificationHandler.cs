using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Web.Domain.Entities;
using Web.Helpers.Interfaces;
using Web.Infrastructure.Data;
using Web.Infrastructure.Data.Factory;
using Web.Infrastructure.Data.Initialize;

namespace Web.Areas.Admin.Application.Emulation.Notifications
{
    public class EmulationStartedNotificationHandler : INotificationHandler<EmulationStartedNotification>
    {
        private readonly IEnumerable<IApplicationDatabaseInitializer> _applicationDatabaseInitializers;
        private readonly IDataContextFactory<DataContext> _dataContextFactory;
        private readonly ISensorCacheHelper _sensorCacheHelper;

        public EmulationStartedNotificationHandler(IEnumerable<IApplicationDatabaseInitializer> applicationDatabaseInitializers, IDataContextFactory<DataContext> dataContextFactory, ISensorCacheHelper sensorCacheHelper)
        {
            _applicationDatabaseInitializers = applicationDatabaseInitializers ?? throw new ArgumentNullException(nameof(applicationDatabaseInitializers));
            _dataContextFactory = dataContextFactory ?? throw new ArgumentNullException(nameof(dataContextFactory));
            _sensorCacheHelper = sensorCacheHelper ?? throw new ArgumentNullException(nameof(sensorCacheHelper));
        }

        //TODO: check this code
        public async Task Handle(EmulationStartedNotification notification, CancellationToken cancellationToken)
        {
            _sensorCacheHelper.ClearCache();
            foreach (var initializer in _applicationDatabaseInitializers)
            {
                await initializer.InitializeDbAsync();
            }

            await using var context = _dataContextFactory.Create();
            context.Sensors.RemoveRange(context.Sensors);
            await context.SaveChangesAsync(cancellationToken);

            foreach (var device in notification.Emulator.Devices)
            {
                if (device.SensorType == typeof(StaticSensor))
                {
                    context.Add(new StaticSensor
                    {
                        ApiKey = device.ApiKey,
                        Latitude = device.Latitude,
                        Longitude = device.Longitude
                    });
                }

                if (device.SensorType == typeof(PortableSensor))
                {
                    context.Add(new PortableSensor
                    {
                        ApiKey = device.ApiKey
                    });
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}