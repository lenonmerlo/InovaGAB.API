using InovaGAB.API.DTOs.Request;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Implementations;
using InovaGAB.API.Tests.TestSupport;

namespace InovaGAB.API.Tests.ServiceTests;

public class GuidelineServiceTests : IClassFixture<MongoTestFixture>
{
    private readonly MongoTestFixture _fixture;
    private readonly GuidelineService _service;

    public GuidelineServiceTests(MongoTestFixture fixture)
    {
        _fixture = fixture;
        _service = new GuidelineService(fixture.Context);
    }

    [Fact]
    public async Task CreateAsync_PrimeiraVersao_RootIdIgualAoProprioId()
    {
        var leader = await TestDataBuilder.CreateUserAsync(_fixture.Context, UserRole.Leader);

        var created = await _service.CreateAsync(
            new CreateGuidelineRequest
            {
                Title = "Diretriz",
                Description = "Descrição",
                Category = "Categoria",
                Campaign = "Campanha",
                Priority = "High"
            },
            leader.Id);

        Assert.Equal(created.Id, created.RootId);
        Assert.Equal(1, created.Version);
        Assert.True(created.IsCurrent);
    }

    // regressão: a atualização inseria a nova versão como vigente antes de
    // rebaixar a anterior, violando o índice único (rootId + isCurrent) e
    // derrubando a requisição com 500
    [Fact]
    public async Task UpdateAsync_CriaNovaVersaoSemViolarIndiceUnico()
    {
        var leader = await TestDataBuilder.CreateUserAsync(_fixture.Context, UserRole.Leader);

        var v1 = await _service.CreateAsync(
            new CreateGuidelineRequest
            {
                Title = "Diretriz",
                Description = "Descrição",
                Category = "Categoria",
                Campaign = "Campanha",
                Priority = "High"
            },
            leader.Id);

        var v2 = await _service.UpdateAsync(
            v1.Id,
            new CreateGuidelineRequest
            {
                Title = "Diretriz atualizada",
                Description = "Descrição nova",
                Category = "Categoria",
                Campaign = "Campanha",
                Priority = "Medium"
            },
            leader.Id);

        Assert.NotNull(v2);
        Assert.Equal(2, v2!.Version);
        Assert.True(v2.IsCurrent);
        Assert.Equal(v1.Id, v2.PreviousVersionId);
        Assert.Equal(v1.RootId, v2.RootId);

        var v1Reloaded = await _service.GetByIdAsync(v1.Id);
        Assert.False(v1Reloaded!.IsCurrent);
    }

    [Fact]
    public async Task GetHistoryAsync_RetornaTodasAsVersoesDaLinha()
    {
        var leader = await TestDataBuilder.CreateUserAsync(_fixture.Context, UserRole.Leader);

        var v1 = await _service.CreateAsync(
            new CreateGuidelineRequest { Title = "A", Description = "d", Category = "c", Campaign = "camp", Priority = "Low" },
            leader.Id);

        var v2 = await _service.UpdateAsync(
            v1.Id,
            new CreateGuidelineRequest { Title = "B", Description = "d", Category = "c", Campaign = "camp", Priority = "Low" },
            leader.Id);

        var history = await _service.GetHistoryAsync(v2!.Id);

        Assert.NotNull(history);
        Assert.Equal(2, history!.Count);
        Assert.Equal(2, history[0].Version);
        Assert.Equal(1, history[1].Version);
    }

    [Fact]
    public async Task GetAllAsync_ListaApenasVersaoVigente()
    {
        var leader = await TestDataBuilder.CreateUserAsync(_fixture.Context, UserRole.Leader);

        var v1 = await _service.CreateAsync(
            new CreateGuidelineRequest { Title = "A", Description = "d", Category = "c", Campaign = "camp", Priority = "Low" },
            leader.Id);

        await _service.UpdateAsync(
            v1.Id,
            new CreateGuidelineRequest { Title = "B", Description = "d", Category = "c", Campaign = "camp", Priority = "Low" },
            leader.Id);

        var all = await _service.GetAllAsync();

        Assert.Single(all, guideline => guideline.RootId == v1.RootId);
    }
}
