using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InovaGAB.API.Tests.TestSupport;

namespace InovaGAB.API.Tests.IntegrationTests;

public class GuidelineLinkIntegrationTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public GuidelineLinkIntegrationTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CriarIdeia_ComDiretrizAtiva_VinculaEExpoeResumo()
    {
        var leaderClient = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.LeaderEmail);

        var guidelineId = await CreateGuidelineAsync(leaderClient);

        var operatorClient = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.OperatorEmail);

        var response = await operatorClient.PostAsJsonAsync(
            "/api/Idea",
            new
            {
                title = "Ideia vinculada",
                description = "Descrição",
                division = "Testes",
                evidenceUrl = (string?)null,
                challengeId = (string?)null,
                guidelineId
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(guidelineId, body.GetProperty("guidelineId").GetString());
        Assert.True(body.TryGetProperty("guideline", out var guideline));
        Assert.False(guideline.ValueKind == JsonValueKind.Null);
    }

    [Fact]
    public async Task CriarIdeia_ComDiretrizInativa_Retorna400()
    {
        var leaderClient = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.LeaderEmail);

        var guidelineId = await CreateGuidelineAsync(leaderClient);

        var deleteResponse = await leaderClient.DeleteAsync(
            $"/api/Guideline/{guidelineId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var operatorClient = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.OperatorEmail);

        var response = await operatorClient.PostAsJsonAsync(
            "/api/Idea",
            new
            {
                title = "Não deveria ser criada",
                description = "Descrição",
                division = "Testes",
                evidenceUrl = (string?)null,
                challengeId = (string?)null,
                guidelineId
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AtualizarDiretriz_CriaNovaVersaoEHistoricoPermanece()
    {
        var leaderClient = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.LeaderEmail);

        var guidelineId = await CreateGuidelineAsync(leaderClient);

        var updateResponse = await leaderClient.PutAsJsonAsync(
            $"/api/Guideline/{guidelineId}",
            new
            {
                title = "Diretriz atualizada",
                description = "Descrição nova",
                category = "Categoria",
                campaign = "Campanha",
                priority = "Medium"
            });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<JsonElement>();
        var newId = updated.GetProperty("id").GetString();

        var historyResponse = await leaderClient.GetAsync(
            $"/api/Guideline/{newId}/history");
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);

        var history = await historyResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, history.GetArrayLength());
    }

    private static async Task<string> CreateGuidelineAsync(HttpClient leaderClient)
    {
        var response = await leaderClient.PostAsJsonAsync(
            "/api/Guideline",
            new
            {
                title = "Diretriz de integração",
                description = "Descrição",
                category = "Categoria",
                campaign = "Campanha",
                priority = "High"
            });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetString()!;
    }
}
