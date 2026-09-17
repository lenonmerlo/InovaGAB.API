using System.Net;
using System.Net.Http.Json;
using InovaGAB.API.Tests.TestSupport;

namespace InovaGAB.API.Tests.IntegrationTests;

public class AuthIntegrationTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public AuthIntegrationTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_Retorna200EToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/Auth/login",
            new { email = AuthHelper.ManagerEmail, password = AuthHelper.Password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var token = await AuthHelper.LoginAsync(
            _factory.CreateClient(),
            AuthHelper.ManagerEmail);
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Login_ComSenhaInvalida_Retorna401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/Auth/login",
            new { email = AuthHelper.ManagerEmail, password = "senha-errada" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_SempreCriaOperator_MesmoTentandoDefinirOutraRole()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/Auth/register",
            new
            {
                name = "Usuário Teste",
                email = $"{Guid.NewGuid():N}@teste.com",
                password = "Senha@123",
                division = "Testes",
                role = "Leader"
            });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.Equal("Operator", body.GetProperty("role").GetString());
    }
}
