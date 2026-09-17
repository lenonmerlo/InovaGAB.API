using InovaGAB.API.DTOs.Response;

namespace InovaGAB.API.Services.Interfaces
{
    public interface IAiScoringService
    {
        // retorna null se a ideia não existir; lança AiScoringUnavailableException
        // em caso de timeout, falha do provedor ou resposta inválida
        Task<AiScoreSuggestionResponse?> SuggestScoreAsync(
            string ideaId,
            CancellationToken cancellationToken);
    }
}
