using InovaGAB.API.Models;
using MongoDB.Driver;

namespace InovaGAB.API.Data;

public static class MongoDbIndexes
{
    public static async Task CreateAsync(
        MongoDbContext context)
    {
        await context.Users.Indexes.CreateOneAsync(
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys
                    .Ascending(user => user.Email),
                new CreateIndexOptions
                {
                    Name = "ux_users_email",
                    Unique = true
                }));

        await context.Ideas.Indexes.CreateManyAsync(
            new[]
            {
                new CreateIndexModel<Idea>(
                    Builders<Idea>.IndexKeys
                        .Ascending(idea => idea.UserId),
                    new CreateIndexOptions
                    {
                        Name = "ix_ideas_userId"
                    }),
                new CreateIndexModel<Idea>(
                    Builders<Idea>.IndexKeys
                        .Ascending(idea => idea.ChallengeId),
                    new CreateIndexOptions
                    {
                        Name = "ix_ideas_challengeId"
                    }),
                new CreateIndexModel<Idea>(
                    Builders<Idea>.IndexKeys
                        .Ascending(idea => idea.Status),
                    new CreateIndexOptions
                    {
                        Name = "ix_ideas_status"
                    })
            });

        await context.Projects.Indexes.CreateManyAsync(
            new[]
            {
                new CreateIndexModel<Project>(
                    Builders<Project>.IndexKeys
                        .Ascending(project => project.ManagerId),
                    new CreateIndexOptions
                    {
                        Name = "ix_projects_managerId"
                    }),
                new CreateIndexModel<Project>(
                    Builders<Project>.IndexKeys
                        .Ascending(project => project.IdeaId),
                    new CreateIndexOptions
                    {
                        Name = "ix_projects_ideaId"
                    }),
                new CreateIndexModel<Project>(
                    Builders<Project>.IndexKeys
                        .Ascending(project => project.Status),
                    new CreateIndexOptions
                    {
                        Name = "ix_projects_status"
                    })
            });

        await context.StrategicGuidelines.Indexes
            .CreateOneAsync(
                new CreateIndexModel<StrategicGuideline>(
                    Builders<StrategicGuideline>.IndexKeys
                        .Ascending(guideline =>
                            guideline.IsActive)
                        .Descending(guideline =>
                            guideline.CreatedAt),
                    new CreateIndexOptions
                    {
                        Name =
                            "ix_guidelines_active_createdAt"
                    }));

        await context.Challenges.Indexes.CreateOneAsync(
            new CreateIndexModel<Challenge>(
                Builders<Challenge>.IndexKeys
                    .Ascending(challenge => challenge.IsActive)
                    .Ascending(challenge => challenge.EndDate),
                new CreateIndexOptions
                {
                    Name = "ix_challenges_active_endDate"
                }));

        await context.AuditLogs.Indexes.CreateOneAsync(
            new CreateIndexModel<AuditLog>(
                Builders<AuditLog>.IndexKeys
                    .Descending(log => log.CreatedAt),
                new CreateIndexOptions
                {
                    Name = "ix_auditLogs_createdAt"
                }));
    }
}
