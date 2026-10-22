using Microsoft.AspNetCore.Mvc;
using projeto_estoque_api.Dtos;
using projeto_estoque_api.Services;

namespace projeto_estoque_api.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly ProdutoService _produtos;

    public ProdutosController(ProdutoService produtos) => _produtos = produtos;

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] int? categoriaId,
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 8)
    {
        return Ok(await _produtos.ListarAsync(busca, categoriaId, pagina, itensPorPagina));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        return Ok(await _produtos.ObterAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ProdutoSalvarDto dados)
    {
        var produto = await _produtos.CriarAsync(dados);

        return CreatedAtAction(nameof(Obter), new { id = produto.Id }, produto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ProdutoSalvarDto dados)
    {
        return Ok(await _produtos.AtualizarAsync(id, dados));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        await _produtos.ExcluirAsync(id);

        return NoContent();
    }
}
