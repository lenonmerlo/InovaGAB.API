using InovaGAB.API.Models;
using MongoDB.Driver;

namespace InovaGAB.API.Data;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoCollection<User> Users =>
        _database.GetCollection<User>("users");

    public IMongoCollection<Idea> Ideas =>
        _database.GetCollection<Idea>("ideas");

    public IMongoCollection<Project> Projects =>
        _database.GetCollection<Project>("projects");

    public IMongoCollection<Challenge> Challenges =>
        _database.GetCollection<Challenge>("challenges");

    public IMongoCollection<StrategicGuideline> StrategicGuidelines =>
        _database.GetCollection<StrategicGuideline>("strategicGuidelines");

    public IMongoCollection<AuditLog> AuditLogs =>
        _database.GetCollection<AuditLog>("auditLogs");
}