using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projeto_estoque_web.Models
{
    public class Produto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome do produto.")]
        [StringLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione uma categoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecione uma categoria.")]
        public int CategoriaId { get; set; }

        public int? LocalizacaoId { get; set; }

        public LocalizacaoGalpao? LocalizacaoGalpao { get; set; }

        public Categoria? Categoria { get; set; }

        [StringLength(500)]
        public string? Descricao { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Preco { get; set; }

        [Range(0, 1000000)]
        public int Quantidade { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [NotMapped]
        public string Status => Quantidade switch
        {
            <= 0 => "Esgotado",
            <= 5 => "Estoque baixo",
            _ => "Disponível"
        };

        [NotMapped]
        public string StatusClasse => Quantidade switch
        {
            <= 0 => "esgotado",
            <= 5 => "baixo",
            _ => "disponivel"
        };
    }
}