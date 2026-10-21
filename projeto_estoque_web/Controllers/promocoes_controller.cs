using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using projeto_estoque_web.Models;
using projeto_estoque_web.Models.view_models;

namespace projeto_estoque_web.Controllers
{
    public class PromocoesController : Controller
    {
        private const int ItensPorPagina = 8;

        private static readonly List<Promocao> _promocoes = new()
        {
            new Promocao
            {
                Id = 1,
                Nome = "Semana do Café",
                DescontoPercentual = 10m,
                Ativa = true,
                Itens = new List<PromocaoItem>
                {
                    new PromocaoItem { ProdutoId = 5, NomeProduto = "Café torrado 500g", PrecoTabela = 14.90m }
                }
            },
            new Promocao
            {
                Id = 2,
                Nome = "Limpeza da casa",
                DescontoPercentual = 5m,
                Ativa = false,
                Itens = new List<PromocaoItem>
                {
                    new PromocaoItem { ProdutoId = 12, NomeProduto = "Detergente 500ml", PrecoTabela = 3.40m },
                    new PromocaoItem { ProdutoId = 13, NomeProduto = "Álcool 1L", PrecoTabela = 6.90m }
                }
            }
        };

        private static int _proximoId = _promocoes.Count + 1;

        internal static List<Promocao> TodasAsPromocoes => _promocoes;

        internal static Promocao? PromocaoAtivaDoProduto(int produtoId)
            => _promocoes.FirstOrDefault(p => p.Ativa && p.Itens.Any(i => i.ProdutoId == produtoId));

        internal static decimal? PercentualDoProduto(int produtoId)
            => PromocaoAtivaDoProduto(produtoId)?.DescontoPercentual;

        internal static decimal PrecoPromocional(Produto produto)
        {
            var percentual = PercentualDoProduto(produto.Id);

            return percentual is null
                ? produto.Preco
                : Promocao.CalcularPrecoPromocional(produto.Preco, percentual.Value);
        }

        internal static decimal EconomiaDoProduto(Produto produto)
            => Math.Round(
                produto.Preco - PrecoPromocional(produto),
                2,
                MidpointRounding.AwayFromZero);

        internal static bool ProdutoEmPromocaoAtiva(int produtoId, int ignorarPromocaoId = 0)
            => _promocoes.Any(p => p.Id != ignorarPromocaoId &&
                                   p.Ativa &&
                                   p.Itens.Any(i => i.ProdutoId == produtoId));

        public IActionResult Index(string busca, int pagina = 1)
        {
            var lista = _promocoes.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();

                lista = lista.Where(p =>
                    p.Nome.ToLower().Contains(termo) ||
                    p.Status.ToLower().Contains(termo) ||
                    p.Itens.Any(i => i.NomeProduto.ToLower().Contains(termo)));
            }

            var ordenados = lista
                .OrderByDescending(p => p.Ativa)
                .ThenBy(p => p.Nome)
                .ToList();

            var totalPaginas = Math.Max(1, (int)Math.Ceiling(ordenados.Count / (double)ItensPorPagina));
            pagina = Math.Clamp(pagina, 1, totalPaginas);

            var paginaItens = ordenados
                .Skip((pagina - 1) * ItensPorPagina)
                .Take(ItensPorPagina)
                .ToList();

            var ativas = _promocoes.Where(p => p.Ativa).ToList();

            var model = new PromocoesViewModel
            {
                TotalPromocoes = _promocoes.Count,
                PromoçõesAtivas = ativas.Count,
                ProdutosEmPromocao = ativas
                    .SelectMany(p => p.Itens)
                    .Select(i => i.ProdutoId)
                    .Distinct()
                    .Count(),
                DescontoMedio = ativas.Count == 0
                    ? 0m
                    : Math.Round(ativas.Average(p => p.DescontoPercentual), 2, MidpointRounding.AwayFromZero),
                EconomiaPotencial = ativas.Sum(p => p.EconomiaTotal),
                Promocoes = paginaItens,
                Busca = busca ?? string.Empty,
                PaginaAtual = pagina,
                TotalPaginas = totalPaginas
            };

            return View(model);
        }

