using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using projeto_estoque_web.Models;
using projeto_estoque_web.Models.view_models;
using projeto_estoque_web.Services;

namespace projeto_estoque_web.Controllers
{
    public class PedidosController : Controller
    {
        private const int ItensPorPagina = 8;

        public static readonly string[] StatusDisponiveis =
        {
            "Pendente", "Pago", "Preparando", "Enviado", "Entregue", "Cancelado"
        };

        private readonly EstoqueApiClient _api;

        public PedidosController(EstoqueApiClient api) => _api = api;

        public async Task<IActionResult> Index(string? busca, int pagina = 1)
        {
            ViewBag.StatusDisponiveis = StatusDisponiveis;

            var dados = await _api.ListarPedidosAsync(busca, pagina, ItensPorPagina);

            var model = new PedidosViewModel
            {
                TotalPedidos = dados.Resumo.TotalPedidos,
                PedidosPendentes = dados.Resumo.Pendentes,
                PedidosEntregues = dados.Resumo.Entregues,
                ReceitaTotal = dados.Resumo.ReceitaTotal,
                Pedidos = dados.Itens.Select(pedido => new PedidoResumoViewModel
                {
                    Id = pedido.Id,
                    Data = pedido.Data,
                    Valor = pedido.ValorTotal,
                    Status = pedido.StatusLabel,
                    StatusClasse = pedido.StatusClasse,
                    Cliente = pedido.Cliente,
                    QuantidadeItens = pedido.TotalItens
                }).ToList(),
                Busca = busca ?? string.Empty,
                PaginaAtual = dados.Pagina,
                TotalPaginas = dados.TotalPaginas
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarStatus(int id, string? status, string? busca, int pagina = 1)
        {
            if (!StatusDisponiveis.Contains(status, StringComparer.OrdinalIgnoreCase))
            {
                TempData["Erro"] = "Status inválido.";
                return RedirectToAction(nameof(Index), new { busca, pagina });
            }

            var novoStatus = StatusDisponiveis.First(
                s => s.Equals(status, StringComparison.OrdinalIgnoreCase));

            try
            {
                var pedido = await _api.AlterarStatusPedidoAsync(id, novoStatus);

                TempData["Mensagem"] = $"Pedido #{pedido.Id} atualizado para \"{pedido.StatusLabel}\".";
            }
            catch (ApiException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { busca, pagina });
        }

        public async Task<IActionResult> Criar()
        {
            await CarregarListasAsync();

            ViewBag.StatusList = ListaStatus("Pendente");

            return View(new Pedido { Data = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Pedido pedido)
        {
            await CarregarListasAsync();

            ViewBag.StatusList = ListaStatus(pedido.Status);

            NormalizarStatus(pedido);
            NormalizarItens(pedido);
            LimparErrosDeItens();

            if (!ModelState.IsValid)
                return View(pedido);

            if (pedido.Itens.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Adicione pelo menos um item ao pedido.");
                return View(pedido);
            }

            try
            {
                var criado = await _api.CriarPedidoAsync(ParaSalvar(pedido));

                TempData["Mensagem"] = $"Pedido #{criado.Id} cadastrado com sucesso.";

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(pedido);
            }
        }

        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                var pedido = MapearPedido(await _api.ObterPedidoAsync(id));

                await CarregarListasAsync();

                ViewBag.StatusList = ListaStatus(pedido.Status);

                return View(pedido);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Pedido pedido)
        {
            if (id != pedido.Id)
                return NotFound();

            try
            {
                await _api.ObterPedidoAsync(id);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }

            await CarregarListasAsync();

            ViewBag.StatusList = ListaStatus(pedido.Status);

            NormalizarStatus(pedido);
            NormalizarItens(pedido);
            LimparErrosDeItens();

            if (!ModelState.IsValid)
                return View(pedido);

            if (pedido.Itens.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Adicione pelo menos um item ao pedido.");
                return View(pedido);
            }

            try
            {
                await _api.AtualizarPedidoAsync(id, ParaSalvar(pedido));

                TempData["Mensagem"] = $"Pedido #{id} atualizado com sucesso.";

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 404)
                    return NotFound();

                ModelState.AddModelError(string.Empty, ex.Message);

                return View(pedido);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            try
            {
                await _api.ExcluirPedidoAsync(id);

                TempData["Mensagem"] = $"Pedido #{id} excluído com sucesso.";
            }
            catch (ApiException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task CarregarListasAsync()
        {
            var produtos = await _api.TodosProdutosAsync();

            ViewBag.Produtos = produtos
                .OrderBy(p => p.Nome)
                .Select(p => new SelectListItem($"{p.Nome} — {p.Preco.ToString("C")}", p.Id.ToString()))
                .ToList();
        }

        private static SelectList ListaStatus(string? selecionado)
            => new(StatusDisponiveis, selecionado);

        private static Pedido MapearPedido(PedidoApiDto pedido) => new()
        {
            Id = pedido.Id,
            Cliente = pedido.Cliente,
            Data = pedido.Data,
            Status = pedido.StatusLabel,
            Itens = pedido.Itens.Select(item => new PedidoItem
            {
                ProdutoId = item.ProdutoId,
                NomeProduto = item.NomeProduto,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario
            }).ToList()
        };

        private static PedidoSalvarApiDto ParaSalvar(Pedido pedido) => new()
        {
            Cliente = pedido.Cliente,
            Data = pedido.Data,
            Status = pedido.Status,
            Itens = pedido.Itens.Select(item => new ItemPedidoSalvarApiDto
            {
                ProdutoId = item.ProdutoId,
                Quantidade = item.Quantidade
            }).ToList()
        };

        private static void NormalizarItens(Pedido pedido)
        {
            pedido.Itens = pedido.Itens
                .Where(i => i.ProdutoId > 0 && i.Quantidade > 0)
                .GroupBy(i => i.ProdutoId)
                .Select(g => new PedidoItem
                {
                    ProdutoId = g.Key,
                    Quantidade = g.Sum(i => i.Quantidade)
                })
                .ToList();
        }

        private static void NormalizarStatus(Pedido pedido)
        {
            pedido.Status = StatusDisponiveis.FirstOrDefault(
                s => s.Equals(pedido.Status, StringComparison.OrdinalIgnoreCase)) ?? "Pendente";
        }

        private void LimparErrosDeItens()
        {
            foreach (var key in ModelState.Keys
                .Where(k => k.StartsWith("Itens", StringComparison.OrdinalIgnoreCase))
                .ToList())
            {
                ModelState.Remove(key);
            }
        }
    }
}
