using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Web.Domain.Entities;
using Web.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Web.IntegrationTests.Controllers.API
{
    // [Collection("Integration: Sequential")]
    public class SensorControllerTest : BaseScenario
    {
        public SensorControllerTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Trait("Category","Smoke")]
        [Fact]
        public async Task Get_get_all_sensors_and_response_ok_status_code()
        {
            //Arrange

            using var server = GetDefaultTestServerBuilder()
                .Build();
            var client = server.CreateClient();

            //Act
            var response = await client.GetAsync("api/sensors");

            //Assert
            response.EnsureSuccessStatusCode();
        }

        [Trait("Category","Smoke")]
        [Fact]
        public async Task Get_get_all_sensors_and_response_ok_status_code_with_data()
        {
            //Arrange
            var dataSet = new Sensor[]
            {
                Defaults.DeletedStaticSensor,
                Defaults.StaticSensor,
                Defaults.ActiveStaticSensor,
                Defaults.ActiveAndVisibleStaticSensor,
                Defaults.DeletedPortableSensor,
                Defaults.PortableSensor,
                Defaults.ActivePortableSensor,
            };

            var activeStaticSensorsCount =
                dataSet.OfType<StaticSensor>()
                    .Count(z => z.IsAvailable());

            using var server = GetDefaultTestServerBuilder()
                .UseSensors(dataSet)
                .Build();
            var client = server.CreateClient();

            //Act
            var response =
                await client.GetAsync(
                    $"api/sensors");
            var responseStr = await response.Content.ReadAsStringAsync();
            var jsonResponse = JArray.Parse(responseStr);


            //Assert
            Assert.True(jsonResponse.Count == activeStaticSensorsCount);
        }
    }
}