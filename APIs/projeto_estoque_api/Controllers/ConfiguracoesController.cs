using Microsoft.AspNetCore.Mvc;
using projeto_estoque_api.Dtos;
using projeto_estoque_api.Services;

namespace projeto_estoque_api.Controllers;

[ApiController]
[Route("api/configuracoes")]
public class ConfiguracoesController : ControllerBase
{
    private readonly ConfiguracaoService _configuracoes;

    public ConfiguracoesController(ConfiguracaoService configuracoes) => _configuracoes = configuracoes;

    /// <summary>Dados da empresa (cadastro único). Preenchido=false enquanto não houver registro.</summary>
    [HttpGet("empresa")]
    public async Task<IActionResult> ObterEmpresa()
    {
        return Ok(await _configuracoes.ObterAsync());
    }

    [HttpPut("empresa")]
    public async Task<IActionResult> SalvarEmpresa([FromBody] EmpresaSalvarDto dados)
    {
        return Ok(await _configuracoes.SalvarAsync(dados));
    }
}
