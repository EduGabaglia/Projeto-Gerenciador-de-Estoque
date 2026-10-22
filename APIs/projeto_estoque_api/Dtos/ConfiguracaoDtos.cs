namespace projeto_estoque_api.Dtos;

/// <summary>
/// Dados da empresa (cadastro único). "Preenchido" indica se já existe
/// registro no banco; enquanto false, o formulário abre vazio no sistema.
/// </summary>
public record EmpresaDto(
    int Id,
    bool Preenchido,
    string Nome,
    string? NomeFantasia,
    string Cnpj,
    string Cep,
    string Endereco,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string Cidade,
    string Uf,
    string? Telefone,
    string? Email);

/// <summary>Dados aceitos ao salvar os dados da empresa.</summary>
public class EmpresaSalvarDto
{
    public string? Nome { get; set; }

    public string? Cnpj { get; set; }

    public string? Cep { get; set; }

    public string? Endereco { get; set; }

    public string? Cidade { get; set; }

    public string? Uf { get; set; }

    public string? Telefone { get; set; }

    public string? Email { get; set; }
}
