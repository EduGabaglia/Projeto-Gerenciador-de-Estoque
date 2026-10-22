using Microsoft.AspNetCore.Mvc;
using projeto_estoque_web.Models.view_models;
using projeto_estoque_web.Services;

namespace projeto_estoque_web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly EstoqueApiClient _api;

        public DashboardController(EstoqueApiClient api) => _api = api;

        public async Task<IActionResult> Index()
        {
            var dados = await _api.ObterDashboardAsync();
            var promocoes = await _api.TodasPromocoesAsync();

            var ativas = promocoes.Where(p => p.Ativa).ToList();

            var itensEmPromocao = ativas
                .SelectMany(p => p.Itens.Select(i => new ItemPromocaoViewModel
                {
                    ProdutoId = i.ProdutoId,
                    NomeProduto = i.NomeProduto,
                    NomePromocao = p.Nome,
                    PrecoTabela = i.PrecoTabela,
                    DescontoPercentual = p.DescontoPercentual,
                    PrecoPromocional = i.PrecoPromocional,
                    Economia = i.Economia
                }))
                .OrderByDescending(x => x.Economia)
                .Take(6)
                .ToList();

            var model = new DashboardViewModel
            {
                TotalProdutos = dados.Resumo.TotalProdutos,
                EstoqueBaixo = dados.Resumo.EstoqueBaixo,
                PedidosPendentes = dados.Resumo.PedidosPendentes,
                VendasMes = dados.Resumo.VendasMes,

                UltimosPedidos = dados.UltimosPedidos.Select(pedido => new PedidoResumoViewModel
                {
                    Id = pedido.Id,
                    Cliente = pedido.Cliente,
                    Data = pedido.Data,
                    Valor = pedido.ValorTotal,
                    Status = pedido.StatusLabel,
                    StatusClasse = pedido.StatusClasse,
                    QuantidadeItens = pedido.TotalItens
                }).ToList(),

                ProdutosMaisVendidos = dados.MaisVendidos.Select(produto => new ProdutoMaisVendidoViewModel
                {
                    Nome = produto.NomeProduto,
                    QuantidadeVendida = produto.Quantidade
                }).ToList(),

                VendasUltimos7Dias = dados.VendasUltimos7Dias.Select(venda => new VendaDiaViewModel
                {
                    Data = venda.Data,
                    Valor = venda.Valor
                }).ToList(),

                PromocoesAtivas = ativas.Count,
                ProdutosEmPromocao = ativas
                    .SelectMany(p => p.Itens)
                    .Select(i => i.ProdutoId)
                    .Distinct()
                    .Count(),
                EconomiaPromocional = ativas.Sum(p => p.EconomiaTotal),
                ItensEmPromocao = itensEmPromocao
            };

            return View(model);
        }
    }
}
