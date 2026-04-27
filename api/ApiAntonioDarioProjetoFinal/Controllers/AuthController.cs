using ApiAntonioDarioProjetoFinal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiAntonioDarioProjetoFinal.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    { _db = db; _config = config; }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (await _db.Utilizadores.AnyAsync(u => u.Email == req.Email))
            return Conflict(new { mensagem = "Email já registado" });

        var u = new Models.Utilizador {
            Nome = req.Nome, Email = req.Email, Password = req.Password, Role = "User"
        };
        _db.Utilizadores.Add(u);
        await _db.SaveChangesAsync();
        return Created("", new { u.Id, u.Email, u.Nome });
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Utilizadores
            .FirstOrDefaultAsync(u => u.Email == req.Email && u.Password == req.Password);

        if (user == null)
            return Unauthorized(new { mensagem = "Email ou password incorretos" });

        var token = GerarToken(user.Email, user.Role, user.Id);
        return Ok(new { token, user.Nome, user.Email, user.Role });
    }

    private string GerarToken(string email, string role, int id)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var claims = new[] {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.NameIdentifier, id.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Nome, string Email, string Password);