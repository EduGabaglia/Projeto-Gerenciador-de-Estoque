using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using projeto_estoque_web.Models;
using projeto_estoque_web.Models.view_models;
using projeto_estoque_web.Services;

namespace projeto_estoque_web.Controllers
{
    public class PromocoesController : Controller
    {
        private const int ItensPorPagina = 8;

        private readonly EstoqueApiClient _api;

        public PromocoesController(EstoqueApiClient api) => _api = api;

        public async Task<IActionResult> Index(string? busca, int pagina = 1)
        {
            var dados = await _api.ListarPromocoesAsync(busca, pagina, ItensPorPagina);

            var model = new PromocoesViewModel
            {
                TotalPromocoes = dados.Resumo.TotalPromocoes,
                PromoçõesAtivas = dados.Resumo.PromocoesAtivas,
                ProdutosEmPromocao = dados.Resumo.ProdutosEmPromocao,
                DescontoMedio = dados.Resumo.DescontoMedio,
                EconomiaPotencial = dados.Resumo.EconomiaPotencial,
                Promocoes = dados.Itens.Select(MapearPromocao).ToList(),
                Busca = busca ?? string.Empty,
                PaginaAtual = dados.Pagina,
                TotalPaginas = dados.TotalPaginas
            };

            return View(model);
        }

        public async Task<IActionResult> Criar()
        {
            await PrepararCatalogoAsync();

            return View(new Promocao());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Promocao promocao)
        {
            var produtos = await PrepararCatalogoAsync();

            NormalizarItens(promocao, produtos);
            LimparErrosDeItens();

            if (!ModelState.IsValid)
                return View(promocao);

            try
            {
                var criada = await _api.CriarPromocaoAsync(ParaSalvar(promocao));

                TempData["Mensagem"] = $"Promoção \"{criada.Nome}\" cadastrada com sucesso.";

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(promocao);
            }
        }

        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                var promocao = MapearPromocao(await _api.ObterPromocaoAsync(id));

                await PrepararCatalogoAsync(id);

                return View(promocao);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Promocao promocao)
        {
            if (id != promocao.Id)
                return NotFound();

            try
            {
                await _api.ObterPromocaoAsync(id);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return NotFound();
            }

            var produtos = await PrepararCatalogoAsync(id);

            NormalizarItens(promocao, produtos);
            LimparErrosDeItens();

            if (!ModelState.IsValid)
                return View(promocao);

            try
            {
                await _api.AtualizarPromocaoAsync(id, ParaSalvar(promocao));

                TempData["Mensagem"] = $"Promoção \"{promocao.Nome}\" atualizada com sucesso.";

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 404)
                    return NotFound();

                ModelState.AddModelError(string.Empty, ex.Message);

                return View(promocao);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarStatus(int id, string? busca, int pagina = 1)
        {
            try
            {
                var promocao = await _api.AlternarStatusPromocaoAsync(id);

                TempData["Mensagem"] =
                    $"Promoção \"{promocao.Nome}\" {(promocao.Ativa ? "ativada" : "desativada")} com sucesso.";
            }
            catch (ApiException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { busca, pagina });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            try
            {
                var promocao = await _api.ObterPromocaoAsync(id);

                await _api.ExcluirPromocaoAsync(id);

                TempData["Mensagem"] = $"Promoção \"{promocao.Nome}\" excluída com sucesso.";
            }
            catch (ApiException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Carrega o catálogo de produtos para o seletor e marca os que já estão
        /// em outra promoção ativa (o servidor valida novamente ao salvar).
        /// </summary>
        private async Task<List<ProdutoApiDto>> PrepararCatalogoAsync(int? promocaoId = null)
        {
            var produtos = await _api.TodosProdutosAsync();
            var promocoes = await _api.TodasPromocoesAsync();

            var bloqueios = promocoes
                .Where(p => p.Ativa && p.Id != (promocaoId ?? 0))
                .SelectMany(p => p.Itens.Select(i => new { i.ProdutoId, Promocao = p.Nome }))
                .GroupBy(x => x.ProdutoId)
                .ToDictionary(g => g.Key, g => g.First().Promocao);

            var catalogo = produtos
                .OrderBy(p => p.Nome)
                .Select(p =>
                {
                    var bloqueado = bloqueios.TryGetValue(p.Id, out var motivo);

                    return new ProdutoBuscaViewModel
                    {
                        Id = p.Id,
                        Nome = p.Nome,
                        Categoria = p.CategoriaNome,
                        Preco = p.Preco,
                        Bloqueado = bloqueado,
                        BloqueioMotivo = motivo
                    };
                })
                .ToList();

            ViewBag.Catalogo = JsonSerializer.Serialize(
                catalogo,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return produtos;
        }

        private static Promocao MapearPromocao(PromocaoApiDto promocao) => new()
        {
            Id = promocao.Id,
            Nome = promocao.Nome,
            DescontoPercentual = promocao.DescontoPercentual,
            Ativa = promocao.Ativa,
            Itens = promocao.Itens.Select(item => new PromocaoItem
            {
                ProdutoId = item.ProdutoId,
                NomeProduto = item.NomeProduto,
                PrecoTabela = item.PrecoTabela
            }).ToList()
        };

        private static PromocaoSalvarApiDto ParaSalvar(Promocao promocao) => new()
        {
            Nome = promocao.Nome,
            DescontoPercentual = promocao.DescontoPercentual,
            Ativa = promocao.Ativa,
            ProdutosIds = promocao.Itens.Select(item => item.ProdutoId).ToList()
        };

        private static void NormalizarItens(Promocao promocao, List<ProdutoApiDto> produtos)
        {
            promocao.Itens = promocao.Itens
                .Where(i => i.ProdutoId > 0)
                .GroupBy(i => i.ProdutoId)
                .Select(g => g.First())
                .Select(item =>
                {
                    var produto = produtos.FirstOrDefault(p => p.Id == item.ProdutoId);

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
    }
}
