using System.ComponentModel.DataAnnotations;

namespace projeto_estoque_web.Models.view_models
{
    public class EstoqueViewModel
    {
        public int TotalProdutos { get; set; }
        public int EstoqueBaixo { get; set; }
        public int CategoriasCadastradas { get; set; }
        public decimal ValorTotalEstoque { get; set; }

        public List<Models.Produto> Produtos { get; set; } = new();

        public string Busca { get; set; } = string.Empty;
        public int PaginaAtual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
    }

    public class MovimentoEstoqueViewModel
    {
        [Display(Name = "Produto")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecione um produto.")]
        public int ProdutoId { get; set; }

        [Display(Name = "Quantidade")]
        [Required(ErrorMessage = "Informe a quantidade.")]
        [Range(1, 1000000, ErrorMessage = "A quantidade deve ser maior que zero.")]
        public int Quantidade { get; set; }
    }

    public class AjusteEstoqueViewModel
    {
        [Display(Name = "Produto")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecione um produto.")]
        public int ProdutoId { get; set; }

        [Display(Name = "Nova quantidade")]
        [Required(ErrorMessage = "Informe a nova quantidade.")]
        [Range(0, 1000000, ErrorMessage = "A quantidade não pode ser negativa.")]
        public int NovaQuantidade { get; set; }
    }
}
