using System.Net;
using InovaGAB.API.Tests.TestSupport;

namespace InovaGAB.API.Tests.IntegrationTests;

public class ObjectIdValidationIntegrationTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public ObjectIdValidationIntegrationTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetIdeaPorId_ComIdInvalido_Retorna400()
    {
        var client = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.OperatorEmail);

        var response = await client.GetAsync("/api/Idea/ID_INVALIDO");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetProjectPorId_ComIdInvalido_Retorna400()
    {
        var client = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.ManagerEmail);

        var response = await client.GetAsync("/api/Project/ID_INVALIDO");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetIdeaPorId_ComIdInexistenteMasValido_Retorna404()
    {
        var client = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.OperatorEmail);

        var response = await client.GetAsync(
            "/api/Idea/000000000000000000000000");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
