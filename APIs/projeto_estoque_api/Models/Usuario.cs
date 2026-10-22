using System;
using System.Collections.Generic;

namespace projeto_estoque_api.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string SenhaHash { get; set; } = null!;

    public string? Telefone { get; set; }

    public bool Ativo { get; set; }

    public DateTime DataCadastro { get; set; }

    public virtual ICollection<EnderecoUsuario> EnderecoUsuarios { get; set; } = new List<EnderecoUsuario>();

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
