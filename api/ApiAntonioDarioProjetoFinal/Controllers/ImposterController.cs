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
    [HttpGet("inventory/{sku}")]
    public async Task<IActionResult> GetInventory(string sku)
    {
        var client = _http.CreateClient("ImposterClient");
        var response = await client.GetAsync($"http://localhost:3001/inventory/{sku}");
        return Ok(await response.Content.ReadAsStringAsync());
    }

    // POST /api/imposter/payments
    [HttpPost("payments")]
    public async Task<IActionResult> ProcessPayment([FromBody] object body)
    {
        var client = _http.CreateClient("ImposterClient");
        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(body),
            System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("http://localhost:3002/payments", content);
        return Ok(await response.Content.ReadAsStringAsync());
    }
}