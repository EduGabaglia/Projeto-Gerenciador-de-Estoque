namespace projeto_estoque_web.Models.view_models
{
    public class PedidosViewModel
    {
        public int TotalPedidos { get; set; }
        public int PedidosPendentes { get; set; }
        public int PedidosEntregues { get; set; }
        public decimal ReceitaTotal { get; set; }

        public List<PedidoResumoViewModel> Pedidos { get; set; } = new();

        public string Busca { get; set; } = string.Empty;
        public int PaginaAtual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
    }
}