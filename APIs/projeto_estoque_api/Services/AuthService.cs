using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using projeto_estoque_api.Common;
using projeto_estoque_api.Data;
using projeto_estoque_api.Dtos;
using projeto_estoque_api.Models;

namespace projeto_estoque_api.Services;

public class AuthService
{
    private readonly EstoqueDbContext _db;
    private readonly JwtService _jwt;

    public AuthService(EstoqueDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<AuthRespostaDto> CadastrarAsync(CadastroDto dados)
    {
        var nome = ValidarNome(dados.Nome);
        var email = ValidarEmail(dados.Email);
        var senha = ValidarNovaSenha(dados.Senha);
        var telefone = NormalizarTelefone(dados.Telefone);

        if (await _db.Usuarios.AnyAsync(u => u.Email.ToLower() == email))
            throw ErroDeNegocioException.Conflito("Já existe uma conta com este e-mail.");

        var usuario = new Usuario
        {
            Nome = nome,
            Email = email,
            SenhaHash = SenhaHasher.Criar(senha),
            Telefone = telefone,
            Ativo = true,
            DataCadastro = DateTime.Now
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return Resposta(usuario);
    }

    public async Task<AuthRespostaDto> LoginAsync(LoginDto dados)
    {
        var email = (dados.Email ?? string.Empty).Trim().ToLowerInvariant();
        var senha = dados.Senha ?? string.Empty;

        if (email.Length == 0 || senha.Length == 0)
            throw ErroDeNegocioException.NaoAutorizado("E-mail ou senha inválidos.");

        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (usuario is null || !usuario.Ativo || !SenhaHasher.Verificar(senha, usuario.SenhaHash))
            throw ErroDeNegocioException.NaoAutorizado("E-mail ou senha inválidos.");

        return Resposta(usuario);
    }

    public async Task<UsuarioDto> ObterAsync(int usuarioId)
    {
        var usuario = await ObterAtivoAsync(usuarioId);

        return ParaDto(usuario);
    }

    public async Task<UsuarioDto> AtualizarPerfilAsync(int usuarioId, PerfilAtualizarDto dados)
    {
        var usuario = await ObterAtivoAsync(usuarioId);

        usuario.Nome = ValidarNome(dados.Nome);

        if (dados.Telefone is not null)
            usuario.Telefone = NormalizarTelefone(dados.Telefone);

        await _db.SaveChangesAsync();

        return ParaDto(usuario);
    }

    public async Task AlterarSenhaAsync(int usuarioId, SenhaAlterarDto dados)
    {
        var usuario = await ObterAtivoAsync(usuarioId);
        var atual = dados.SenhaAtual ?? string.Empty;
        var nova = ValidarNovaSenha(dados.NovaSenha);

        if (!SenhaHasher.Verificar(atual, usuario.SenhaHash))
            throw new ErroDeNegocioException("A senha atual está incorreta.");

        if (SenhaHasher.Verificar(nova, usuario.SenhaHash))
            throw new ErroDeNegocioException("A nova senha precisa ser diferente da atual.");

        usuario.SenhaHash = SenhaHasher.Criar(nova);
        await _db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int usuarioId)
    {
        var usuario = await ObterAtivoAsync(usuarioId);

        // Exclusão lógica: preserva o histórico de pedidos vinculado à conta.
        usuario.Ativo = false;
        usuario.SenhaHash = "CONTA_EXCLUIDA";
        usuario.Telefone = null;

        await _db.SaveChangesAsync();
    }

    private async Task<Usuario> ObterAtivoAsync(int usuarioId)
    {
        return await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Ativo)
            ?? throw ErroDeNegocioException.NaoAutorizado("Sessão inválida ou expirada. Entre novamente.");
    }

    private AuthRespostaDto Resposta(Usuario usuario)
    {
        return new AuthRespostaDto(_jwt.Gerar(usuario), ParaDto(usuario));
    }

    private static UsuarioDto ParaDto(Usuario usuario)
    {
        return new UsuarioDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Telefone);
    }

    private static string ValidarNome(string? valor)
    {
        var nome = (valor ?? string.Empty).Trim();

        if (nome.Length == 0)
            throw new ErroDeNegocioException("Informe seu nome.");

        if (nome.Length > 120)
            throw new ErroDeNegocioException("O nome deve ter no máximo 120 caracteres.");

        return nome;
    }

    private static string ValidarEmail(string? valor)
    {
        var email = (valor ?? string.Empty).Trim().ToLowerInvariant();

        if (email.Length == 0)
            throw new ErroDeNegocioException("Informe o e-mail.");

        if (email.Length > 254 || !MailAddress.TryCreate(email, out var endereco) || endereco.Address != email)
            throw new ErroDeNegocioException("Informe um e-mail válido.");

        return email;
    }

    private static string ValidarNovaSenha(string? valor)
    {
        var senha = valor ?? string.Empty;

        if (senha.Length < 6)
            throw new ErroDeNegocioException("A senha precisa ter pelo menos 6 caracteres.");

        if (senha.Length > 100)
            throw new ErroDeNegocioException("A senha deve ter no máximo 100 caracteres.");

        return senha;
    }

    private static string? NormalizarTelefone(string? valor)
    {
        var telefone = (valor ?? string.Empty).Trim();

        if (telefone.Length == 0)
            return null;

        if (telefone.Length > 15)
            throw new ErroDeNegocioException("O telefone deve ter no máximo 15 caracteres.");

        return telefone;
    }
}
