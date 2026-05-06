using ApiAntonioDarioProjetoFinal.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// ── BASE DE DADOS ─────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// ── REDIS CACHE ───────────────────────────────────────────────
var redisConn = builder.Configuration["Redis:ConnectionString"]
             ?? builder.Configuration["Redis__ConnectionString"]
             ?? "redis:6379";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConn;
    options.InstanceName = "ProjetoFinal:";
});

// ── MEMORY CACHE (Polly L1) ───────────────────────────────────
builder.Services.AddMemoryCache();

// ── POLLY — HTTP client para o Imposter ──────────────────────
builder.Services.AddHttpClient("ImposterClient")
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

// ── JWT ───────────────────────────────────────────────────────
var jwtKey      = builder.Configuration["Jwt:Key"]      ?? builder.Configuration["Jwt__Key"]      ?? "ChaveSecretaMuitoLongaParaJWT2026AntonioDario!";
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]   ?? builder.Configuration["Jwt__Issuer"]   ?? "ApiAntonioDario";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? builder.Configuration["Jwt__Audience"] ?? "ApiAntonioDarioUsers";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer   = true,
            ValidIssuer      = jwtIssuer,
            ValidateAudience = true,
            ValidAudience    = jwtAudience,
            ValidateLifetime = true,
            ClockSkew        = TimeSpan.Zero,
            // Garante que o campo "role" do JWT é mapeado para ClaimTypes.Role
            RoleClaimType    = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// ── CORS ──────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ── SWAGGER ───────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title       = "API Antonio Dario — Projeto Final UC605",
        Version     = "v1",
        Description = "API REST com SQL Server, Redis Cache (híbrido Polly+Redis), Polly e Mountebank"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        In          = ParameterLocation.Header,
        Description = "Insere: Bearer {token}  (sem chavetas)",
        Name        = "Authorization",
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {{
        new OpenApiSecurityScheme {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        }, Array.Empty<string>() // Sintaxe otimizada para .NET
    }});
});

var app = builder.Build();

// ── MIDDLEWARE PIPELINE ───────────────────────────────────────
app.UseCors("PermitirFrontend");
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── MIGRAÇÕES AUTOMÁTICAS (Docker) ───────────────────────────
// Aplica migrações pendentes ao arrancar — necessário no Docker
// porque a BD é criada pelo EF e não pelo script SQL externo
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var retries = 0;
    while (retries < 10)
    {
        try
        {
            db.Database.EnsureCreated();
            Console.WriteLine(">>> Base de dados verificada/atualizada com sucesso.");
            break;
        }
        catch (Exception ex)
        {
            retries++;
            Console.WriteLine($">>> Tentativa {retries}/10 — BD não pronta ainda: {ex.Message}");
            Thread.Sleep(3000); 
        }
    }
}

app.Run();

// ── POLÍTICAS POLLY ───────────────────────────────────────────

// Retry: tenta 3x com espera exponencial (2s, 4s, 8s)
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
    HttpPolicyExtensions.HandleTransientHttpError()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

// Circuit Breaker: após 5 falhas consecutivas, para 30 segundos
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
    HttpPolicyExtensions.HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));