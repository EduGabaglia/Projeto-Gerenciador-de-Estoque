using System.ComponentModel.DataAnnotations;

namespace projeto_estoque_web.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        public ICollection<Produto> Produtos { get; set; }
            = new List<Produto>();
    }
}