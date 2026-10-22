using Microsoft.AspNetCore.Mvc;
using projeto_estoque_api.Dtos;
using projeto_estoque_api.Services;

namespace projeto_estoque_api.Controllers;

[ApiController]
[Route("api/promocoes")]
public class PromocoesController : ControllerBase
{
    private readonly PromocaoService _promocoes;

    public PromocoesController(PromocaoService promocoes) => _promocoes = promocoes;

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 8)
    {
        return Ok(await _promocoes.ListarAsync(busca, pagina, itensPorPagina));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        return Ok(await _promocoes.ObterAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] PromocaoSalvarDto dados)
    {
        return Ok(await _promocoes.CriarAsync(dados));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] PromocaoSalvarDto dados)
    {
        return Ok(await _promocoes.AtualizarAsync(id, dados));
    }

    /// <summary>Ativa/desativa a promoção (validando conflito de produtos ao ativar).</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> AlternarStatus(int id)
    {
        return Ok(await _promocoes.AlternarStatusAsync(id));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        await _promocoes.ExcluirAsync(id);

        return NoContent();
    }
}
