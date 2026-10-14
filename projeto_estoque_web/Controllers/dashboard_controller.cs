#if false
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projeto_estoque_web.Data;
using projeto_estoque_web.Models.view_models;

namespace projeto_estoque_web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // PRODUTOS DO SQL SERVER
            var totalProdutos = await _context.Produtos
                .CountAsync();

            var estoqueBaixo = await _context.Produtos
                .CountAsync(p => p.Quantidade <= 5);

            // PEDIDOS AINDA EM MEMÓRIA
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

            var model = new DashboardViewModel
            {
                TotalProdutos = totalProdutos,
                EstoqueBaixo = estoqueBaixo,

                PedidosPendentes = pedidos.Count(
                    p => p.Status == "Pendente"),

                VendasMes = pedidos
                    .Where(p =>
                        p.Status != "Cancelado" &&
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

                ProdutosMaisVendidos = maisVendidos
            };

            return View(model);
        }
    }
}
#endif