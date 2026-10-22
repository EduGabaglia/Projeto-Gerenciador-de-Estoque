namespace projeto_estoque_web.Models.view_models
{
    public class DashboardViewModel
    {
        public int TotalProdutos { get; set; }
        public int EstoqueBaixo { get; set; }
        public int PedidosPendentes { get; set; }
        public decimal VendasMes { get; set; }

        public List<PedidoResumoViewModel> UltimosPedidos { get; set; } = new();
        public List<ProdutoMaisVendidoViewModel> ProdutosMaisVendidos { get; set; } = new();
        public List<VendaDiaViewModel> VendasUltimos7Dias { get; set; } = new();

        public int PromocoesAtivas { get; set; }
        public int ProdutosEmPromocao { get; set; }
        public decimal EconomiaPromocional { get; set; }
        public List<ItemPromocaoViewModel> ItensEmPromocao { get; set; } = new();
    }

    public class ItemPromocaoViewModel
    {
        public int ProdutoId { get; set; }
        public string NomeProduto { get; set; } = string.Empty;
        public string NomePromocao { get; set; } = string.Empty;
        public decimal PrecoTabela { get; set; }
        public decimal DescontoPercentual { get; set; }
        public decimal PrecoPromocional { get; set; }
        public decimal Economia { get; set; }
    }

    public class PedidoResumoViewModel
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusClasse { get; set; } = "pendente";
        public string Cliente { get; set; } = string.Empty;
        public int QuantidadeItens { get; set; }
    }

    public class ProdutoMaisVendidoViewModel
    {
        public string Nome { get; set; } = string.Empty;
        public int QuantidadeVendida { get; set; }
    }

    public class VendaDiaViewModel
    {
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }
    }
}