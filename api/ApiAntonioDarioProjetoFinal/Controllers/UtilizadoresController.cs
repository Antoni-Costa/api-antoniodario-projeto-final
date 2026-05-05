using ApiAntonioDarioProjetoFinal.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApiAntonioDarioProjetoFinal.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UtilizadoresController : ControllerBase
{
    private readonly AppDbContext _db;
    public UtilizadoresController(AppDbContext db) => _db = db;

    // GET /api/utilizadores — apenas Admins
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var lista = await _db.Utilizadores
            .Select(u => new { u.Id, u.Nome, u.Email, u.Role, u.CriadoEm })
            .ToListAsync();

        return Ok(lista);
    }

    // GET /api/utilizadores/{id}
    // Admins vêem qualquer um; Users só vêem o seu próprio perfil
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userIdLogado = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole     = User.FindFirstValue(ClaimTypes.Role);

        if (userRole != "Admin" && userIdLogado != id.ToString())
            return Forbid();

        var u = await _db.Utilizadores
            .Where(x => x.Id == id)
            .Select(u => new { u.Id, u.Nome, u.Email, u.Role, u.CriadoEm })
            .FirstOrDefaultAsync();

        if (u == null) return NotFound(new { mensagem = "Utilizador não encontrado." });

        return Ok(u);
    }

    // DELETE /api/utilizadores/{id} — apenas Admins
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdLogado = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Impede que um Admin apague a própria conta
        if (userIdLogado == id.ToString())
            return BadRequest(new { mensagem = "Não pode apagar a sua própria conta de administrador." });

        var u = await _db.Utilizadores.FindAsync(id);
        if (u == null) return NotFound(new { mensagem = "Utilizador não encontrado." });

        _db.Utilizadores.Remove(u);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}