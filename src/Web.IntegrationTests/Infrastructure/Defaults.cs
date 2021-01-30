using System.Collections.Generic;
using Web.Domain.Entities;
using Web.Helpers;

namespace Web.IntegrationTests.Infrastructure
{
    public class Defaults
    {
        public const double Latitude = 53.333333;
        public const double Longitude = 51.111111;

        public static StaticSensor StaticSensor => new StaticSensor
        {
            ApiKey = ApiKeyHelper.Generate(),
            Latitude = Latitude,
            Longitude = Longitude,
            IsActive = false,
            IsVisible = false,
            Readings = new List<StaticSensorReading>()
        };
        
        public static StaticSensor ActiveStaticSensor => new StaticSensor
        {
            ApiKey = ApiKeyHelper.Generate(),
            Latitude = Latitude,
            Longitude = Longitude,
            IsActive = true,
            IsVisible = false,
            Readings = new List<StaticSensorReading>()
        };
        
        public static StaticSensor ActiveAndVisibleStaticSensor => new StaticSensor
        {
            ApiKey = ApiKeyHelper.Generate(),
            Latitude = Latitude,
            Longitude = Longitude,
            IsActive = true,
            IsVisible = true,
            Readings = new List<StaticSensorReading>()
        };
        
        public static StaticSensor DeletedStaticSensor => new StaticSensor
        {
            ApiKey = ApiKeyHelper.Generate(),
            Latitude = Latitude,
            Longitude = Longitude,
            IsActive = false,
            IsVisible = false,
            IsDeleted = true,
            Readings = new List<StaticSensorReading>()
        };

        public static PortableSensor PortableSensor => new PortableSensor
        {
            ApiKey = ApiKeyHelper.Generate(),
            IsActive = false
        };
        
        public static PortableSensor ActivePortableSensor => new PortableSensor
        {
            ApiKey = ApiKeyHelper.Generate(),
            IsActive = true
        };
        
        public static PortableSensor DeletedPortableSensor => new PortableSensor
        {
            ApiKey = ApiKeyHelper.Generate(),
            IsActive = false,
            IsDeleted = true
        };
    }
}