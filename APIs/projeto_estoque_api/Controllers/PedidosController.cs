using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using projeto_estoque_api.Common;
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

    /// <summary>Pedidos do usuário autenticado (app mobile).</summary>
    [Authorize]
    [HttpGet("meus")]
    public async Task<IActionResult> Meus(
        [FromQuery] string? status,
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 20)
    {
        var usuarioId = UsuarioId();
        return Ok(await _pedidos.ListarDoUsuarioAsync(usuarioId, status, pagina, itensPorPagina));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        return Ok(await _pedidos.ObterAsync(id, User.UsuarioId()));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] PedidoSalvarDto dados)
    {
        var pedido = await _pedidos.CriarAsync(dados, User.UsuarioId());

        return CreatedAtAction(nameof(Obter), new { id = pedido.Id }, pedido);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] PedidoSalvarDto dados)
    {
        if (User.UsuarioId() is not null)
            throw ErroDeNegocioException.Proibido("Pelo aplicativo não é possível editar pedidos.");

        return Ok(await _pedidos.AtualizarAsync(id, dados, null));
    }

    /// <summary>Altera o status do pedido (cancela devolvendo estoque; reativa reservando de novo).</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> AlterarStatus(int id, [FromBody] StatusPedidoDto dados)
    {
        var usuarioId = User.UsuarioId();
        var novoStatus = StatusPedido.Converter(dados.Status);

        if (usuarioId is not null)
        {
            if (novoStatus is null)
                throw new ErroDeNegocioException("Status inválido.");

            if (novoStatus != "CANCELADO")
                throw ErroDeNegocioException.Proibido("Pelo aplicativo é possível apenas cancelar pedidos.");
        }

        return Ok(await _pedidos.AtualizarStatusAsync(id, dados.Status, usuarioId));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        if (User.UsuarioId() is not null)
            throw ErroDeNegocioException.Proibido("Pelo aplicativo não é possível excluir pedidos.");

        await _pedidos.ExcluirAsync(id);

        return NoContent();
    }

    private int UsuarioId()
    {
        return User.UsuarioId()
            ?? throw ErroDeNegocioException.NaoAutorizado("Sessão inválida. Entre novamente.");
    }
}
