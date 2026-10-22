using Microsoft.AspNetCore.Mvc;
using projeto_estoque_web.Models;
using projeto_estoque_web.Models.view_models;
using projeto_estoque_web.Services;

namespace projeto_estoque_web.Controllers
{
    public class CategoriasController : Controller
    {
        private const int ItensPorPagina = 8;

        private readonly EstoqueApiClient _api;

        public CategoriasController(EstoqueApiClient api) => _api = api;

        public async Task<IActionResult> Index(string? busca, int pagina = 1)
        {
            var dados = await _api.ListarCategoriasAsync(busca, pagina, ItensPorPagina);

            var model = new CategoriasViewModel
            {
                TotalCategorias = dados.Resumo.TotalCategorias,
                CategoriasEmUso = dados.Resumo.EmUso,
                CategoriasVazias = dados.Resumo.Vazias,
                MediaProdutosPorCategoria = dados.Resumo.MediaProdutosPorCategoriaEmUso,
                Categorias = dados.Itens.Select(categoria => new CategoriaResumoViewModel
                {
                    Id = categoria.Id,
                    Nome = categoria.Nome,
                    Descricao = categoria.Descricao,
                    QuantidadeProdutos = categoria.ProdutosVinculados
                }).ToList(),
                Busca = busca ?? string.Empty,
                PaginaAtual = dados.Pagina,
                TotalPaginas = dados.TotalPaginas
            };

            return View(model);
        }

        public IActionResult Criar()
        {
            return View(new Categoria());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Categoria categoria)
        {
            if (!ModelState.IsValid)
                return View(categoria);

            try
            {
                await _api.CriarCategoriaAsync(ParaSalvar(categoria));

                TempData["Mensagem"] = "Categoria cadastrada com sucesso.";

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                AdicionarErro(ex);

                return View(categoria);
            }
        }

        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                var categoria = await _api.ObterCategoriaAsync(id);

                return View(new Categoria
                {
                    Id = categoria.Id,
                    Nome = categoria.Nome,
                    Descricao = categoria.Descricao
                });
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Categoria categoria)
        {
            if (id != categoria.Id)
                return NotFound();

            try
            {
                await _api.ObterCategoriaAsync(id);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
                return View(categoria);

            try
            {
                await _api.AtualizarCategoriaAsync(id, ParaSalvar(categoria));

                TempData["Mensagem"] = "Categoria atualizada com sucesso.";

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 404)
                    return NotFound();

                AdicionarErro(ex);

                return View(categoria);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            try
            {
                await _api.ExcluirCategoriaAsync(id);

                TempData["Mensagem"] = "Categoria excluída com sucesso.";
            }
            catch (ApiException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private static CategoriaSalvarApiDto ParaSalvar(Categoria categoria) => new()
        {
            Nome = categoria.Nome,
            Descricao = categoria.Descricao
        };

        private void AdicionarErro(ApiException ex)
        {
            if (ex.StatusCode == 409)
                ModelState.AddModelError(nameof(Categoria.Nome), ex.Message);
            else
                ModelState.AddModelError(string.Empty, ex.Message);
        }
    }
}
