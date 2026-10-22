using System;
using System.Collections.Generic;

namespace projeto_estoque_api.Models;

public partial class LocalizacaoGalpao
{
    public int Id { get; set; }

    public string Localizacao { get; set; } = null!;

    public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
