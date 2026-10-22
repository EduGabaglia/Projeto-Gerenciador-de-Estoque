using Microsoft.EntityFrameworkCore;
using projeto_estoque_api.Common;
using projeto_estoque_api.Data;
using projeto_estoque_api.Dtos;

namespace projeto_estoque_api.Services;

public class DashboardService
{
    private readonly EstoqueDbContext _db;

    public DashboardService(EstoqueDbContext db) => _db = db;

    public async Task<DashboardDto> ObterAsync()
    {
        var agora = DateTime.Now;
        var inicioMes = new DateTime(agora.Year, agora.Month, 1);

        var resumo = new DashboardResumoDto(
            await _db.Produtos.CountAsync(p => p.Ativo),
            await _db.Produtos.CountAsync(p => p.Ativo && p.Quantidade <= 5),
            await _db.Pedidos.CountAsync(p => p.Status == "PENDENTE"),
            await _db.Pedidos
                .Where(p => p.Status != "CANCELADO" && p.Data >= inicioMes)
                .SumAsync(p => (decimal?)p.ValorTotal) ?? 0m);

        var ultimosPedidos = await _db.Pedidos
            .AsNoTracking()
            .Include(p => p.Usuario)
            .Include(p => p.PedidoItems)
            .OrderByDescending(p => p.Data)
            .ThenByDescending(p => p.Id)
            .Take(3)
            .ToListAsync();

        // Mais vendidos do mês corrente (RN-45 corrigido: o rótulo da tela é "Este mês").
        var maisVendidos = await _db.PedidoItems
            .AsNoTracking()
            .Where(i => i.Pedido.Status != "CANCELADO" && i.Pedido.Data >= inicioMes)
            .GroupBy(i => i.Produto.Nome)
            .Select(g => new { Nome = g.Key, Quantidade = g.Sum(i => i.Quantidade) })
            .OrderByDescending(p => p.Quantidade)
            .Take(4)
            .ToListAsync();

        var inicioJanela = DateTime.Today.AddDays(-6);

        var vendas = await _db.Pedidos
            .AsNoTracking()
            .Where(p => p.Status != "CANCELADO" && p.Data >= inicioJanela)
            .Select(p => new { p.Data, p.ValorTotal })
            .ToListAsync();

        var vendasUltimos7Dias = Enumerable.Range(0, 7)
            .Select(dia =>
            {
                var data = inicioJanela.AddDays(dia);

                return new VendaDiaDto(data, vendas
                    .Where(v => v.Data.Date == data)
                    .Sum(v => v.ValorTotal));
            })
            .ToList();

        return new DashboardDto(
            resumo,
            ultimosPedidos.Select(p => new UltimoPedidoDto(
                p.Id,
                p.Usuario?.Nome ?? string.Empty,
                p.Data,
                p.Status,
                StatusPedido.Rotulo(p.Status),
                StatusPedido.Classe(p.Status),
                p.PedidoItems.Count,
                p.ValorTotal)).ToList(),
            maisVendidos.Select(p => new ProdutoMaisVendidoDto(p.Nome, p.Quantidade)).ToList(),
            vendasUltimos7Dias);
    }
}
