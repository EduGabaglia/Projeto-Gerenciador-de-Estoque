
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using projeto_estoque_web.Data;
using projeto_estoque_web.Models;
using projeto_estoque_web.Models.view_models;

namespace projeto_estoque_web.Controllers
{
    public class ProdutosController : Controller
    {
        private const int ItensPorPagina = 8;
        private readonly AppDbContext _context;

        public ProdutosController(AppDbContext context)
        {
            _context = context;
        }

        // LISTAR E PESQUISAR PRODUTOS
        public async Task<IActionResult> Index(
            string busca = "", int pagina = 1)
        {
            var consulta = _context.Produtos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.LocalizacaoGalpao)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim();

                consulta = consulta.Where(p =>
                    p.Nome.Contains(termo) ||
                    (p.Categoria != null &&
                     p.Categoria.Nome.Contains(termo)));
            }

            // ESTATÍSTICAS
            var totalProdutos = await _context.Produtos
                .CountAsync();

            var estoqueBaixo = await _context.Produtos
                .CountAsync(p =>
                    p.Quantidade > 0 && p.Quantidade <= 5);

            var categorias = await _context.Categorias
                .CountAsync();

            var valorTotal = await _context.Produtos
                .SumAsync(p => (decimal?)
                    (p.Preco * p.Quantidade)) ?? 0m;

            // PAGINAÇÃO
            var totalItensFiltrados = await consulta.CountAsync();

            var totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling(
                    totalItensFiltrados / (double)ItensPorPagina));

            pagina = Math.Clamp(pagina, 1, totalPaginas);

            var produtos = await consulta
                .OrderBy(p => p.Nome)
                .ThenBy(p => p.Id)
                .Skip((pagina - 1) * ItensPorPagina)
                .Take(ItensPorPagina)
                .ToListAsync();

            var model = new ProdutosViewModel
            {
                TotalProdutos = totalProdutos,
                EstoqueBaixo = estoqueBaixo,
                CategoriasCadastradas = categorias,
                ValorTotalEstoque = valorTotal,
                Produtos = produtos,
                Busca = busca,
                PaginaAtual = pagina,
                TotalPaginas = totalPaginas
            };

            return View(model);
        }

        // ABRIR FORMULÁRIO DE CADASTRO
        [HttpGet]
        public async Task<IActionResult> Criar()
        {
            ViewBag.Categorias = await ListaCategoriasAsync();
            ViewBag.Localizacoes = await ListaLocalizacoesAsync();

            return View(new Produto());
        }

        // CADASTRAR PRODUTO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Produto produto)
        {
            if (!await CategoriaValidaAsync(produto.CategoriaId))
            {
                ModelState.AddModelError(
                    nameof(Produto.CategoriaId),
                    "Selecione uma categoria válida e ativa.");
            }

            // Verificar se a localização existe no banco
            if (produto.LocalizacaoId.HasValue)
            {
                bool localizacaoExiste = await _context.LocalizacoesGalpao
                    .AnyAsync(l => l.Id == produto.LocalizacaoId.Value);

                if (!localizacaoExiste)
                {
                    ModelState.AddModelError(
                        nameof(Produto.LocalizacaoId),
                        "Selecione uma localização válida.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias =
                    await ListaCategoriasAsync(produto.CategoriaId);

                ViewBag.Localizacoes =
                    await ListaLocalizacoesAsync(produto.LocalizacaoId);

                return View(produto);
            }

            produto.Id = 0;
            produto.Ativo = true;
            produto.DataCadastro = DateTime.Now;

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            TempData["Mensagem"] =
                "Produto cadastrado com sucesso.";

            return RedirectToAction(nameof(Index));
        }
        // ABRIR FORMULÁRIO DE EDIÇÃO
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);

            if (produto is null)
                return NotFound();

            ViewBag.Categorias =
                await ListaCategoriasAsync(produto.CategoriaId);

            ViewBag.Localizacoes =
                await ListaLocalizacoesAsync(produto.LocalizacaoId);

            return View(produto);
        }

        // ATUALIZAR PRODUTO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id, Produto produto)
        {
            if (id != produto.Id)
                return NotFound();

            var existente = await _context.Produtos
                .FindAsync(id);

            if (existente is null)
                return NotFound();

            if (!await CategoriaValidaAsync(produto.CategoriaId))
            {
                ModelState.AddModelError(
                    nameof(Produto.CategoriaId),
                    "Selecione uma categoria válida e ativa.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias =
                    await ListaCategoriasAsync(produto.CategoriaId);

                ViewBag.Localizacoes =
                    await ListaLocalizacoesAsync(produto.LocalizacaoId);

                return View(produto);
            }
            if (produto.LocalizacaoId.HasValue)
            {
                bool localizacaoExiste = await _context.LocalizacoesGalpao
                    .AnyAsync(l => l.Id == produto.LocalizacaoId.Value);

                if (!localizacaoExiste)
                {
                    ModelState.AddModelError(
                        nameof(Produto.LocalizacaoId),
                        "Selecione uma localização válida.");
                }
            }

            existente.Nome = produto.Nome;
            existente.CategoriaId = produto.CategoriaId;
            existente.Descricao = produto.Descricao;
            existente.LocalizacaoId = produto.LocalizacaoId;
            existente.Preco = produto.Preco;
            existente.Quantidade = produto.Quantidade;
            existente.Ativo = produto.Ativo;

            await _context.SaveChangesAsync();

            TempData["Mensagem"] =
                "Produto atualizado com sucesso.";

            return RedirectToAction(nameof(Index));
        }
        private async Task<SelectList> ListaLocalizacoesAsync(int? selecionada = null)
        {
            var localizacoes = await _context.LocalizacoesGalpao
                .AsNoTracking()
                .OrderBy(l => l.Localizacao)
                .ToListAsync();

            return new SelectList(
                localizacoes,
                "Id",
                "Localizacao",
                selecionada);
        }

        // EXCLUIR PRODUTO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            var produto = await _context.Produtos
                .FindAsync(id);

            if (produto is null)
                return NotFound();

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();

            TempData["Mensagem"] =
                "Produto excluído com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        // CARREGAR CATEGORIAS DO BANCO
        private async Task<SelectList> ListaCategoriasAsync(
            int? selecionada = null)
        {
            var categorias = await _context.Categorias
                .AsNoTracking()
                .Where(c => c.Ativo || c.Id == selecionada)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            return new SelectList(
                categorias,
                "Id",
                "Nome",
                selecionada);
        }

        // VERIFICAR SE A CATEGORIA EXISTE
        private async Task<bool> CategoriaValidaAsync(int categoriaId)
        {
            return await _context.Categorias
                .AnyAsync(c => c.Id == categoriaId && c.Ativo);
        }
    }
}
