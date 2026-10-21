using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using projeto_estoque_web.Models;
using projeto_estoque_web.Models.view_models;

namespace projeto_estoque_web.Controllers
{
    public class PedidosController : Controller
    {
        private const int ItensPorPagina = 8;

        public static readonly string[] StatusDisponiveis =
        {
            "Pendente", "Preparando", "Entregue", "Cancelado"
        };

        private static readonly List<Pedido> _pedidos = new()
        {
            new Pedido
            {
                Id = 1006,
                Cliente = "João Souza",
                Data = DateTime.Now,
                Status = "Pendente",
                Itens = new List<PedidoItem>
                {
                    new PedidoItem { ProdutoId = 1, NomeProduto = "Arroz 5kg", Quantidade = 2, PrecoUnitario = 24.90m },
                    new PedidoItem { ProdutoId = 6, NomeProduto = "Leite integral 1L", Quantidade = 2, PrecoUnitario = 4.50m },
                    new PedidoItem { ProdutoId = 10, NomeProduto = "Sabonete", Quantidade = 3, PrecoUnitario = 2.90m }
                }
            },
            new Pedido
            {
                Id = 1005,
                Cliente = "Maria Silva",
                Data = DateTime.Now.AddDays(-1),
                Status = "Preparando",
                Itens = new List<PedidoItem>
                {
                    new PedidoItem { ProdutoId = 5, NomeProduto = "Café torrado 500g", Quantidade = 1, PrecoUnitario = 14.90m },
                    new PedidoItem { ProdutoId = 4, NomeProduto = "Açúcar 1kg", Quantidade = 2, PrecoUnitario = 5.20m }
                }
            },
            new Pedido
            {
                Id = 1004,
                Cliente = "Ana Costa",
                Data = DateTime.Now.AddDays(-2),
                Status = "Entregue",
                Itens = new List<PedidoItem>
                {
                    new PedidoItem { ProdutoId = 12, NomeProduto = "Detergente 500ml", Quantidade = 2, PrecoUnitario = 3.40m },
                    new PedidoItem { ProdutoId = 13, NomeProduto = "Álcool 1L", Quantidade = 1, PrecoUnitario = 6.90m }
                }
            },
            new Pedido
            {
                Id = 1003,
                Cliente = "Pedro Lima",
                Data = DateTime.Now.AddDays(-3),
                Status = "Cancelado",
                Itens = new List<PedidoItem>
                {
                    new PedidoItem { ProdutoId = 8, NomeProduto = "Refrigerante 2L", Quantidade = 4, PrecoUnitario = 9.90m }
                }
            },
            new Pedido
            {
                Id = 1002,
                Cliente = "Fernanda Dias",
                Data = DateTime.Now.AddDays(-4),
                Status = "Entregue",
                Itens = new List<PedidoItem>
                {
                    new PedidoItem { ProdutoId = 11, NomeProduto = "Shampoo 400ml", Quantidade = 1, PrecoUnitario = 18.90m },
                    new PedidoItem { ProdutoId = 14, NomeProduto = "Pão de forma", Quantidade = 2, PrecoUnitario = 8.90m }
                }
            },
            new Pedido
            {
                Id = 1001,
                Cliente = "Loja Central",
                Data = DateTime.Now.AddDays(-5),
                Status = "Entregue",
                Itens = new List<PedidoItem>
                {
                    new PedidoItem { ProdutoId = 3, NomeProduto = "Óleo de cozinha 900ml", Quantidade = 2, PrecoUnitario = 8.50m },
                    new PedidoItem { ProdutoId = 9, NomeProduto = "Água mineral 500ml", Quantidade = 6, PrecoUnitario = 1.80m }
                }
            }
        };

        private static int _proximoId = 1007;

        internal static List<Pedido> TodosOsPedidos => _pedidos;

        public IActionResult Index(string busca, int pagina = 1)
        {
            ViewBag.StatusDisponiveis = StatusDisponiveis;

            var lista = _pedidos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                var codigo = int.TryParse(termo, out var c) ? c : 0;

                lista = lista.Where(p =>
                    p.Cliente.ToLower().Contains(termo) ||
                    p.Id == codigo);
            }

            var ordenados = lista
                .OrderByDescending(p => p.Data)
                .ThenByDescending(p => p.Id)
                .ToList();

            var totalPaginas = Math.Max(1, (int)Math.Ceiling(ordenados.Count / (double)ItensPorPagina));
            pagina = Math.Clamp(pagina, 1, totalPaginas);

