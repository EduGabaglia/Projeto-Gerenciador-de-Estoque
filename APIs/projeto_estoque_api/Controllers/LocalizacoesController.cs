using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projeto_estoque_api.Data;
using projeto_estoque_api.Dtos;

namespace projeto_estoque_api.Controllers;

[ApiController]
[Route("api/localizacoes")]
public class LocalizacoesController : ControllerBase
{
    private readonly EstoqueDbContext _db;

    public LocalizacoesController(EstoqueDbContext db) => _db = db;

    /// <summary>Lista de localizações do galpão para preencher seletores no sistema web.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var localizacoes = await _db.LocalizacaoGalpaos
            .AsNoTracking()
            .OrderBy(l => l.Localizacao)
            .Select(l => new OpcaoDto(l.Id, l.Localizacao))
            .ToListAsync();

        return Ok(localizacoes);
    }
}
