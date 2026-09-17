using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace InovaGAB.API.Tests.TestSupport;

// os usuários usados aqui vêm do DataSeeder, que roda automaticamente no
// startup da aplicação (mesmo seed usado em desenvolvimento)
public static class AuthHelper
{
    public const string OperatorEmail = "joao.operador@aguiabranca.com.br";
    public const string ManagerEmail = "ana.gestora@aguiabranca.com.br";
    public const string LeaderEmail = "carlos.lider@aguiabranca.com.br";
    public const string Password = "senha123";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<string> LoginAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync(
            "/api/Auth/login",
            new { email, password = Password });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        return body!.Token;
    }

    public static async Task<HttpClient> AuthenticatedClientAsync(
        ApiTestFactory factory,
        string email)
    {
        var client = factory.CreateClient();
        var token = await LoginAsync(client, email);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
