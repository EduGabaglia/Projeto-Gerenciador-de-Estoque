using System.ComponentModel.DataAnnotations;

namespace projeto_estoque_web.Models
{
    public class LocalizacaoGalpao
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe a localização.")]
        [StringLength(35)]
        public string Localizacao { get; set; } = string.Empty;

        public ICollection<Produto> Produtos { get; set; }
            = new List<Produto>();
    }
}