        public IActionResult Criar()
        {
            PrepararFormulario();

            return View(new Promocao());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Criar(Promocao promocao)
        {
            PrepararFormulario();

            NormalizarItens(promocao);
            LimparErrosDeItens();

            if (!ModelState.IsValid)
                return View(promocao);

            var erro = ValidarPromocao(promocao, 0);

            if (erro is not null)
            {
                ModelState.AddModelError(string.Empty, erro);
                return View(promocao);
            }

            promocao.Id = _proximoId++;
            _promocoes.Add(promocao);

            TempData["Mensagem"] = $"Promoção \"{promocao.Nome}\" cadastrada com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var promocao = _promocoes.FirstOrDefault(p => p.Id == id);

            if (promocao is null)
                return NotFound();

            PrepararFormulario();

            return View(promocao);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Promocao promocao)
        {
            if (id != promocao.Id)
                return NotFound();

            var existente = _promocoes.FirstOrDefault(p => p.Id == id);

            if (existente is null)
                return NotFound();

            PrepararFormulario(id);

            NormalizarItens(promocao);
            LimparErrosDeItens();

            if (!ModelState.IsValid)
                return View(promocao);

            var erro = ValidarPromocao(promocao, id);

            if (erro is not null)
            {
                ModelState.AddModelError(string.Empty, erro);
                return View(promocao);
            }

            existente.Nome = promocao.Nome;
            existente.DescontoPercentual = promocao.DescontoPercentual;
            existente.Ativa = promocao.Ativa;
            existente.Itens = promocao.Itens;

            TempData["Mensagem"] = $"Promoção \"{promocao.Nome}\" atualizada com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AlternarStatus(int id, string? busca, int pagina = 1)
        {
            var promocao = _promocoes.FirstOrDefault(p => p.Id == id);

            if (promocao is null)
                return NotFound();

            promocao.Ativa = !promocao.Ativa;

            TempData["Mensagem"] =
                $"Promoção \"{promocao.Nome}\" {(promocao.Ativa ? "ativada" : "desativada")} com sucesso.";

            return RedirectToAction(nameof(Index), new { busca, pagina });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            var promocao = _promocoes.FirstOrDefault(p => p.Id == id);

            if (promocao is not null)
            {
                _promocoes.Remove(promocao);
                TempData["Mensagem"] = $"Promoção \"{promocao.Nome}\" excluída com sucesso.";
            }

            return RedirectToAction(nameof(Index));
        }

        private static string? ValidarPromocao(Promocao promocao, int ignorarPromocaoId)
        {
            var nomeDuplicado = _promocoes.Any(p =>
                p.Id != ignorarPromocaoId &&
                p.Nome.Trim().Equals(promocao.Nome.Trim(), StringComparison.OrdinalIgnoreCase));

            if (nomeDuplicado)
                return "Já existe uma promoção com este nome.";

            if (promocao.Itens.Count == 0)
                return "Adicione pelo menos um produto à promoção.";

            foreach (var item in promocao.Itens)
            {
                var produto = ProdutosController.TodosOsProdutos.FirstOrDefault(p => p.Id == item.ProdutoId);

                if (produto is null)
                    return "Um dos produtos selecionados não existe mais no cadastro.";

                var emConflito = PromocaoAtivaDoProduto(item.ProdutoId);

                if (emConflito is not null && emConflito.Id != ignorarPromocaoId)
                    return $"\"{produto.Nome}\" já está na promoção \"{emConflito.Nome}\". " +
                           $"Desative ou exclua essa promoção, ou remova o produto.";
            }

            return null;
        }

        private static void NormalizarItens(Promocao promocao)
        {
            promocao.Itens = promocao.Itens
                .Where(i => i.ProdutoId > 0)
                .GroupBy(i => i.ProdutoId)
                .Select(g => g.First())
                .Select(item =>
                {
                    var produto = ProdutosController.TodosOsProdutos
                        .FirstOrDefault(p => p.Id == item.ProdutoId);

                    return new PromocaoItem
                    {
                        ProdutoId = item.ProdutoId,
                        NomeProduto = produto?.Nome ?? $"Produto não encontrado (#{item.ProdutoId})",
                        PrecoTabela = produto?.Preco ?? 0m
                    };
                })
                .OrderBy(i => i.NomeProduto)
                .ToList();
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

        private void PrepararFormulario(int? promocaoId = null)
        {
            var catalogo = ProdutosController.TodosOsProdutos
                .OrderBy(p => p.Nome)
                .Select(p =>
                {
                    var bloqueio = promocaoId is null
                        ? PromocaoAtivaDoProduto(p.Id)
                        : PromocaoAtivaDoProduto(p.Id) is { } promo && promo.Id != promocaoId
                            ? promo
                            : null;

                    return new ProdutoBuscaViewModel
                    {
                        Id = p.Id,
                        Nome = p.Nome,
                        Categoria = p.Categoria,
                        Preco = p.Preco,
                        Bloqueado = bloqueio is not null,
                        BloqueioMotivo = bloqueio?.Nome
                    };
                })
                .ToList();

            ViewBag.Catalogo = JsonSerializer.Serialize(
                catalogo,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}