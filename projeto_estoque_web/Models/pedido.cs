using System.ComponentModel.DataAnnotations;

namespace projeto_estoque_web.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome do cliente.")]
        [StringLength(80, ErrorMessage = "O nome deve ter no máximo 80 caracteres.")]
        public string Cliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a data do pedido.")]
        [Display(Name = "Data")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Pendente";

        public List<PedidoItem> Itens { get; set; } = new();

        public decimal ValorTotal => Itens.Sum(i => i.Quantidade * i.PrecoUnitario);

        public int TotalItens => Itens.Count;
    }

    public class PedidoItem
    {
        public int ProdutoId { get; set; }
        public string NomeProduto { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }
}