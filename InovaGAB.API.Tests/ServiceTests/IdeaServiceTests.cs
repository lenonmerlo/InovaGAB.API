using InovaGAB.API.DTOs.Request;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Implementations;
using InovaGAB.API.Tests.TestSupport;
using MongoDB.Driver;

namespace InovaGAB.API.Tests.ServiceTests;

public class IdeaServiceTests : IClassFixture<MongoTestFixture>
{
    private readonly MongoTestFixture _fixture;
    private readonly IdeaService _service;

    public IdeaServiceTests(MongoTestFixture fixture)
    {
        _fixture = fixture;
        _service = new IdeaService(fixture.Context);
    }

    [Fact]
    public async Task ApproveAsync_PrimeiraAprovacao_DaBonusDePontos()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(_fixture.Context, author.Id);

        await _service.ApproveAsync(idea.Id, 8, 7, 9);

        var updatedAuthor = await _fixture.Context.Users
            .Find(user => user.Id == author.Id)
            .FirstOrDefaultAsync();

        Assert.Equal(author.Points + 50, updatedAuthor!.Points);
    }

    [Fact]
    public async Task ApproveAsync_Reaprovacao_NaoDuplicaBonus()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(_fixture.Context, author.Id);

        await _service.ApproveAsync(idea.Id, 8, 7, 9);
        await _service.ApproveAsync(idea.Id, 10, 10, 10);

        var updatedAuthor = await _fixture.Context.Users
            .Find(user => user.Id == author.Id)
            .FirstOrDefaultAsync();

        Assert.Equal(author.Points + 50, updatedAuthor!.Points);
    }

    [Fact]
    public async Task ApproveAsync_ScoreForaDoIntervalo_LancaExcecao()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(_fixture.Context, author.Id);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _service.ApproveAsync(idea.Id, 15, 5, 5));
    }

    [Fact]
    public async Task ApproveAsync_IdeiaRejeitada_LancaExcecao()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(
            _fixture.Context,
            author.Id,
            IdeaStatus.Rejected);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ApproveAsync(idea.Id, 8, 8, 8));
    }

    [Fact]
    public async Task RejectAsync_DeSubmitted_MudaParaRejected()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(_fixture.Context, author.Id);

        var result = await _service.RejectAsync(idea.Id);

        Assert.Equal("Rejected", result!.Status);
    }

    [Fact]
    public async Task RejectAsync_DeIdeiaAprovada_LancaExcecao()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(
            _fixture.Context,
            author.Id,
            IdeaStatus.Approved);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RejectAsync(idea.Id));
    }

    [Fact]
    public async Task UpdateAsync_AutorDiferente_LancaUnauthorized()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var outroUsuario = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(_fixture.Context, author.Id);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.UpdateAsync(
                idea.Id,
                new UpdateIdeaRequest { Title = "Outro título" },
                outroUsuario.Id));
    }

    [Fact]
    public async Task UpdateAsync_ForaDeSubmitted_LancaInvalidOperation()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(
            _fixture.Context,
            author.Id,
            IdeaStatus.Approved);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(
                idea.Id,
                new UpdateIdeaRequest { Title = "Outro título" },
                author.Id));
    }

    [Fact]
    public async Task DeleteAsync_AutorDiferente_LancaUnauthorized()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var outroUsuario = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(_fixture.Context, author.Id);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.DeleteAsync(idea.Id, outroUsuario.Id));
    }

    [Fact]
    public async Task DeleteAsync_AntesDaAvaliacao_ExcluiLogicamente()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(_fixture.Context, author.Id);

        var result = await _service.DeleteAsync(idea.Id, author.Id);

        Assert.Equal(true, result);
        Assert.Null(await _service.GetByIdAsync(idea.Id));
    }

    [Fact]
    public async Task DeleteAsync_ApoisAvaliacao_LancaInvalidOperation()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(
            _fixture.Context,
            author.Id,
            IdeaStatus.Approved);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteAsync(idea.Id, author.Id));
    }

    [Fact]
    public async Task PrioritizeAsync_DefinePrioridadeInformada()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(_fixture.Context, author.Id);

        var result = await _service.PrioritizeAsync(idea.Id, "High");

        Assert.Equal("High", result!.Priority);
    }

    [Fact]
    public async Task PrioritizeAsync_ValorInvalido_LancaArgumentException()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(_fixture.Context, author.Id);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.PrioritizeAsync(idea.Id, "Urgentissimo"));
    }
}
