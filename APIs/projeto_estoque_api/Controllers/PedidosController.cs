using Microsoft.AspNetCore.Mvc;
using projeto_estoque_api.Dtos;
using projeto_estoque_api.Services;

namespace projeto_estoque_api.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly PedidoService _pedidos;

    public PedidosController(PedidoService pedidos) => _pedidos = pedidos;

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] string? status,
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 8)
    {
        return Ok(await _pedidos.ListarAsync(busca, status, pagina, itensPorPagina));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        return Ok(await _pedidos.ObterAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] PedidoSalvarDto dados)
    {
        var pedido = await _pedidos.CriarAsync(dados);

        return CreatedAtAction(nameof(Obter), new { id = pedido.Id }, pedido);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] PedidoSalvarDto dados)
    {
        return Ok(await _pedidos.AtualizarAsync(id, dados));
    }

    /// <summary>Altera o status do pedido (cancela devolvendo estoque; reativa reservando de novo).</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> AlterarStatus(int id, [FromBody] StatusPedidoDto dados)
    {
        return Ok(await _pedidos.AtualizarStatusAsync(id, dados.Status));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        await _pedidos.ExcluirAsync(id);

        return NoContent();
    }
}
