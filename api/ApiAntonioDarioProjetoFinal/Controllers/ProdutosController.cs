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
    private readonly IHttpClientFactory _clientFactory;

    public ProdutosController(AppDbContext db, IDistributedCache cache, IHttpClientFactory clientFactory)
    { 
        _db = db; 
        _cache = cache; 
        _clientFactory = clientFactory;
    }

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

    // NOVO MÉTODO: GET /api/produtos/stock/{sku}
    // Este método resolve o erro 404 e usa o Mountebank + Polly
    [HttpGet("stock/{sku}")]
    public async Task<IActionResult> GetStock(string sku)
    {
        var client = _clientFactory.CreateClient("ImposterClient");
        
        // Chamada ao Mountebank usando o nome do serviço no Docker
        var response = await client.GetAsync($"http://mountebank:3001/stock/{sku}");

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, "Erro ao consultar stock externo");

        var content = await response.Content.ReadAsStringAsync();
        return Ok(JsonSerializer.Deserialize<object>(content));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.Produtos.FindAsync(id);
        return p == null ? NotFound() : Ok(p);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Produto produto)
    {
        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync("produtos:all");
        return CreatedAtAction(nameof(GetById), new { id = produto.Id }, produto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Produto produto)
    {
        if (id != produto.Id) return BadRequest();
        _db.Entry(produto).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync("produtos:all");
        return NoContent();
    }

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