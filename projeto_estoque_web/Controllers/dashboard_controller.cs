using Microsoft.AspNetCore.Mvc;
using projeto_estoque_web.Models.view_models;

namespace projeto_estoque_web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var produtos = ProdutosController.TodosOsProdutos;
            var pedidos = PedidosController.TodosOsPedidos;

            var agora = DateTime.Now;

            var maisVendidos = pedidos
                .Where(p => p.Status != "Cancelado")
                .SelectMany(p => p.Itens)
                .GroupBy(i => i.NomeProduto)
                .Select(g => new ProdutoMaisVendidoViewModel
                {
                    Nome = g.Key,
                    QuantidadeVendida = g.Sum(i => i.Quantidade)
                })
                .OrderByDescending(x => x.QuantidadeVendida)
                .Take(4)
                .ToList();

            var ativas = PromocoesController.TodasAsPromocoes
                .Where(p => p.Ativa)
                .ToList();

            var itensEmPromocao = ativas
                .SelectMany(p => p.Itens.Select(i => new ItemPromocaoViewModel
                {
                    ProdutoId = i.ProdutoId,
                    NomeProduto = i.NomeProduto,
                    NomePromocao = p.Nome,
                    PrecoTabela = i.PrecoTabela,
                    DescontoPercentual = p.DescontoPercentual,
                    PrecoPromocional = i.PrecoPromocional(p.DescontoPercentual),
                    Economia = i.Economia(p.DescontoPercentual)
                }))
                .OrderByDescending(x => x.Economia)
                .Take(6)
                .ToList();

            var model = new DashboardViewModel
            {
                TotalProdutos = produtos.Count,
                EstoqueBaixo = produtos.Count(p => p.Quantidade <= 5),
                PedidosPendentes = pedidos.Count(p => p.Status == "Pendente"),
                VendasMes = pedidos
                    .Where(p => p.Status != "Cancelado" &&
                                p.Data.Year == agora.Year &&
                                p.Data.Month == agora.Month)
                    .Sum(p => p.ValorTotal),

                UltimosPedidos = pedidos
                    .OrderByDescending(p => p.Data)
                    .ThenByDescending(p => p.Id)
                    .Take(3)
                    .Select(p => new PedidoResumoViewModel
                    {
                        Id = p.Id,
                        Data = p.Data,
                        Valor = p.ValorTotal,
                        Status = p.Status
                    })
                    .ToList(),

                ProdutosMaisVendidos = maisVendidos,

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