namespace InovaGAB.API.Exceptions
{
    // provedor de IA indisponível, timeout ou resposta inválida; nunca deve
    // bloquear a avaliação manual do gestor
    public class AiScoringUnavailableException : Exception
    {
        public AiScoringUnavailableException(string message)
            : base(message)
        {
        }

        public AiScoringUnavailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
