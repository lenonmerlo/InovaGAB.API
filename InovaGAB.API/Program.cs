using System.Text;
using InovaGAB.API.Configuration;
using InovaGAB.API.Data;
using InovaGAB.API.Middleware;
using InovaGAB.API.Services.Implementations;
using InovaGAB.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

MongoDbConventions.Register();

// ── Controllers
builder.Services.AddControllers();

// ── MongoDB
builder.Services
    .AddOptions<MongoDbSettings>()
    .Bind(
        builder.Configuration.GetSection(
            MongoDbSettings.SectionName))
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(
                settings.ConnectionString),
        "MongoDb:ConnectionString é obrigatória.")
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(
                settings.DatabaseName),
        "MongoDb:DatabaseName é obrigatório.")
    .ValidateOnStart();

builder.Services.AddSingleton<IMongoClient>(
    serviceProvider =>
    {
        var settings = serviceProvider
            .GetRequiredService<IOptions<MongoDbSettings>>()
            .Value;

        return new MongoClient(
            settings.ConnectionString);
    });

builder.Services.AddSingleton<IMongoDatabase>(
    serviceProvider =>
    {
        var settings = serviceProvider
            .GetRequiredService<IOptions<MongoDbSettings>>()
            .Value;

        var client = serviceProvider
            .GetRequiredService<IMongoClient>();

        return client.GetDatabase(
            settings.DatabaseName);
    });

builder.Services.AddSingleton<MongoDbContext>();

// ── JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],
                ValidAudience =
                    builder.Configuration["Jwt:Audience"],
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
            };
    });

builder.Services.AddAuthorization();

// ── Swagger com suporte a JWT
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "InovaGAB API",
            Version = "v1"
        });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Informe: Bearer {seu token}"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

// ── CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// ── Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IIdeaService, IdeaService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<
    IGuidelineService,
    GuidelineService>();
builder.Services.AddScoped<
    IDashboardService,
    DashboardService>();
builder.Services.AddScoped<
    IChallengeService,
    ChallengeService>();

var app = builder.Build();

// ── Dados iniciais do MongoDB
using (var scope = app.Services.CreateScope())
{
    var mongoDbContext = scope.ServiceProvider
        .GetRequiredService<MongoDbContext>();

    await DataSeeder.SeedAsync(mongoDbContext);
}

// ── Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<AuditMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
