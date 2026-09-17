using System.Net;
using InovaGAB.API.Configuration;
using InovaGAB.API.Exceptions;
using InovaGAB.API.Services.Implementations;
using InovaGAB.API.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace InovaGAB.API.Tests.ServiceTests;

// substitui a chamada de rede real ao Gemini por um HttpMessageHandler
// falso, cobrindo sucesso, JSON invalido, falha do provedor e timeout
// sem depender de rede nem de uma chave de API real
public class AiScoringServiceTests : IClassFixture<MongoTestFixture>
{
    private readonly MongoTestFixture _fixture;

    public AiScoringServiceTests(MongoTestFixture fixture)
    {
        _fixture = fixture;
    }

    private AiScoringService BuildService(
        FakeHttpMessageHandler handler,
        string apiKey = "chave-de-teste",
        int timeoutSeconds = 5)
    {
        var settings = Options.Create(new GeminiSettings
        {
            ApiKey = apiKey,
            Model = "gemini-3.5-flash-lite",
            TimeoutSeconds = timeoutSeconds
        });

        return new AiScoringService(
            _fixture.Context,
            new HttpClient(handler),
            settings,
            NullLogger<AiScoringService>.Instance);
    }

    [Fact]
    public async Task SuggestScoreAsync_IdeiaInexistente_RetornaNull()
    {
        var handler = FakeHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "{}");
        var service = BuildService(handler);

        var result = await service.SuggestScoreAsync(
            "000000000000000000000000",
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task SuggestScoreAsync_SemChaveConfigurada_LancaUnavailable()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(_fixture.Context, author.Id);

        var handler = FakeHttpMessageHandler.ReturningJson(HttpStatusCode.OK, "{}");
        var service = BuildService(handler, apiKey: "");

        await Assert.ThrowsAsync<AiScoringUnavailableException>(
            () => service.SuggestScoreAsync(idea.Id, CancellationToken.None));
    }

    [Fact]
    public async Task SuggestScoreAsync_RespostaValida_RetornaSugestao()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(_fixture.Context, author.Id);

        const string geminiBody = """
            {
              "candidates": [
                {
                  "content": {
                    "parts": [
                      { "text": "{\"impactScore\":8,\"feasibilityScore\":6,\"alignmentScore\":9,\"justification\":\"ok\"}" }
                    ]
                  }
                }
              ]
            }
            """;

        var handler = FakeHttpMessageHandler.ReturningJson(HttpStatusCode.OK, geminiBody);
        var service = BuildService(handler);

        var result = await service.SuggestScoreAsync(idea.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(8, result!.ImpactScore);
        Assert.Equal(6, result.FeasibilityScore);
        Assert.Equal(9, result.AlignmentScore);
        Assert.Equal("ok", result.Justification);
    }

    [Fact]
    public async Task SuggestScoreAsync_JsonInvalido_LancaUnavailable()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(_fixture.Context, author.Id);

        var handler = FakeHttpMessageHandler.ReturningJson(
            HttpStatusCode.OK,
            "isso nao e um json valido");
        var service = BuildService(handler);

        await Assert.ThrowsAsync<AiScoringUnavailableException>(
            () => service.SuggestScoreAsync(idea.Id, CancellationToken.None));
    }

    [Fact]
    public async Task SuggestScoreAsync_ScoreForaDaFaixa_LancaUnavailable()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(_fixture.Context, author.Id);

        const string geminiBody = """
            {
              "candidates": [
                {
                  "content": {
                    "parts": [
                      { "text": "{\"impactScore\":42,\"feasibilityScore\":6,\"alignmentScore\":9,\"justification\":\"ok\"}" }
                    ]
                  }
                }
              ]
            }
            """;

        var handler = FakeHttpMessageHandler.ReturningJson(HttpStatusCode.OK, geminiBody);
        var service = BuildService(handler);

        await Assert.ThrowsAsync<AiScoringUnavailableException>(
            () => service.SuggestScoreAsync(idea.Id, CancellationToken.None));
    }

    [Fact]
    public async Task SuggestScoreAsync_FalhaDoProvedor_LancaUnavailable()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(_fixture.Context, author.Id);

        var handler = FakeHttpMessageHandler.ReturningJson(
            HttpStatusCode.InternalServerError,
            "{\"error\":\"boom\"}");
        var service = BuildService(handler);

        await Assert.ThrowsAsync<AiScoringUnavailableException>(
            () => service.SuggestScoreAsync(idea.Id, CancellationToken.None));
    }

    [Fact]
    public async Task SuggestScoreAsync_Timeout_LancaUnavailable()
    {
        var author = await TestDataBuilder.CreateUserAsync(_fixture.Context);
        var idea = await TestDataBuilder.CreateIdeaAsync(_fixture.Context, author.Id);

        var handler = FakeHttpMessageHandler.Delaying(TimeSpan.FromSeconds(2));
        var service = BuildService(handler, timeoutSeconds: 1);

        await Assert.ThrowsAsync<AiScoringUnavailableException>(
            () => service.SuggestScoreAsync(idea.Id, CancellationToken.None));
    }
}
