using System.Collections.Generic;
using System.Threading.Tasks;
using Web.Infrastructure.Data;
using Web.Infrastructure.Data.Factory;
using Web.Infrastructure.Data.Initialize;
using Web.Infrastructure.Data.Initialize.Seed;

namespace Web.IntegrationTests.Infrastructure.Data.Initialize
{
    public class TestApplicationDatabaseInitializer : DefaultApplicationDatabaseInitializer
    {
        public TestApplicationDatabaseInitializer(IDataContextFactory<DataContext> dataContextFactory,
            IEnumerable<IDatabaseSeeder<DataContext>> databaseSeeders) :
            base(dataContextFactory, databaseSeeders)
        {
        }

        public override async Task InitializeDbAsync()
        {
            //TODO: изучить, как запускаются тесты и как правильно инициализировать базу
            await using (var context = _dataContextFactory.Create())
            {
                await context.Database.EnsureDeletedAsync();
            }

            await base.InitializeDbAsync();
        }
    }
}