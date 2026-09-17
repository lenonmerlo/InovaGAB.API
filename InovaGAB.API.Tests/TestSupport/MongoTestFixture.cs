using InovaGAB.API.Data;
using MongoDB.Driver;

namespace InovaGAB.API.Tests.TestSupport;

// cada instancia usa um banco isolado (nome unico) no MongoDB local, criado
// e derrubado por teste, sem depender do banco de desenvolvimento/seed
public class MongoTestFixture : IAsyncLifetime
{
    private readonly string _connectionString =
        Environment.GetEnvironmentVariable("MONGO_TEST_CONNECTION_STRING")
        ?? "mongodb://localhost:27017";

    private readonly string _databaseName = $"InovaGab_Test_{Guid.NewGuid():N}";

    private IMongoClient _client = null!;

    public MongoDbContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _client = new MongoClient(_connectionString);
        Context = new MongoDbContext(_client.GetDatabase(_databaseName));

        await MongoDbIndexes.CreateAsync(Context);
    }

    public async Task DisposeAsync()
    {
        await _client.DropDatabaseAsync(_databaseName);
    }
}
