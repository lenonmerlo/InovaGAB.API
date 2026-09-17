using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using InovaGAB.API.Configuration;
using InovaGAB.API.Data;
using InovaGAB.API.DTOs.Response;
using InovaGAB.API.Exceptions;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace InovaGAB.API.Services.Implementations;

public class AiScoringService : IAiScoringService
{
    private const string PromptVersion = "idea-scoring-v1";

    private static readonly JsonSerializerOptions SuggestionJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly MongoDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly ILogger<AiScoringService> _logger;

    public AiScoringService(
        MongoDbContext context,
        HttpClient httpClient,
        IOptions<GeminiSettings> settings,
        ILogger<AiScoringService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<AiScoreSuggestionResponse?> SuggestScoreAsync(
        string ideaId,
        CancellationToken cancellationToken)
    {
        var idea = await _context.Ideas
            .Find(existingIdea => existingIdea.Id == ideaId && !existingIdea.IsDeleted)
            .FirstOrDefaultAsync();

        if (idea == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new AiScoringUnavailableException(
                "Sugestão por IA não está configurada nesta instância.");
        }

        StrategicGuideline? guideline = null;

        if (idea.GuidelineId != null)
        {
            guideline = await _context.StrategicGuidelines
                .Find(existingGuideline => existingGuideline.Id == idea.GuidelineId)
                .FirstOrDefaultAsync();
        }

        var prompt = BuildPrompt(idea, guideline);

        using var timeoutCts = new CancellationTokenSource(
            TimeSpan.FromSeconds(_settings.TimeoutSeconds));

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCts.Token);

        GeminiSuggestionPayload payload;

        try
        {
            payload = await CallGeminiAsync(prompt, linkedCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Timeout ao chamar o provedor de IA para a ideia {IdeaId}.",
                ideaId);

            throw new AiScoringUnavailableException(
                "O provedor de IA demorou demais para responder. Avalie manualmente.");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "Falha de comunicação com o provedor de IA para a ideia {IdeaId}.",
                ideaId);

            throw new AiScoringUnavailableException(
                "Não foi possível obter uma sugestão da IA no momento. Avalie manualmente.");
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(
                exception,
                "Resposta da IA em formato inválido para a ideia {IdeaId}.",
                ideaId);

            throw new AiScoringUnavailableException(
                "A IA retornou uma resposta em formato inválido. Avalie manualmente.");
        }

        ValidateScore(payload.ImpactScore);
        ValidateScore(payload.FeasibilityScore);
        ValidateScore(payload.AlignmentScore);

        return new AiScoreSuggestionResponse
        {
            ImpactScore = payload.ImpactScore,
            FeasibilityScore = payload.FeasibilityScore,
            AlignmentScore = payload.AlignmentScore,
            Justification = payload.Justification,
            Model = _settings.Model
        };
    }

    private async Task<GeminiSuggestionPayload> CallGeminiAsync(
        string prompt,
        CancellationToken cancellationToken)
    {
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                temperature = 0.2
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(requestBody)
        };

        request.Headers.Add("x-goog-api-key", _settings.ApiKey);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Gemini retornou status {(int)response.StatusCode}.");
        }

        var envelope = await response.Content
            .ReadFromJsonAsync<GeminiResponseEnvelope>(cancellationToken: cancellationToken);

        var text = envelope?.Candidates?
            .FirstOrDefault()?.Content?.Parts?
            .FirstOrDefault()?.Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new JsonException("Resposta da IA vazia.");
        }

        var payload = JsonSerializer.Deserialize<GeminiSuggestionPayload>(
            text,
            SuggestionJsonOptions);

        if (payload == null)
        {
            throw new JsonException("Não foi possível interpretar a sugestão da IA.");
        }

        return payload;
    }

    private static void ValidateScore(int score)
    {
        if (score is < 0 or > 10)
        {
            throw new AiScoringUnavailableException(
                "A IA retornou uma pontuação fora da faixa de 0 a 10. Avalie manualmente.");
        }
    }

    private static string BuildPrompt(Idea idea, StrategicGuideline? guideline)
    {
        var guidelineText = guideline == null
            ? "Nenhuma diretriz estratégica vinculada."
            : $"Diretriz vinculada: \"{guideline.Title}\" (categoria: {guideline.Category}, campanha: {guideline.Campaign}).";

        return $$"""
            Você é um avaliador de ideias de inovação corporativa. Analise a ideia abaixo e
            responda SOMENTE com um JSON no formato exato:
            {"impactScore": <int 0-10>, "feasibilityScore": <int 0-10>, "alignmentScore": <int 0-10>, "justification": "<até 2 frases>"}

            Critérios (cada um de 0 a 10):
            - impactScore: impacto potencial para a operação;
            - feasibilityScore: viabilidade técnica e operacional;
            - alignmentScore: alinhamento com a diretriz estratégica informada.

            Título: {{idea.Title}}
            Descrição: {{idea.Description}}
            {{guidelineText}}

            Prompt versão: {{PromptVersion}}.
            """;
    }

    private sealed class GeminiSuggestionPayload
    {
        public int ImpactScore { get; set; }
        public int FeasibilityScore { get; set; }
        public int AlignmentScore { get; set; }
        public string Justification { get; set; } = string.Empty;
    }

    private sealed class GeminiResponseEnvelope
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart>? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
