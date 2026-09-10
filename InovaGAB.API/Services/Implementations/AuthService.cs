using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InovaGAB.API.Data;
using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace InovaGAB.API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly MongoDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(
        MongoDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse?> LoginAsync(
        LoginRequest request)
    {
        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var user = await _context.Users
            .Find(user => user.Email == normalizedEmail)
            .FirstOrDefaultAsync();

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash))
        {
            return null;
        }

        var token = GenerateToken(user);

        return MapToResponse(user, token);
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request)
    {
        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var existingUser = await _context.Users
            .Find(user => user.Email == normalizedEmail)
            .AnyAsync();

        if (existingUser)
        {
            throw new InvalidOperationException(
                "Já existe um usuário cadastrado com este e-mail.");
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    request.Password),
            Role = UserRole.Operator,
            Division = request.Division.Trim(),
            Points = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.InsertOneAsync(user);

        var token = GenerateToken(user);

        return MapToResponse(user, token);
    }

    private static AuthResponse MapToResponse(
        User user,
        string token)
    {
        return new AuthResponse
        {
            Token = token,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            Division = user.Division
        };
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id),
            new Claim(
                ClaimTypes.Email,
                user.Email),
            new Claim(
                ClaimTypes.Name,
                user.Name),
            new Claim(
                ClaimTypes.Role,
                user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}
