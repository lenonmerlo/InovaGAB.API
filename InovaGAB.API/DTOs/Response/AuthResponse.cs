namespace InovaGAB.API.DTOs.Response
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
    }
}
