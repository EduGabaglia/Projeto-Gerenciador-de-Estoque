namespace projeto_estoque_api.Dtos;

public record UsuarioDto(int Id, string Nome, string Email, string? Telefone);

public record AuthRespostaDto(string Token, UsuarioDto Usuario);

/// <summary>Dados aceitos no cadastro do app mobile.</summary>
public class CadastroDto
{
    public string? Nome { get; set; }

    public string? Email { get; set; }

    public string? Senha { get; set; }

    public string? Telefone { get; set; }
}

/// <summary>Dados aceitos no login do app mobile.</summary>
public class LoginDto
{
    public string? Email { get; set; }

    public string? Senha { get; set; }
}

/// <summary>Dados aceitos na edição do perfil do app mobile.</summary>
public class PerfilAtualizarDto
{
    public string? Nome { get; set; }

    public string? Telefone { get; set; }
}

/// <summary>Dados aceitos na troca de senha do app mobile.</summary>
public class SenhaAlterarDto
{
    public string? SenhaAtual { get; set; }

    public string? NovaSenha { get; set; }
}
