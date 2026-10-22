using System;
using System.Collections.Generic;

namespace projeto_estoque_api.Models;

public partial class Promocao
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public decimal DescontoPercentual { get; set; }

    public bool Ativa { get; set; }

    public DateTime DataCadastro { get; set; }

    public virtual ICollection<PromocaoItem> PromocaoItems { get; set; } = new List<PromocaoItem>();
}
