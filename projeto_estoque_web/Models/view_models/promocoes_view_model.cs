namespace projeto_estoque_web.Models.view_models
{
    public class PromocoesViewModel
    {
        public int TotalPromocoes { get; set; }
        public int PromoçõesAtivas { get; set; }
        public int ProdutosEmPromocao { get; set; }
        public decimal DescontoMedio { get; set; }
        public decimal EconomiaPotencial { get; set; }

        public List<Promocao> Promocoes { get; set; } = new();

        public string Busca { get; set; } = string.Empty;
        public int PaginaAtual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
    }

    public class PromocaoProdutoViewModel
    {
        public int ProdutoId { get; set; }
        public string NomeProduto { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal PrecoTabela { get; set; }
        public decimal DescontoPercentual { get; set; }

        public decimal PrecoPromocional
            => Promocao.CalcularPrecoPromocional(PrecoTabela, DescontoPercentual);

        public decimal Economia => PrecoTabela - PrecoPromocional;
    }

    public class ProdutoBuscaViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public bool Bloqueado { get; set; }
        public string? BloqueioMotivo { get; set; }
    }
}