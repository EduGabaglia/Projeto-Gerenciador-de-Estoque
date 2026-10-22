using Microsoft.AspNetCore.Mvc;
using projeto_estoque_api.Dtos;
using projeto_estoque_api.Services;

namespace projeto_estoque_api.Controllers;

[ApiController]
[Route("api/estoque")]
public class EstoqueController : ControllerBase
{
    private readonly EstoqueService _estoque;

    public EstoqueController(EstoqueService estoque) => _estoque = estoque;

    /// <summary>Posição de estoque dos produtos ativos (saldo e status).</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 8)
    {
        return Ok(await _estoque.ListarAsync(busca, pagina, itensPorPagina));
    }

    /// <summary>Registra entrada, saída ou ajuste de estoque e atualiza o saldo do produto.</summary>
    [HttpPost("movimentos")]
    public async Task<IActionResult> Movimentar([FromBody] MovimentoSalvarDto dados)
    {
        return Ok(await _estoque.MovimentarAsync(dados));
    }

    /// <summary>Histórico de movimentações, opcionalmente filtrado por produto.</summary>
    [HttpGet("movimentos")]
    public async Task<IActionResult> Movimentos(
        [FromQuery] int? produtoId,
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 10)
    {
        return Ok(await _estoque.ListarMovimentosAsync(produtoId, pagina, itensPorPagina));
    }
}
