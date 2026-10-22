using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using projeto_estoque_web.Models;
using projeto_estoque_web.Models.view_models;
using projeto_estoque_web.Services;

namespace projeto_estoque_web.Controllers
{
    public class EstoqueController : Controller
    {
        private const int ItensPorPagina = 8;

        private readonly EstoqueApiClient _api;

        public EstoqueController(EstoqueApiClient api) => _api = api;

        public async Task<IActionResult> Index(string? busca, int pagina = 1)
        {
            var dados = await _api.ListarEstoqueAsync(busca, pagina, ItensPorPagina);
            var resumoProdutos = await _api.ListarProdutosAsync(null, 1, 1);

            var model = new EstoqueViewModel
            {
                TotalProdutos = dados.Resumo.TotalProdutos,
                EstoqueBaixo = dados.Resumo.EstoqueBaixo,
                CategoriasCadastradas = resumoProdutos.Resumo.CategoriasDistintas,
                ValorTotalEstoque = dados.Resumo.ValorTotalEstoque,
                Produtos = dados.Itens.Select(item => new Produto
                {
                    Id = item.ProdutoId,
                    Nome = item.Nome,
                    CategoriaNome = item.CategoriaNome,
                    Quantidade = item.Quantidade,
                    Localizacao = item.Localizacao ?? string.Empty
                }).ToList(),
                Busca = busca ?? string.Empty,
                PaginaAtual = dados.Pagina,
                TotalPaginas = dados.TotalPaginas
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Entrada()
        {
            await CarregarProdutosAsync();

            return View(new MovimentoEstoqueViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entrada(MovimentoEstoqueViewModel movimento)
        {
            await CarregarProdutosAsync();

            if (!ModelState.IsValid)
                return View(movimento);

            try
            {
                var resultado = await _api.MovimentarEstoqueAsync(
                    movimento.ProdutoId, "ENTRADA", movimento.Quantidade);

                TempData["Mensagem"] = resultado.Mensagem;

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(movimento);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Saida()
        {
            await CarregarProdutosAsync();

            return View(new MovimentoEstoqueViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Saida(MovimentoEstoqueViewModel movimento)
        {
            await CarregarProdutosAsync();

            if (!ModelState.IsValid)
                return View(movimento);

            try
            {
                var resultado = await _api.MovimentarEstoqueAsync(
                    movimento.ProdutoId, "SAIDA", movimento.Quantidade);

                TempData["Mensagem"] = resultado.Mensagem;

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(movimento);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Ajuste()
        {
            await CarregarProdutosAsync();

            return View(new AjusteEstoqueViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ajuste(AjusteEstoqueViewModel movimento)
        {
            await CarregarProdutosAsync();

            if (!ModelState.IsValid)
                return View(movimento);

            try
            {
                var resultado = await _api.MovimentarEstoqueAsync(
                    movimento.ProdutoId, "AJUSTE", movimento.NovaQuantidade);

                TempData["Mensagem"] = resultado.Mensagem;

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(movimento);
            }
        }

        private async Task CarregarProdutosAsync()
        {
            var produtos = await _api.TodosProdutosAsync();

            var itens = produtos
                .OrderBy(p => p.Nome)
                .ToDictionary(
                    p => p.Id.ToString(),
                    p => $"{p.Nome} — Estoque: {p.Quantidade}");

            ViewBag.Produtos = new SelectList(itens, "Key", "Value");
        }
    }
}
