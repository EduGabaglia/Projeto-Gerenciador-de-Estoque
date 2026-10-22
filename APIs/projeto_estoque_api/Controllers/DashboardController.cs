using Microsoft.AspNetCore.Mvc;
using projeto_estoque_api.Services;

namespace projeto_estoque_api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboard;

    public DashboardController(DashboardService dashboard) => _dashboard = dashboard;

    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        return Ok(await _dashboard.ObterAsync());
    }
}
