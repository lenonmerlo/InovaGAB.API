using InovaGAB.API.Data;
using InovaGAB.API.Models;

namespace InovaGAB.API.Tests.TestSupport;

public static class TestDataBuilder
{
    public static async Task<User> CreateUserAsync(
        MongoDbContext context,
        UserRole role = UserRole.Operator,
        string? email = null)
    {
        var user = new User
        {
            Name = $"Usuário {role}",
            Email = email ?? $"{Guid.NewGuid():N}@teste.com",
            PasswordHash = "hash",
            Role = role,
            Division = "Testes"
        };

        await context.Users.InsertOneAsync(user);
        return user;
    }

    public static async Task<Idea> CreateIdeaAsync(
        MongoDbContext context,
        string userId,
        IdeaStatus status = IdeaStatus.Submitted,
        string? guidelineId = null)
    {
        var idea = new Idea
        {
            Title = "Ideia de teste",
            Description = "Descrição de teste",
            Division = "Testes",
            UserId = userId,
            GuidelineId = guidelineId,
            Status = status
        };

        await context.Ideas.InsertOneAsync(idea);
        return idea;
    }

    public static async Task<Project> CreateProjectAsync(
        MongoDbContext context,
        string managerId,
        decimal investment = 10000,
        decimal financialReturn = 0,
        ProjectStatus status = ProjectStatus.Planning,
        ProjectStage stage = ProjectStage.Diagnosis)
    {
        var project = new Project
        {
            Title = "Projeto de teste",
            Description = "Descrição de teste",
            Division = "Testes",
            ManagerId = managerId,
            Investment = investment,
            FinancialReturn = financialReturn,
            Status = status,
            Stage = stage,
            StartDate = DateTime.UtcNow.AddDays(-10),
            Deadline = DateTime.UtcNow.AddDays(30)
        };

        await context.Projects.InsertOneAsync(project);
        return project;
    }

    public static async Task<StrategicGuideline> CreateGuidelineAsync(
        MongoDbContext context,
        string createdById,
        bool isActive = true)
    {
        var guideline = new StrategicGuideline
        {
            Title = "Diretriz de teste",
            Description = "Descrição de teste",
            Category = "Testes",
            Campaign = "Campanha de teste",
            CreatedById = createdById,
            IsActive = isActive,
            IsCurrent = true,
            Version = 1
        };

        guideline.RootId = guideline.Id;

        await context.StrategicGuidelines.InsertOneAsync(guideline);
        return guideline;
    }
}
