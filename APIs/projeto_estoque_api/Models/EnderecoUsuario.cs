using System;
using System.Collections.Generic;

namespace projeto_estoque_api.Models;

public partial class EnderecoUsuario
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string? Apelido { get; set; }

    public string Cep { get; set; } = null!;

    public string Logradouro { get; set; } = null!;

    public string Numero { get; set; } = null!;

    public string? Complemento { get; set; }

    public string Bairro { get; set; } = null!;

    public string Cidade { get; set; } = null!;

    public string Uf { get; set; } = null!;

    public bool Principal { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
