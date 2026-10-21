namespace projeto_estoque_web.Models.view_models
{
    public class CategoriasViewModel
    {
        public int TotalCategorias { get; set; }
        public int CategoriasEmUso { get; set; }
        public int CategoriasVazias { get; set; }
        public decimal MediaProdutosPorCategoria { get; set; }

        public List<CategoriaResumoViewModel> Categorias { get; set; } = new();

        public string Busca { get; set; } = string.Empty;
        public int PaginaAtual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
    }

    public class CategoriaResumoViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int QuantidadeProdutos { get; set; }

        public bool EmUso => QuantidadeProdutos > 0;
    }
}