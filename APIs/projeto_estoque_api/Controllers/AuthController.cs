using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using projeto_estoque_api.Common;
using projeto_estoque_api.Dtos;
using projeto_estoque_api.Services;

namespace projeto_estoque_api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth) => _auth = auth;

    [HttpPost("cadastro")]
    public async Task<IActionResult> Cadastrar([FromBody] CadastroDto dados)
    {
        return Ok(await _auth.CadastrarAsync(dados));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dados)
    {
        return Ok(await _auth.LoginAsync(dados));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        return Ok(await _auth.ObterAsync(UsuarioId()));
    }

    [Authorize]
    [HttpPut("perfil")]
    public async Task<IActionResult> AtualizarPerfil([FromBody] PerfilAtualizarDto dados)
    {
        return Ok(await _auth.AtualizarPerfilAsync(UsuarioId(), dados));
    }

    [Authorize]
    [HttpPut("senha")]
    public async Task<IActionResult> AlterarSenha([FromBody] SenhaAlterarDto dados)
    {
        await _auth.AlterarSenhaAsync(UsuarioId(), dados);

        return NoContent();
    }

    [Authorize]
    [HttpDelete("conta")]
    public async Task<IActionResult> ExcluirConta()
    {
        await _auth.ExcluirAsync(UsuarioId());

        return NoContent();
    }

    private int UsuarioId()
    {
        return User.UsuarioId()
            ?? throw ErroDeNegocioException.NaoAutorizado("Sessão inválida. Entre novamente.");
    }
}
