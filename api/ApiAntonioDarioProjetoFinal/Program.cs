using ApiAntonioDarioProjetoFinal.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Base de Dados com Resiliência ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// --- 2. Cache (Redis e Memory) ---
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "ProjetoFinal:";
});
builder.Services.AddMemoryCache();

// --- 3. Polly (Resiliência para Clientes HTTP) ---
builder.Services.AddHttpClient("ImposterClient")
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

// --- 4. Autenticação JWT ---
// CHAVE, ISSUER E AUDIENCE FIXOS (Garante 100% de match com o AuthController)
var jwtKey = "ChaveSecretaMuitoLongaParaJWT2026AntonioDario!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenHandlers.Clear(); // Limpa os antigos
    options.TokenHandlers.Add(new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler()); // Usa o novo
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = "ApiAntonioDario",         // Fixo (Hardcoded)
        ValidateAudience = true,
        ValidAudience = "ApiAntonioDarioUsers",  // Fixo (Hardcoded)
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
    
        // LOG DE EMERGÊNCIA: Vamos ver o que está a chegar no terminal
        Console.WriteLine($"--- DEBUG HEADER ---");
        Console.WriteLine($"Recebido: '{authHeader}'"); 
    
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            // Limpeza agressiva
            token = token.Replace("\"", "").Replace("\n", "").Replace("\r", "").Trim();
            context.Token = token;
        
            Console.WriteLine($"Token Extraído: '{context.Token}'");
        }
        return Task.CompletedTask;
    },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("--- ERRO DE AUTENTICAÇÃO DETETADO ---");
            Console.WriteLine($"Erro: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// --- 5. Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Antonio Dario", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insira o token desta forma: Bearer {teu_token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {{
        new OpenApiSecurityScheme {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        }, new string[] {}
    }});
});

var app = builder.Build();

// --- 6. Pipeline de Execução ---
if (app.Environment.IsDevelopment() || true) // Forçado true para facilitar no Docker
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// --- 7. Políticas Polly ---
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
    HttpPolicyExtensions.HandleTransientHttpError()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
    HttpPolicyExtensions.HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));