using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Application.Readings.Commands;
using Web.Application.Readings.Commands.DTO;
using Web.Application.Readings.Exceptions;

namespace Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class IntegrationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public IntegrationController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Creates new reading 
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/integration/getaspost?data=qwerty,28,0.5,20,20,20,20,20,20,20,53.2222,52.3333
        ///
        /// Explanation:
        /// 
        ///     HTTP GET is used, because it's more easy for actual device to integrate
        /// </remarks>
        /// <param name="data">Data as string in format: ApiKey,Temp,Hum,Preassure,CO2,LPG,CO,CH4,Dust,Longitude,Latitude;</param>
        /// <response code="202">If data has been successfully accepted</response>
        /// <response code="400">If data structure or readings are invalid</response>   
        [HttpGet("getaspost")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAsPostDataAsync(string data)
        {
            var model = GetModelFromString(data);
            if (model.apiKey == null || model.reading == null)
            {
                return BadRequest("Invalid Data");
            }

            try
            {
                await _mediator.Send(new CreateReadingCommand(model.reading, model.apiKey));
            }
            catch (SensorNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            return Accepted();
        }


        private (string apiKey,SensorReadingDTO reading) GetModelFromString(string data)
        {
            try
            {
                var trimmed = data.Trim(';');
                var groupes = trimmed.Split(",").ToArray();
                return (apiKey: groupes[0], reading: new SensorReadingDTO
                {
                    Temp = float.Parse(groupes[1], CultureInfo.InvariantCulture),
                    Hum = float.Parse(groupes[2], CultureInfo.InvariantCulture),
                    Preassure = float.Parse(groupes[3], CultureInfo.InvariantCulture),
                    CO2 = float.Parse(groupes[4], CultureInfo.InvariantCulture),
                    LPG = float.Parse(groupes[5], CultureInfo.InvariantCulture),
                    CO = float.Parse(groupes[6], CultureInfo.InvariantCulture),
                    CH4 = float.Parse(groupes[7], CultureInfo.InvariantCulture),
                    Dust = float.Parse(groupes[8], CultureInfo.InvariantCulture),
                    Longitude = float.Parse(groupes[9], CultureInfo.InvariantCulture),
                    Latitude = float.Parse(groupes[10], CultureInfo.InvariantCulture)
                });
            }
            catch (Exception ex)
            {
                return (null, null);
            }
        }
    }
}