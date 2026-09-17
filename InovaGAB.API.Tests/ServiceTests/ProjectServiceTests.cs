using InovaGAB.API.DTOs.Request;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Implementations;
using InovaGAB.API.Tests.TestSupport;

namespace InovaGAB.API.Tests.ServiceTests;

public class ProjectServiceTests : IClassFixture<MongoTestFixture>
{
    private readonly MongoTestFixture _fixture;
    private readonly ProjectService _service;

    public ProjectServiceTests(MongoTestFixture fixture)
    {
        _fixture = fixture;
        _service = new ProjectService(fixture.Context);
    }

    [Fact]
    public async Task CreateAsync_InvestimentoNegativo_LancaArgumentException()
    {
        var manager = await TestDataBuilder.CreateUserAsync(_fixture.Context, UserRole.Manager);

        var request = new CreateProjectRequest
        {
            Title = "Projeto",
            Description = "Descrição",
            Division = "Divisão",
            Investment = -1,
            StartDate = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddDays(30)
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(request, manager.Id));
    }

    [Fact]
    public async Task CreateAsync_PrazoAntesDoInicio_LancaArgumentException()
    {
        var manager = await TestDataBuilder.CreateUserAsync(_fixture.Context, UserRole.Manager);

        var request = new CreateProjectRequest
        {
            Title = "Projeto",
            Description = "Descrição",
            Division = "Divisão",
            Investment = 1000,
            StartDate = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddDays(-1)
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(request, manager.Id));
    }

    [Fact]
    public async Task UpdateAsync_TransicaoDeStatusValida_Aplica()
    {
        var manager = await TestDataBuilder.CreateUserAsync(_fixture.Context, UserRole.Manager);
        var project = await TestDataBuilder.CreateProjectAsync(_fixture.Context, manager.Id);

        var result = await _service.UpdateAsync(
            project.Id,
            new UpdateProjectRequest { Status = ProjectStatus.InProgress });

        Assert.Equal("InProgress", result!.Status);
    }

    [Fact]
    public async Task UpdateAsync_TransicaoDeStatusInvalida_LancaInvalidOperation()
    {
        var manager = await TestDataBuilder.CreateUserAsync(_fixture.Context, UserRole.Manager);
        var project = await TestDataBuilder.CreateProjectAsync(
            _fixture.Context,
            manager.Id,
            status: ProjectStatus.Completed);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(
                project.Id,
                new UpdateProjectRequest { Status = ProjectStatus.InProgress }));
    }

    [Fact]
    public async Task UpdateAsync_EtapaForaDeOrdem_LancaInvalidOperation()
    {
        var manager = await TestDataBuilder.CreateUserAsync(_fixture.Context, UserRole.Manager);
        var project = await TestDataBuilder.CreateProjectAsync(_fixture.Context, manager.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(
                project.Id,
                new UpdateProjectRequest { Stage = ProjectStage.Closure }));
    }

    [Fact]
    public async Task UpdateAsync_ProgressoForaDoIntervalo_LancaArgumentException()
    {
        var manager = await TestDataBuilder.CreateUserAsync(_fixture.Context, UserRole.Manager);
        var project = await TestDataBuilder.CreateProjectAsync(_fixture.Context, manager.Id);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAsync(
                project.Id,
                new UpdateProjectRequest { ProgressPercent = 150 }));
    }

    [Fact]
    public async Task ArchiveAsync_ExcluiDaListagemPadraoMasMantemConsultaPorId()
    {
        var manager = await TestDataBuilder.CreateUserAsync(_fixture.Context, UserRole.Manager);
        var project = await TestDataBuilder.CreateProjectAsync(_fixture.Context, manager.Id);

        var archived = await _service.ArchiveAsync(project.Id);
        Assert.Equal(true, archived);

        var listed = await _service.GetAllAsync();
        Assert.DoesNotContain(listed, p => p.Id == project.Id);

        var byId = await _service.GetByIdAsync(project.Id);
        Assert.True(byId!.IsArchived);
    }

    [Fact]
    public async Task GetByIdAsync_VinculoComDiretriz_ExpoeResumoSemConsultaAdicional()
    {
        var manager = await TestDataBuilder.CreateUserAsync(_fixture.Context, UserRole.Manager);
        var guideline = await TestDataBuilder.CreateGuidelineAsync(_fixture.Context, manager.Id);

        var request = new CreateProjectRequest
        {
            Title = "Projeto",
            Description = "Descrição",
            Division = "Divisão",
            Investment = 1000,
            StartDate = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddDays(30),
            GuidelineId = guideline.Id
        };

        var created = await _service.CreateAsync(request, manager.Id);

        Assert.Equal(guideline.Id, created.GuidelineId);
        Assert.Equal(guideline.Title, created.Guideline!.Title);
    }

    [Fact]
    public async Task CreateAsync_DiretrizInativa_LancaInvalidOperation()
    {
        var manager = await TestDataBuilder.CreateUserAsync(_fixture.Context, UserRole.Manager);
        var guideline = await TestDataBuilder.CreateGuidelineAsync(
            _fixture.Context,
            manager.Id,
            isActive: false);

        var request = new CreateProjectRequest
        {
            Title = "Projeto",
            Description = "Descrição",
            Division = "Divisão",
            Investment = 1000,
            StartDate = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddDays(30),
            GuidelineId = guideline.Id
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(request, manager.Id));
    }
}
