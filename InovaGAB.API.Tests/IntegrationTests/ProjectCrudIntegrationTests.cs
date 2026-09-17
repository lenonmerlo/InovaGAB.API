using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InovaGAB.API.Tests.TestSupport;

namespace InovaGAB.API.Tests.IntegrationTests;

public class ProjectCrudIntegrationTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public ProjectCrudIntegrationTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FluxoCompleto_CriarConsultarAtualizarArquivar()
    {
        var client = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.ManagerEmail);

        var createResponse = await client.PostAsJsonAsync(
            "/api/Project",
            new
            {
                title = "Projeto de integração",
                description = "Descrição",
                division = "Testes",
                investment = 10000,
                startDate = DateTime.UtcNow,
                deadline = DateTime.UtcNow.AddDays(30)
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        var getResponse = await client.GetAsync($"/api/Project/{id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/Project/{id}",
            new { status = 1 });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var archiveResponse = await client.DeleteAsync($"/api/Project/{id}");
        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        var listResponse = await client.GetAsync("/api/Project");
        var list = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.DoesNotContain(
            list.EnumerateArray(),
            project => project.GetProperty("id").GetString() == id);

        var getAfterArchive = await client.GetAsync($"/api/Project/{id}");
        Assert.Equal(HttpStatusCode.OK, getAfterArchive.StatusCode);
    }

    [Fact]
    public async Task Create_ComInvestimentoNegativo_Retorna400()
    {
        var client = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.ManagerEmail);

        var response = await client.PostAsJsonAsync(
            "/api/Project",
            new
            {
                title = "Projeto inválido",
                description = "Descrição",
                division = "Testes",
                investment = -100,
                startDate = DateTime.UtcNow,
                deadline = DateTime.UtcNow.AddDays(30)
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ComoOperator_Retorna403()
    {
        var client = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.OperatorEmail);

        var response = await client.PostAsJsonAsync(
            "/api/Project",
            new
            {
                title = "Não deveria ser criado",
                description = "Descrição",
                division = "Testes",
                investment = 1000,
                startDate = DateTime.UtcNow,
                deadline = DateTime.UtcNow.AddDays(30)
            });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