            var paginaItens = ordenados
                .Skip((pagina - 1) * ItensPorPagina)
                .Take(ItensPorPagina)
                .ToList();

            var resumo = paginaItens.Select(p => new PedidoResumoViewModel
            {
                Id = p.Id,
                Data = p.Data,
                Valor = p.ValorTotal,
                Status = p.Status,
                Cliente = p.Cliente,
                QuantidadeItens = p.TotalItens
            }).ToList();

            var ativos = _pedidos.Where(p => p.Status != "Cancelado").ToList();

            var model = new PedidosViewModel
            {
                TotalPedidos = _pedidos.Count,
                PedidosPendentes = _pedidos.Count(p => p.Status == "Pendente"),
                PedidosEntregues = _pedidos.Count(p => p.Status == "Entregue"),
                ReceitaTotal = ativos.Sum(p => p.ValorTotal),
                Pedidos = resumo,
                Busca = busca ?? string.Empty,
                PaginaAtual = pagina,
                TotalPaginas = totalPaginas
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AtualizarStatus(int id, string? status, string? busca, int pagina = 1)
        {
            if (!StatusDisponiveis.Contains(status, StringComparer.OrdinalIgnoreCase))
            {
                TempData["Erro"] = "Status inválido.";
                return RedirectToAction(nameof(Index), new { busca, pagina });
            }

            var pedido = _pedidos.FirstOrDefault(p => p.Id == id);

            if (pedido is null)
            {
                TempData["Erro"] = "Pedido não encontrado.";
                return RedirectToAction(nameof(Index), new { busca, pagina });
            }

            var novoStatus = StatusDisponiveis.First(s => s.Equals(status, StringComparison.OrdinalIgnoreCase));

            if (pedido.Status == novoStatus)
                return RedirectToAction(nameof(Index), new { busca, pagina });

            if (novoStatus == "Cancelado")
            {
                if (pedido.Status != "Cancelado")
                {
                    foreach (var item in pedido.Itens)
                    {
                        var produto = ProdutosController.TodosOsProdutos.FirstOrDefault(p => p.Id == item.ProdutoId);

                        if (produto is not null)
                            produto.Quantidade += item.Quantidade;
                    }
                }
            }
            else if (pedido.Status == "Cancelado")
            {
                foreach (var item in pedido.Itens)
                {
                    var produto = ProdutosController.TodosOsProdutos.FirstOrDefault(p => p.Id == item.ProdutoId);

                    if (produto is not null && item.Quantidade > produto.Quantidade)
                    {
                        TempData["Erro"] = $"Estoque insuficiente para \"{item.NomeProduto}\" ({produto.Quantidade} disponíveis). Status não alterado.";
                        return RedirectToAction(nameof(Index), new { busca, pagina });
                    }
                }

                foreach (var item in pedido.Itens)
                {
                    var produto = ProdutosController.TodosOsProdutos.First(p => p.Id == item.ProdutoId);
                    produto.Quantidade -= item.Quantidade;
                }
            }

            pedido.Status = novoStatus;

            TempData["Mensagem"] = $"Pedido #{pedido.Id} atualizado para \"{novoStatus}\".";

            return RedirectToAction(nameof(Index), new { busca, pagina });
        }

        public IActionResult Criar()
        {
            ViewBag.Produtos = ListaProdutos();
            ViewBag.StatusList = ListaStatus("Pendente");

            return View(new Pedido { Data = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Criar(Pedido pedido)
        {
            ViewBag.Produtos = ListaProdutos();
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

            var quantidadePorProduto = pedido.Itens.ToDictionary(i => i.ProdutoId, i => i.Quantidade);

            if (pedido.Status != "Cancelado")
            {
                foreach (var par in quantidadePorProduto)
                {
                    var produto = ProdutosController.TodosOsProdutos.FirstOrDefault(p => p.Id == par.Key);

                    if (produto is null)
                    {
                        ModelState.AddModelError(string.Empty, "Um dos itens refere-se a um produto inexistente.");
                        return View(pedido);
                    }

                    if (par.Value > produto.Quantidade)
                    {
                        ModelState.AddModelError(string.Empty, $"Estoque insuficiente para \"{produto.Nome}\" ({produto.Quantidade} disponíveis).");
                        return View(pedido);
                    }
                }
            }

            foreach (var item in pedido.Itens)
            {
                var produto = ProdutosController.TodosOsProdutos.FirstOrDefault(p => p.Id == item.ProdutoId);

                if (produto is not null)
                {
                    item.NomeProduto = produto.Nome;
                    item.PrecoUnitario = produto.Preco;

                    if (pedido.Status != "Cancelado")
                        produto.Quantidade -= item.Quantidade;
                }
            }

            pedido.Id = _proximoId++;
            _pedidos.Insert(0, pedido);

            TempData["Mensagem"] = $"Pedido #{pedido.Id} cadastrado com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var pedido = _pedidos.FirstOrDefault(p => p.Id == id);

            if (pedido is null)
                return NotFound();

            ViewBag.Produtos = ListaProdutos();
            ViewBag.StatusList = ListaStatus(pedido.Status);

            return View(pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Pedido pedido)
        {
            if (id != pedido.Id)
                return NotFound();

            var existente = _pedidos.FirstOrDefault(p => p.Id == id);

            if (existente is null)
                return NotFound();

            ViewBag.Produtos = ListaProdutos();
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

            var novoCancelado = pedido.Status == "Cancelado";

            if (!novoCancelado)
            {
                var quantidadePorProduto = pedido.Itens.ToDictionary(i => i.ProdutoId, i => i.Quantidade);

                foreach (var par in quantidadePorProduto)
                {
                    var produto = ProdutosController.TodosOsProdutos.FirstOrDefault(p => p.Id == par.Key);

                    if (produto is null)
                    {
                        ModelState.AddModelError(string.Empty, "Um dos itens refere-se a um produto inexistente.");
                        return View(pedido);
                    }

                    var disponivel = produto.Quantidade;

                    if (existente.Status != "Cancelado")
                    {
                        disponivel += existente.Itens
                            .Where(i => i.ProdutoId == par.Key)
                            .Sum(i => i.Quantidade);
                    }

                    if (par.Value > disponivel)
                    {
                        ModelState.AddModelError(string.Empty, $"Estoque insuficiente para \"{produto.Nome}\" ({disponivel} disponíveis).");
                        return View(pedido);
                    }
                }
            }

            if (existente.Status != "Cancelado")
            {
                foreach (var item in existente.Itens)
                {
                    var produto = ProdutosController.TodosOsProdutos.FirstOrDefault(p => p.Id == item.ProdutoId);

                    if (produto is not null)
                        produto.Quantidade += item.Quantidade;
                }
            }

            if (!novoCancelado)
            {
                foreach (var item in pedido.Itens)
                {
                    var produto = ProdutosController.TodosOsProdutos.First(p => p.Id == item.ProdutoId);
                    item.NomeProduto = produto.Nome;
                    item.PrecoUnitario = produto.Preco;
                    produto.Quantidade -= item.Quantidade;
                }
            }
            else
            {
                foreach (var item in pedido.Itens)
                {
                    var produto = ProdutosController.TodosOsProdutos.FirstOrDefault(p => p.Id == item.ProdutoId);

                    if (produto is not null)
                    {
                        item.NomeProduto = produto.Nome;
                        item.PrecoUnitario = produto.Preco;
                    }
                }
            }

            existente.Cliente = pedido.Cliente;
            existente.Data = pedido.Data;
            existente.Status = pedido.Status;
            existente.Itens = pedido.Itens;

            TempData["Mensagem"] = $"Pedido #{existente.Id} atualizado com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            var pedido = _pedidos.FirstOrDefault(p => p.Id == id);

            if (pedido is not null)
            {
                if (pedido.Status != "Cancelado")
                {
                    foreach (var item in pedido.Itens)
                    {
                        var produto = ProdutosController.TodosOsProdutos.FirstOrDefault(p => p.Id == item.ProdutoId);

                        if (produto is not null)
                            produto.Quantidade += item.Quantidade;
                    }
                }

                _pedidos.Remove(pedido);
                TempData["Mensagem"] = $"Pedido #{pedido.Id} excluído com sucesso.";
            }

            return RedirectToAction(nameof(Index));
        }

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

        private static SelectList ListaProdutos()
        {
            var produtos = ProdutosController.TodosOsProdutos
                .OrderBy(p => p.Nome)
                .ToDictionary(
                    p => p.Id.ToString(),
                    p => $"{p.Nome} — {p.Preco.ToString("C")}");

            return new SelectList(produtos, "Key", "Value");
        }

        private static SelectList ListaStatus(string selecionado)
            => new SelectList(StatusDisponiveis, selecionado);
    }
}