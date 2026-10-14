
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using projeto_estoque_web.Data;
using projeto_estoque_web.Models;
using projeto_estoque_web.Models.view_models;

namespace projeto_estoque_web.Controllers
{
    public class EstoqueController : Controller
    {
        private const int ItensPorPagina = 8;
        private readonly AppDbContext _context;

        public EstoqueController(AppDbContext context)
        {
            _context = context;
        }

        // LISTAGEM DO ESTOQUE
        public async Task<IActionResult> Index(
            string busca = "", int pagina = 1)
        {
            var consulta = _context.Produtos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim();

                consulta = consulta.Where(p =>
                    p.Nome.Contains(termo) ||
                    (p.Categoria != null &&
                     p.Categoria.Nome.Contains(termo)));
            }

            var totalProdutos = await _context.Produtos.CountAsync();
            var totalFiltrado = await consulta.CountAsync();

            var totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling(
                    totalFiltrado / (double)ItensPorPagina));

            pagina = Math.Clamp(pagina, 1, totalPaginas);

            var produtos = await consulta
                .OrderBy(p => p.Nome)
                .ThenBy(p => p.Id)
                .Skip((pagina - 1) * ItensPorPagina)
                .Take(ItensPorPagina)
                .ToListAsync();

            var model = new EstoqueViewModel
            {
                TotalProdutos = totalProdutos,
                Produtos = produtos,
                Busca = busca ?? string.Empty,
                PaginaAtual = pagina,
                TotalPaginas = totalPaginas
            };

            return View(model);
        }

        // ENTRADA - FORMULÁRIO
        [HttpGet]
        public async Task<IActionResult> Entrada()
        {
            ViewBag.Produtos = await ListaProdutosAsync();
            return View(new MovimentoEstoqueViewModel());
        }

        // ENTRADA - SALVAR NO BANCO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entrada(
            MovimentoEstoqueViewModel movimento)
        {
            if (movimento.Quantidade <= 0)
            {
                ModelState.AddModelError(
                    nameof(movimento.Quantidade),
                    "Informe uma quantidade maior que zero.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Produtos = await ListaProdutosAsync();
                return View(movimento);
            }

            var produto = await _context.Produtos
                .FindAsync(movimento.ProdutoId);

            if (produto is null)
            {
                ModelState.AddModelError(
                    nameof(movimento.ProdutoId),
                    "Selecione um produto válido.");

                ViewBag.Produtos = await ListaProdutosAsync();
                return View(movimento);
            }

            try
            {
                produto.Quantidade = checked(
                    produto.Quantidade + movimento.Quantidade);
            }
            catch (OverflowException)
            {
                ModelState.AddModelError(
                    nameof(movimento.Quantidade),
                    "A quantidade informada excede o limite permitido.");

                ViewBag.Produtos = await ListaProdutosAsync();
                return View(movimento);
            }

            await _context.SaveChangesAsync();

            TempData["Mensagem"] =
                $"Entrada de {movimento.Quantidade} unidade(s) " +
                $"de \"{produto.Nome}\" registrada com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        // SAÍDA - FORMULÁRIO
        [HttpGet]
        public async Task<IActionResult> Saida()
        {
            ViewBag.Produtos = await ListaProdutosAsync();
            return View(new MovimentoEstoqueViewModel());
        }

        // SAÍDA - ATUALIZAR BANCO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Saida(
            MovimentoEstoqueViewModel movimento)
        {
            if (movimento.Quantidade <= 0)
            {
                ModelState.AddModelError(
                    nameof(movimento.Quantidade),
                    "Informe uma quantidade maior que zero.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Produtos = await ListaProdutosAsync();
                return View(movimento);
            }

            var produto = await _context.Produtos
                .FindAsync(movimento.ProdutoId);

            if (produto is null)
            {
                ModelState.AddModelError(
                    nameof(movimento.ProdutoId),
                    "Selecione um produto válido.");

                ViewBag.Produtos = await ListaProdutosAsync();
                return View(movimento);
            }

            if (movimento.Quantidade > produto.Quantidade)
            {
                ModelState.AddModelError(
                    nameof(movimento.Quantidade),
                    $"Quantidade insuficiente em estoque " +
                    $"({produto.Quantidade} disponíveis).");

                ViewBag.Produtos = await ListaProdutosAsync();
                return View(movimento);
            }

            produto.Quantidade -= movimento.Quantidade;

            await _context.SaveChangesAsync();

            TempData["Mensagem"] =
                $"Saída de {movimento.Quantidade} unidade(s) " +
                $"de \"{produto.Nome}\" registrada com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        // AJUSTE - FORMULÁRIO
        [HttpGet]
        public async Task<IActionResult> Ajuste()
        {
            ViewBag.Produtos = await ListaProdutosAsync();
            return View(new AjusteEstoqueViewModel());
        }

        // AJUSTE - SALVAR NO BANCO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ajuste(
            AjusteEstoqueViewModel movimento)
        {
            if (movimento.NovaQuantidade < 0)
            {
                ModelState.AddModelError(
                    nameof(movimento.NovaQuantidade),
                    "A quantidade não pode ser negativa.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Produtos = await ListaProdutosAsync();
                return View(movimento);
            }

            var produto = await _context.Produtos
                .FindAsync(movimento.ProdutoId);

            if (produto is null)
            {
                ModelState.AddModelError(
                    nameof(movimento.ProdutoId),
                    "Selecione um produto válido.");

                ViewBag.Produtos = await ListaProdutosAsync();
                return View(movimento);
            }

            produto.Quantidade = movimento.NovaQuantidade;

            await _context.SaveChangesAsync();

            TempData["Mensagem"] =
                $"Estoque de \"{produto.Nome}\" ajustado " +
                $"para {movimento.NovaQuantidade} unidade(s).";

            return RedirectToAction(nameof(Index));
        }

        // CARREGAR PRODUTOS PARA OS FORMULÁRIOS
        private async Task<SelectList> ListaProdutosAsync()
        {
            var produtos = await _context.Produtos
                .AsNoTracking()
                .OrderBy(p => p.Nome)
                .Select(p => new
                {
                    p.Id,
                    Descricao = p.Nome +
                        " — Estoque: " + p.Quantidade
                })
                .ToListAsync();

            return new SelectList(
                produtos,
                "Id",
                "Descricao");
        }
    }
}
