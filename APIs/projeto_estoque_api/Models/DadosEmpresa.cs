using System;
using System.Collections.Generic;

namespace projeto_estoque_api.Models;

public partial class DadosEmpresa
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string? NomeFantasia { get; set; }

    public string Cnpj { get; set; } = null!;

    public string Cep { get; set; } = null!;

    public string Endereco { get; set; } = null!;

    public string? Numero { get; set; }

    public string? Complemento { get; set; }

    public string? Bairro { get; set; }

    public string Cidade { get; set; } = null!;

    public string Uf { get; set; } = null!;

    public string? Telefone { get; set; }

    public string? Email { get; set; }

    public bool Ativo { get; set; }
}
