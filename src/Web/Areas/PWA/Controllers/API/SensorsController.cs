﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Web.Areas.PWA.Models.API.Sensors;
using Web.Areas.PWA.Models.Sensors;
using Web.Domain.Entities;
using Web.Helpers.Interfaces;

namespace Web.Areas.PWA.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorsController : ControllerBase
    {
        private readonly ISensorCacheHelper _sensorCacheHelper;

        public SensorsController(ISensorCacheHelper sensorCacheHelper)
        {
            _sensorCacheHelper = sensorCacheHelper;
        }

        private static IMapper _mapper = new Mapper(new MapperConfiguration(x =>
        {
            x.CreateMap<StaticSensor, StaticSensorModel>();
            x.CreateMap<Reading, SensorDataModel>();
        }));

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var sensors = await _sensorCacheHelper.GetStaticSensorsAsync();
            var model = sensors.Select(f => new StaticSensorModel
            {
                Id = f.Sensor.Id,
                Latitude = f.Sensor.Latitude,
                Longitude = f.Sensor.Longitude,
                PollutionLevel = f.PollutionLevel,
                Readings = _mapper.Map<List<StaticSensorReading>, List<StaticSensorReadingModel>>(f.Sensor.Readings)
            });
            return Ok(model.ToArray());
        }
    }
}