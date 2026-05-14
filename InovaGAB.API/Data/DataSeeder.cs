using InovaGAB.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InovaGAB.API.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            var operator1 = new User
            {
                Name = "João Silva",
                Email = "joao.operador@aguiabranca.com.br",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
                Role = UserRole.Operator,
                Division = "Passageiros",
                Points = 50,
                CreatedAt = DateTime.UtcNow
            };

            var operator2 = new User
            {
                Name = "Maria Costa",
                Email = "maria.operador@aguiabranca.com.br",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
                Role = UserRole.Operator,
                Division = "Logística",
                Points = 120,
                CreatedAt = DateTime.UtcNow
            };

            var manager = new User
            {
                Name = "Ana Gestora",
                Email = "ana.gestora@aguiabranca.com.br",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
                Role = UserRole.Manager,
                Division = "Logística",
                Points = 0,
                CreatedAt = DateTime.UtcNow
            };

            var leader = new User
            {
                Name = "Carlos Lider",
                Email = "carlos.lider@aguiabranca.com.br",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
                Role = UserRole.Leader,
                Division = "Corporativo",
                Points = 0,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(operator1, operator2, manager, leader);
            await context.SaveChangesAsync();

            context.StrategicGuidelines.AddRange
              (
                new StrategicGuideline
                {
                    Title = "Redução de Custos Operacionais",
                    Description = "Foco em iniciativas que gerem economia direta na operação. Meta: -15% até dez/26.",
                    Priority = GuidelinePriority.High,
                    Category = "Financeiro",
                    IsActive = true,
                    CreatedById = leader.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new StrategicGuideline
                {
                    Title = "Digitalização da Operação",
                    Description = "Priorizar soluções mobile e automações de processo nos setores de passageiros e comércio.",
                    Priority = GuidelinePriority.High,
                    Category = "Inovação",
                    IsActive = true,
                    CreatedById = leader.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new StrategicGuideline
                {
                    Title = "Engajamento do Colaborador",
                    Description = "Ampliar a participação ativa no InovaGAB. Meta: 30% dos colaboradores com ao menos 1 ideia.",
                    Priority = GuidelinePriority.Medium,
                    Category = "Pessoas",
                    IsActive = true,
                    CreatedById = leader.Id,
                    CreatedAt = DateTime.UtcNow
                }
              );
            await context.SaveChangesAsync();

            var challenge = new Challenge
            {
                Title = "Redução de Combustível",
                Description = "Compartilhe ideias para reduzir o consumo de combustível da frota",
                Prize = 5000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                CreatedById = leader.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Challenges.Add( challenge );
            await context.SaveChangesAsync();

            var idea1 = new Idea
            {
                Title = "Otimizar rota ES-010",
                Description = "A rota ES-010 tem paradas desnecessárias que aumentam o tempo de viagem em ~18 min.",
                Division = "Passageiros",
                Status = IdeaStatus.Approved,
                ImpactScore = 9,
                FeasibilityScore = 8,
                AlignmentScore = 10,
                UserId = operator1.Id,
                ChallengeId = challenge.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-12)
            };

            var idea2 = new Idea
            {
                Title = "App checklist de veículos",
                Description = "Substituir o checklist em papel por um app mobile para reduzir erros e tempo.",
                Division = "Logística",
                Status = IdeaStatus.UnderReview,
                ImpactScore = 7,
                FeasibilityScore = 9,
                AlignmentScore = 8,
                UserId = operator2.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };

            var idea3 = new Idea
            {
                Title = "Uniforme sustentável",
                Description = "Adotar uniformes produzidos com material reciclado para reduzir impacto ambiental",
                Division = "Comércio",
                Status = IdeaStatus.Submitted,
                UserId = operator1.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            context.Ideas.AddRange(idea1, idea2, idea3);
            await context.SaveChangesAsync();

            context.Projects.Add(new Project
            {
                Title = "Otimização Rota ES-010",
                Description = "Projeto originado da ideia aprovada de otimização de rota.",
                Division = "Passageiros",
                Status = ProjectStatus.InProgress,
                Stage = ProjectStage.Implementation,
                Investment = 18000,
                FinancialReturn = 420000,
                ProductivityGain = 22,
                StartDate = DateTime.UtcNow.AddDays(-30),
                Deadline = DateTime.UtcNow.AddDays(60),
                ProgressPercent = 65,
                ManagerId = manager.Id,
                IdeaId = idea1.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            });
            await context.SaveChangesAsync();
        }
    }
}
