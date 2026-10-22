using System.ComponentModel.DataAnnotations;

namespace projeto_estoque_web.Models
{
    public class Produto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome do produto.")]
        [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "Categoria")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecione uma categoria.")]
        public int CategoriaId { get; set; }

        /// <summary>Nome da categoria retornado pela API (somente exibição).</summary>
        public string CategoriaNome { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "Informe o preço do produto.")]
        [Range(0.01, 1000000, ErrorMessage = "O preço deve ser maior que zero.")]
        [DataType(DataType.Currency)]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "Informe a quantidade em estoque.")]
        [Range(0, 1000000, ErrorMessage = "A quantidade não pode ser negativa.")]
        public int Quantidade { get; set; }

        [Display(Name = "Localização no galpão")]
        public int? LocalizacaoId { get; set; }

        /// <summary>Descrição da localização retornada pela API (somente exibição).</summary>
        public string Localizacao { get; set; } = string.Empty;

        public string Status => Quantidade switch
        {
            <= 0 => "Esgotado",
            <= 5 => "Estoque baixo",
            _ => "Disponível"
        };

        public string StatusClasse => Quantidade switch
        {
            <= 0 => "esgotado",
            <= 5 => "baixo",
            _ => "disponivel"
        };
    }
}
