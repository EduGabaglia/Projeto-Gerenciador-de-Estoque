namespace projeto_estoque_web.Models.view_models
{
    public class ProdutosViewModel
    {
        public int TotalProdutos { get; set; }
        public int EstoqueBaixo { get; set; }
        public int CategoriasCadastradas { get; set; }
        public decimal ValorTotalEstoque { get; set; }

        public List<Produto> Produtos { get; set; } = new();

        public string Busca { get; set; } = string.Empty;
        public int PaginaAtual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
    }
}