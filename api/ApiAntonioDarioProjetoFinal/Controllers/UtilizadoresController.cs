using ApiAntonioDarioProjetoFinal.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiAntonioDarioProjetoFinal.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UtilizadoresController : ControllerBase
{
    private readonly AppDbContext _db;
    public UtilizadoresController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Utilizadores
            .Select(u => new { u.Id, u.Nome, u.Email, u.Role, u.CriadoEm })
            .ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var u = await _db.Utilizadores.FindAsync(id);
        return u == null ? NotFound() : Ok(u);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await _db.Utilizadores.FindAsync(id);
        if (u == null) return NotFound();
        _db.Utilizadores.Remove(u);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}