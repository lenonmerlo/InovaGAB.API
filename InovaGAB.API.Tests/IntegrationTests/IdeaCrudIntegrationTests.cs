using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InovaGAB.API.Tests.TestSupport;

namespace InovaGAB.API.Tests.IntegrationTests;

public class IdeaCrudIntegrationTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public IdeaCrudIntegrationTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FluxoCompleto_CriarConsultarEditarExcluir()
    {
        var client = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.OperatorEmail);

        var createResponse = await client.PostAsJsonAsync(
            "/api/Idea",
            new
            {
                title = "Ideia de integração",
                description = "Descrição",
                division = "Testes",
                evidenceUrl = (string?)null,
                challengeId = (string?)null,
                guidelineId = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        var getResponse = await client.GetAsync($"/api/Idea/{id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/Idea/{id}",
            new { title = "Ideia de integração (editada)" });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/Idea/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDelete = await client.GetAsync($"/api/Idea/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
    public async Task Prioritize_ComoManager_Retorna200EComoOperator_Retorna403()
    {
        var operatorClient = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.OperatorEmail);

        var createResponse = await operatorClient.PostAsJsonAsync(
            "/api/Idea",
            new
            {
                title = "Ideia para priorizar",
                description = "Descrição",
                division = "Testes",
                evidenceUrl = (string?)null,
                challengeId = (string?)null,
                guidelineId = (string?)null
            });

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        var managerClient = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.ManagerEmail);

        var prioritizeResponse = await managerClient.PatchAsync(
            $"/api/Idea/{id}/prioritize",
            JsonContent.Create(new { priority = "High" }));
        Assert.Equal(HttpStatusCode.OK, prioritizeResponse.StatusCode);

        var forbiddenResponse = await operatorClient.PatchAsync(
            $"/api/Idea/{id}/prioritize",
            JsonContent.Create(new { priority = "Low" }));
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
    }
}
