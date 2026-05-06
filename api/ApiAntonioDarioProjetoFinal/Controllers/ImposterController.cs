using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiAntonioDarioProjetoFinal.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImposterController : ControllerBase
{
    private readonly IHttpClientFactory _http;
    public ImposterController(IHttpClientFactory http) => _http = http;

    // GET /api/imposter/inventory/{sku}
    // Chama o Mountebank com retry automático
    [HttpGet("inventory/{sku}")]
public async Task<IActionResult> GetInventory(string sku)
{
    try
    {
        var client   = _http.CreateClient("ImposterClient");
        var response = await client.GetAsync($"http://mountebank:3001/inventory/{sku}");
        var body     = await response.Content.ReadAsStringAsync();
        
        // Propaga o Status Code real (e.g., 404, 500) em vez de forçar Ok()
        return StatusCode((int)response.StatusCode, body);
    }
    catch (Polly.CircuitBreaker.BrokenCircuitException)
    {
        // Devolve o Erro 503 para acionar o alerta no Frontend
        return StatusCode(503, new { mensagem = "Serviço indisponível temporariamente (Circuit Breaker Aberto)." });
    }
}

    // POST /api/imposter/payments
    [HttpPost("payments")]
    public async Task<IActionResult> ProcessPayment([FromBody] object body)
    {
        var client  = _http.CreateClient("ImposterClient");
        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(body),
            System.Text.Encoding.UTF8,
            "application/json");
        var response = await client.PostAsync("http://mountebank:3001/payments", content);
        var result   = await response.Content.ReadAsStringAsync();
        return Ok(result);
    }
}