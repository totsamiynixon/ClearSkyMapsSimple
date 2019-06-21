﻿using System;

namespace Web.Areas.Admin.Emulation.Models
{
    public class SensorDataModel
    {
        public string ApiKey { get; set; }

        public float CO2 { get; set; }

        public float LPG { get; set; }

        public float CO { get; set; }

        public float CH4 { get; set; }

        public float Dust { get; set; }

        public float Temp { get; set; }

        public float Hum { get; set; }

        public float Preassure { get; set; }

        public DateTime Created { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
