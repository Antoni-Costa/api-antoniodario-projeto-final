using ApiAntonioDarioProjetoFinal.Data;
using ApiAntonioDarioProjetoFinal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ApiAntonioDarioProjetoFinal.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProdutosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache;

    public ProdutosController(AppDbContext db, IDistributedCache cache)
    { _db = db; _cache = cache; }

    // GET /api/produtos — lista todos (com cache Redis por 5 minutos)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        const string key = "produtos:all";
        var cached = await _cache.GetStringAsync(key);
        if (cached != null)
            return Ok(JsonSerializer.Deserialize<List<Produto>>(cached));

        var produtos = await _db.Produtos.ToListAsync();
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(produtos),
            new DistributedCacheEntryOptions {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        return Ok(produtos);
    }

    // GET /api/produtos/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.Produtos.FindAsync(id);
        return p == null ? NotFound() : Ok(p);
    }

    // POST /api/produtos
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Produto produto)
    {
        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync("produtos:all");
        return CreatedAtAction(nameof(GetById), new { id = produto.Id }, produto);
    }

    // PUT /api/produtos/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Produto produto)
    {
        if (id != produto.Id) return BadRequest();
        _db.Entry(produto).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync("produtos:all");
        return NoContent();
    }

    // DELETE /api/produtos/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Produtos.FindAsync(id);
        if (p == null) return NotFound();
        _db.Produtos.Remove(p);
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync("produtos:all");
        return NoContent();
    }
}