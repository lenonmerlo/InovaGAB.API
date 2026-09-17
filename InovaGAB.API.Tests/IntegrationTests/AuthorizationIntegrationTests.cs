using System.Net;
using InovaGAB.API.Tests.TestSupport;

namespace InovaGAB.API.Tests.IntegrationTests;

public class AuthorizationIntegrationTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public AuthorizationIntegrationTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Endpoint_SemToken_Retorna401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/Guideline");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Endpoint_ComRoleErrada_Retorna403()
    {
        var client = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.OperatorEmail);

        var response = await client.GetAsync("/api/Dashboard");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Endpoint_ComRoleCorreta_Retorna200()
    {
        var client = await AuthHelper.AuthenticatedClientAsync(
            _factory,
            AuthHelper.LeaderEmail);

        var response = await client.GetAsync("/api/Dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
