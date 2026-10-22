using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using projeto_estoque_web.Models;
using projeto_estoque_web.Models.view_models;
using projeto_estoque_web.Services;

namespace projeto_estoque_web.Controllers
{
    public class ProdutosController : Controller
    {
        private const int ItensPorPagina = 8;

        private readonly EstoqueApiClient _api;

        public ProdutosController(EstoqueApiClient api) => _api = api;

        public async Task<IActionResult> Index(string? busca, int pagina = 1)
        {
            var dados = await _api.ListarProdutosAsync(busca, pagina, ItensPorPagina);

            var model = new ProdutosViewModel
            {
                TotalProdutos = dados.Resumo.TotalProdutos,
                EstoqueBaixo = dados.Resumo.EstoqueBaixo,
                CategoriasCadastradas = dados.Resumo.CategoriasDistintas,
                ValorTotalEstoque = dados.Resumo.ValorTotalEstoque,
                Produtos = dados.Itens.Select(MapearProduto).ToList(),
                Busca = busca ?? string.Empty,
                PaginaAtual = dados.Pagina,
                TotalPaginas = dados.TotalPaginas
            };

            return View(model);
        }

        public async Task<IActionResult> Criar(string? retorno)
        {
            await CarregarListasAsync();

            ViewBag.Retorno = retorno;

            return View(new Produto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Produto produto, string? retorno)
        {
            await CarregarListasAsync(produto.CategoriaId, produto.LocalizacaoId);

            ViewBag.Retorno = retorno;

            if (!ModelState.IsValid)
                return View(produto);

            try
            {
                await _api.CriarProdutoAsync(ParaSalvar(produto));

                TempData["Mensagem"] = "Produto cadastrado com sucesso.";

                return RedirectToOrigin(retorno);
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(produto);
            }
        }

        public async Task<IActionResult> Editar(int id, string? retorno)
        {
            try
            {
                var produto = MapearProduto(await _api.ObterProdutoAsync(id));

                await CarregarListasAsync(produto.CategoriaId, produto.LocalizacaoId);

                ViewBag.Retorno = retorno;

                return View(produto);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Produto produto, string? retorno)
        {
            if (id != produto.Id)
                return NotFound();

            try
            {
                await _api.ObterProdutoAsync(id);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }

            await CarregarListasAsync(produto.CategoriaId, produto.LocalizacaoId);

            ViewBag.Retorno = retorno;

            if (!ModelState.IsValid)
                return View(produto);

            try
            {
                await _api.AtualizarProdutoAsync(id, ParaSalvar(produto));

                TempData["Mensagem"] = "Produto atualizado com sucesso.";

                return RedirectToOrigin(retorno);
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 404)
                    return NotFound();

                ModelState.AddModelError(string.Empty, ex.Message);

                return View(produto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            try
            {
                await _api.ExcluirProdutoAsync(id);

                TempData["Mensagem"] = "Produto excluído com sucesso.";
            }
            catch (ApiException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task CarregarListasAsync(int? categoriaId = null, int? localizacaoId = null)
        {
            var categorias = await _api.ListarOpcoesCategoriaAsync();
            var localizacoes = await _api.ListarLocalizacoesAsync();

            ViewBag.Categorias = new SelectList(categorias, "Id", "Nome", categoriaId);
            ViewBag.Localizacoes = new SelectList(localizacoes, "Id", "Nome", localizacaoId);
        }

        private static Produto MapearProduto(ProdutoApiDto produto) => new()
        {
            Id = produto.Id,
            Nome = produto.Nome,
            CategoriaId = produto.CategoriaId,
            CategoriaNome = produto.CategoriaNome,
            Descricao = produto.Descricao ?? string.Empty,
            Preco = produto.Preco,
            Quantidade = produto.Quantidade,
            LocalizacaoId = produto.LocalizacaoId,
            Localizacao = produto.Localizacao ?? string.Empty
        };

        private static ProdutoSalvarApiDto ParaSalvar(Produto produto) => new()
        {
            Nome = produto.Nome,
            CategoriaId = produto.CategoriaId,
            Descricao = produto.Descricao,
            Preco = produto.Preco,
            Quantidade = produto.Quantidade,
            LocalizacaoId = produto.LocalizacaoId
        };

        private IActionResult RedirectToOrigin(string? retorno)
            => retorno == "estoque"
                ? RedirectToAction(nameof(EstoqueController.Index), "Estoque")
                : RedirectToAction(nameof(Index));
    }
}
