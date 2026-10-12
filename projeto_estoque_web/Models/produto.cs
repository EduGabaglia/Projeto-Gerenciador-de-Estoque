using System.ComponentModel.DataAnnotations;

namespace projeto_estoque_web.Models
{
    public class Produto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome do produto.")]
        [StringLength(80, ErrorMessage = "O nome deve ter no máximo 80 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione uma categoria.")]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o preço do produto.")]
        [Range(0.01, 1000000, ErrorMessage = "O preço deve ser maior que zero.")]
        [DataType(DataType.Currency)]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "Informe a quantidade em estoque.")]
        [Range(0, 1000000, ErrorMessage = "A quantidade não pode ser negativa.")]
        public int Quantidade { get; set; }

        [Display(Name = "Localização no galpão")]
        [StringLength(100, ErrorMessage = "A localização deve ter no máximo 100 caracteres.")]
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
