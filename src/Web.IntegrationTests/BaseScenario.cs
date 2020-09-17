using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Web.Areas.Admin.Infrastructure.Auth;
using Web.Areas.Admin.Infrastructure.Data;
using Web.Areas.PWA;
using Web.Domain.Entities;
using Web.Domain.Entities.Identity;
using Web.Infrastructure.Data;
using Web.Infrastructure.Data.Initialize.Seed;
using Web.IntegrationTests.Areas.Admin;
using Web.IntegrationTests.Areas.Admin.Infrastructure;
using Web.IntegrationTests.Areas.Admin.Infrastructure.Data.Initialize.Seed;
using Web.IntegrationTests.Infrastructure.Data.Initialize.Seed;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Web.IntegrationTests
{
    public class BaseScenario
    {
        public ITest Test { get; }
        public ITestOutputHelper TestOutputHelper { get; }

        public BaseScenario(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
            Test = (ITest) (testOutputHelper as TestOutputHelper)?.GetType()
                .GetField("test", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(testOutputHelper);
        }

        public TestServerBuilder GetDefaultTestServerBuilder()
        {
            return new TestServerBuilder(TestOutputHelper, Test);
        }
    }


    public class TestServerBuilder
    {
        //test context
        protected readonly ITestOutputHelper _testOutputHelper;
        protected readonly ITest _test;

        //database
        private readonly List<IDatabaseSeeder<DataContext>> _databaseSeeders;
        private readonly List<IDatabaseSeeder<IdentityDataContext>> _identityDatabaseSeeders;

        //authentication
        private bool _useCustomAuth = false;
        private User User { get; set; }
        private List<string> Roles { get; set; }

        public TestServerBuilder(ITestOutputHelper testOutputHelper, ITest test)
        {
            _testOutputHelper = testOutputHelper;
            _test = test;
            
            _databaseSeeders = new List<IDatabaseSeeder<DataContext>>();
            _identityDatabaseSeeders = new List<IDatabaseSeeder<IdentityDataContext>>();
        }

        public TestServerBuilder UseSensors(params Sensor[] sensors)
        {
            _databaseSeeders.Add(new TestSensorsDatabaseSeeder(sensors));

            return this;
        }

        public TestServerBuilder UseUsers(params User[] users)
        {
            _identityDatabaseSeeders.Add(new TestIdentityDatabaseSeeder(users));

            return this;
        }

        public TestServerBuilder UseUsersWithRoles(params (User user, List<string> roles)[] usersWithRoles)
        {
            _identityDatabaseSeeders.Add(new TestIdentityDatabaseSeeder(usersWithRoles));

            return this;
        }

        public TestServerBuilder UseCustomAuth(User user, params string[] roles)
        {
            User = user;
            Roles = roles.ToList();
            _useCustomAuth = true;

            return this;
        }

        public TestServerBuilder UseDefaultAuth()
        {
            User = AdminAreaDefaults.DefaultUser;
            Roles = new List<string> {AuthSettings.Roles.Admin};
            _useCustomAuth = true;

            return this;
        }

        public async Task<TestServer> BuildAsync()
        {
            var path = Assembly.GetAssembly(typeof(TestStartup))
                ?.Location;

            var hostBuilder = Program.CreateWebHostBuilder(Array.Empty<string>())
                .UseStartup<TestStartup>()
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var config = configBuilder.Build();
                    var connectionString = config.GetSection("Settings").GetValue<string>("ConnectionString");
                    _testOutputHelper.WriteLine($"Connection String: {connectionString}");
                    var connectionStringInitialCatalogSegment =
                        connectionString.Split(";").First(z => z.Contains("Initial Catalog"));
                    var connectionStringTransformedInitialCatalogSegment =
                        $"{connectionStringInitialCatalogSegment}.{_test.TestCase.TestMethod.Method.Name}";
                    var transformedConnectionString = connectionString.Replace(
                        connectionStringInitialCatalogSegment,
                        connectionStringTransformedInitialCatalogSegment);
                    
                    configBuilder.AddInMemoryCollection(
                        new Dictionary<string, string>
                        {
                            ["Settings:ConnectionString"] = transformedConnectionString
                        });
                })
                .ConfigureTestServices(services =>
                {
                    if (_databaseSeeders.Any())
                    {
                        foreach (var seeder in _databaseSeeders)
                        {
                            services.AddTransient<IDatabaseSeeder<DataContext>>(x => seeder);
                        }
                    }

                    if (_identityDatabaseSeeders.Any())
                    {
                        foreach (var seeder in _identityDatabaseSeeders)
                        {
                            services.AddTransient<IDatabaseSeeder<IdentityDataContext>>(
                                x => seeder);
                        }
                    }
                })
                .ConfigureServices(services =>
                {
                    services.Configure<TestAdminAreaOptions>(x =>
                    {
                        x.Auth = new TestAdminAreaAuthOptions
                        {
                            UseCustomAuth = _useCustomAuth,
                            Roles = Roles,
                            User = User
                        };
                    });
                })
                .UseContentRoot(Path.GetDirectoryName(path));

            Program.ConfigureAdminArea<TestAdminArea>(hostBuilder);
            Program.ConfigurePWAArea<PWAArea>(hostBuilder);

            var testServer = new TestServer(hostBuilder);

            await Program.InitializeApplicationAsync(testServer.Host);

            return testServer;
        }
    }
}

/*public TestServerBuilder UseDatabaseSeeder(IDatabaseSeeder<DataContext> databaseSeeder)
{
    _databaseSeeders.Add(databaseSeeder);

    return this;
}

public TestServerBuilder UseIdentityDatabaseSeeder(
    IDatabaseSeeder<IdentityDataContext> identityDatabaseSeeder)
{
    _identityDatabaseSeeders.Add(identityDatabaseSeeder);

    return this;
}*/

//TODO: check how it works when docker fully implemented
/*.ConfigureAppConfiguration((context, configBuilder) =>
{
    var config = configBuilder.BuildAsync();
    configBuilder.AddInMemoryCollection(
        new Dictionary<string, string>
        {
            ["Settings:ConnectionString"] = config.GetSection("Settings").GetValue<string>("ConnectionString").Replace("{Id}", Guid.NewGuid().ToString())
        });
})*/
/*.ConfigureAppConfiguration((context, configBuilder) =>
{
    var testJsonConfigRootPath = Assembly.GetAssembly(typeof(TestStartup))
        ?.Location;
    
    //var testJsonFileProvider = new PhysicalFileProvider(testJsonConfigRootPath);
    //configBuilder.AddJsonFile(provider: testJsonFileProvider, path: "appsettings.json", optional: false, reloadOnChange: true);

    var testJsonStr = File.ReadAllText(Path.Combine(testJsonConfigRootPath, "appsettings.json"));
    configBuilder.AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(testJsonStr)));
})*/