﻿namespace Web.Areas.Admin.Models.Sensors
{
    public class StaticSensorDetailsViewModel : SensorDetailsViewModel
    {
        public bool IsVisible { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}