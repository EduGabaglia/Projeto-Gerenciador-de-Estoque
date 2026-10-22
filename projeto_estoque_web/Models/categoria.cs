using System.ComponentModel.DataAnnotations;

namespace projeto_estoque_web.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome da categoria.")]
        [StringLength(80, ErrorMessage = "O nome deve ter no máximo 80 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a descrição da categoria.")]
        [StringLength(255, ErrorMessage = "A descrição deve ter no máximo 255 caracteres.")]
        public string Descricao { get; set; } = string.Empty;
    }
}
