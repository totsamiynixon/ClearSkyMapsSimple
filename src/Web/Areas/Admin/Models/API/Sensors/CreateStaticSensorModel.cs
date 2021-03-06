﻿using System.ComponentModel.DataAnnotations;

namespace Web.Areas.Admin.Models.API.Sensors
{
    public class CreateStaticSensorModel
    {
        [Required]
        public string ApiKey { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
    }
}