using System.ComponentModel.DataAnnotations;

namespace projeto_estoque_web.Models
{
    public class Promocao
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome da promoção.")]
        [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o percentual de desconto.")]
        [Range(0.01, 99.99, ErrorMessage = "O desconto deve estar entre 0,01 e 99,99 %.")]
        [Display(Name = "Desconto")]
        public decimal DescontoPercentual { get; set; }

        [Display(Name = "Promoção ativa")]
        public bool Ativa { get; set; } = true;

        public List<PromocaoItem> Itens { get; set; } = new();

        public int TotalProdutos => Itens.Count;

        public decimal EconomiaTotal => Itens.Sum(i => i.Economia(DescontoPercentual));

        public decimal PrecoPromocionalTotal => Itens.Sum(i => i.PrecoPromocional(DescontoPercentual));

        public string Status => Ativa ? "Ativa" : "Inativa";

        public string StatusClasse => Ativa ? "ativa" : "inativa";

        public static decimal CalcularPrecoPromocional(decimal precoTabela, decimal descontoPercentual)
        {
            if (precoTabela <= 0)
                return 0;

            return Math.Round(
                precoTabela * (1 - descontoPercentual / 100m),
                2,
                MidpointRounding.AwayFromZero);
        }
    }

    public class PromocaoItem
    {
        public int ProdutoId { get; set; }

        public string NomeProduto { get; set; } = string.Empty;

        public decimal PrecoTabela { get; set; }

        public decimal PrecoPromocional(decimal descontoPercentual)
            => Promocao.CalcularPrecoPromocional(PrecoTabela, descontoPercentual);

        public decimal Economia(decimal descontoPercentual)
            => Math.Round(PrecoTabela - PrecoPromocional(descontoPercentual), 2, MidpointRounding.AwayFromZero);
    }
}