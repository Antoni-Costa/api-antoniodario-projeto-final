using ApiAntonioDarioProjetoFinal.Data;
using ApiAntonioDarioProjetoFinal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace ApiAntonioDarioProjetoFinal.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProdutosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache; // Redis (L2)
    private readonly IMemoryCache _mem;         // In-memory/Polly (L1)

    public ProdutosController(AppDbContext db, IDistributedCache cache, IMemoryCache mem)
    {
        _db    = db;
        _cache = cache;
        _mem   = mem;
    }

    // GET /api/produtos
    // Cache híbrido: L1 IMemoryCache/Polly (30s) → L2 Redis (5min) → L3 SQL Server
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        const string key = "produtos:all";

        // L1 — IMemoryCache (Polly Cache local): mais rápido, sem rede
        if (_mem.TryGetValue(key, out List<Produto>? memCached))
            return Ok(memCached);

        // L2 — Redis Cache distribuído
        var redisCached = await _cache.GetStringAsync(key);
        if (redisCached != null)
        {
            var fromRedis = JsonSerializer.Deserialize<List<Produto>>(redisCached)!;
            _mem.Set(key, fromRedis, TimeSpan.FromSeconds(30)); // promove ao L1
            return Ok(fromRedis);
        }

        // L3 — Base de dados SQL Server
        var produtos = await _db.Produtos.ToListAsync();
        var json     = JsonSerializer.Serialize(produtos);

        // Guarda no Redis por 5 minutos
        await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        // Guarda no IMemoryCache por 30 segundos
        _mem.Set(key, produtos, TimeSpan.FromSeconds(30));

        return Ok(produtos);
    }

    // GET /api/produtos/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.Produtos.FindAsync(id);
        return p == null ? NotFound(new { mensagem = $"Produto com Id={id} não encontrado." }) : Ok(p);
    }

    // POST /api/produtos
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Produto produto)
    {
        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync();
        await InvalidarCache();
        return CreatedAtAction(nameof(GetById), new { id = produto.Id }, produto);
    }

    // PUT /api/produtos/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Produto produtoAtualizado)
    {
        if (id != produtoAtualizado.Id) return BadRequest();

        var existente = await _db.Produtos.FindAsync(id);
        if (existente == null) return NotFound();

        existente.Nome = produtoAtualizado.Nome;
        existente.Descricao = produtoAtualizado.Descricao;
        existente.Preco = produtoAtualizado.Preco;
        existente.Stock = produtoAtualizado.Stock;
        existente.SKU = produtoAtualizado.SKU;
        // Não mexemos no "existente.CriadoEm"

        await _db.SaveChangesAsync(); 
        await InvalidarCache();
        return NoContent();
    }

    // DELETE /api/produtos/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Produtos.FindAsync(id);
        if (p == null) return NotFound(new { mensagem = $"Produto com Id={id} não encontrado." });

        _db.Produtos.Remove(p);
        await _db.SaveChangesAsync();
        await InvalidarCache();
        return NoContent();
    }

    // Invalida ambos os níveis de cache quando os dados mudam
    private async Task InvalidarCache()
    {
        await _cache.RemoveAsync("produtos:all"); // L2 Redis
        _mem.Remove("produtos:all");              // L1 IMemoryCache
    }
}