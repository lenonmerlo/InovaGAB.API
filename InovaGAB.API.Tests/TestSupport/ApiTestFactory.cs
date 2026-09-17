using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace InovaGAB.API.Tests.TestSupport;

// sobe a aplicação real (pipeline HTTP completo) contra um banco MongoDB
// isolado por classe de teste; a aplicação faz seu proprio seed no startup
public class ApiTestFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"InovaGab_Test_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDb:DatabaseName"] = _databaseName
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("MONGO_TEST_CONNECTION_STRING")
                ?? "mongodb://localhost:27017";

            new MongoClient(connectionString).DropDatabase(_databaseName);
        }

        base.Dispose(disposing);
    }
}
