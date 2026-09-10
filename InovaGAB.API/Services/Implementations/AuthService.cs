using InovaGAB.API.Data;
using InovaGAB.API.DTOs.Request;
using InovaGAB.API.DTOs.Response;
using InovaGAB.API.Models;
using InovaGAB.API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        var user = await _context.Users
            .Find(user => user.Email == request.Email)
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
        var existingUser = await _context.Users
            .Find(user => user.Email == request.Email)
            .AnyAsync();

        if (existingUser)
        {
            throw new InvalidOperationException(
                "Já existe um usuário cadastrado com este e-mail.");
        }

        if (!Enum.TryParse<UserRole>(
                request.Role,
                ignoreCase: true,
                out var role))
        {
            throw new ArgumentException(
                "Perfil de usuário inválido.");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            Division = request.Division,
            Points = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.InsertOneAsync(user);

        var token = GenerateToken(user);

        return MapToResponse(user, token);
    }

    private AuthResponse MapToResponse(
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
