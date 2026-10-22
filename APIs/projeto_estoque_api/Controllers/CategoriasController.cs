using Microsoft.AspNetCore.Mvc;
using projeto_estoque_api.Dtos;
using projeto_estoque_api.Services;

namespace projeto_estoque_api.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly CategoriaService _categorias;

    public CategoriasController(CategoriaService categorias) => _categorias = categorias;

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 8)
    {
        return Ok(await _categorias.ListarAsync(busca, pagina, itensPorPagina));
    }

    /// <summary>Lista simples (id + nome) para preencher seletores no sistema web.</summary>
    [HttpGet("opcoes")]
    public async Task<IActionResult> Opcoes()
    {
        return Ok(await _categorias.OpcoesAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        return Ok(await _categorias.ObterAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CategoriaSalvarDto dados)
    {
        return Ok(await _categorias.CriarAsync(dados));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] CategoriaSalvarDto dados)
    {
        return Ok(await _categorias.AtualizarAsync(id, dados));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        await _categorias.ExcluirAsync(id);

        return NoContent();
    }
}
