namespace projeto_estoque_api.Models;

public partial class Categoria
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string? Descricao { get; set; }

    public bool Ativo { get; set; }

    public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
