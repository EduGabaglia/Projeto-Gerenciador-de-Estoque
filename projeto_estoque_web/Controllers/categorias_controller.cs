
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projeto_estoque_web.Data;
using projeto_estoque_web.Models;
using projeto_estoque_web.Models.view_models;

namespace projeto_estoque_web.Controllers
{
    public class CategoriasController : Controller
    {
        private const int ItensPorPagina = 8;
        private readonly AppDbContext _context;

      public CategoriasController(AppDbContext context)
{
    _context = context;
}

// TESTAR CONEXÃO COM O SQL SERVER
[HttpGet]
public async Task<IActionResult> TestarConexao()
{
    try
    {
        var conectado = await _context.Database.CanConnectAsync();

        if (!conectado)
        {
            return Content("Não foi possível conectar ao SQL Server.");
        }

        var totalCategorias = await _context.Categorias.CountAsync();
        var totalProdutos = await _context.Produtos.CountAsync();

        return Content(
            $"Conexão realizada com sucesso!\n" +
            $"Categorias cadastradas: {totalCategorias}\n" +
            $"Produtos cadastrados: {totalProdutos}"
        );
    }
    catch (Exception ex)
    {
        return Content($"Erro: {ex.Message}");
    }
}

// LISTAR, PESQUISAR E PAGINAR CATEGORIAS
public async Task<IActionResult> Index(
    string busca = "", int pagina = 1)
        {
            var consulta = _context.Categorias
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim();

                consulta = consulta.Where(c =>
                    c.Nome.Contains(termo) ||
                    (c.Descricao != null &&
                     c.Descricao.Contains(termo)));
            }

            // ESTATÍSTICAS GERAIS
            var totalCategorias = await _context.Categorias
                .CountAsync();

            var categoriasEmUso = await _context.Categorias
                .CountAsync(c => _context.Produtos
                    .Any(p => p.CategoriaId == c.Id));

            var categoriasVazias =
                totalCategorias - categoriasEmUso;

            var totalProdutos = await _context.Produtos
                .CountAsync();

            var mediaProdutos = categoriasEmUso == 0
                ? 0m
                : Math.Round(
                    totalProdutos / (decimal)categoriasEmUso,
                    1);

            // PAGINAÇÃO
            var totalFiltrado = await consulta.CountAsync();

            var totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling(
                    totalFiltrado / (double)ItensPorPagina));

            pagina = Math.Clamp(pagina, 1, totalPaginas);

            // CONSULTA CATEGORIAS E QUANTIDADE DE PRODUTOS
            var categorias = await consulta
                .OrderBy(c => c.Nome)
                .ThenBy(c => c.Id)
                .Skip((pagina - 1) * ItensPorPagina)
                .Take(ItensPorPagina)
                .Select(c => new CategoriaResumoViewModel
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Descricao = c.Descricao ?? string.Empty,
                    QuantidadeProdutos = _context.Produtos
                        .Count(p => p.CategoriaId == c.Id)
                })
                .ToListAsync();

            var model = new CategoriasViewModel
            {
                TotalCategorias = totalCategorias,
                CategoriasEmUso = categoriasEmUso,
                CategoriasVazias = categoriasVazias,
                MediaProdutosPorCategoria = mediaProdutos,
                Categorias = categorias,
                Busca = busca ?? string.Empty,
                PaginaAtual = pagina,
                TotalPaginas = totalPaginas
            };

            return View(model);
        }

        // ABRIR FORMULÁRIO DE CADASTRO
        [HttpGet]
        public IActionResult Criar()
        {
            return View(new Categoria());
        }

        // CADASTRAR CATEGORIA NO SQL SERVER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Categoria categoria)
        {
            categoria.Nome = categoria.Nome?.Trim()
                ?? string.Empty;

            if (await NomeDuplicadoAsync(categoria.Nome, null))
            {
                ModelState.AddModelError(
                    nameof(Categoria.Nome),
                    "Já existe uma categoria com esse nome.");
            }

            if (!ModelState.IsValid)
                return View(categoria);

            categoria.Id = 0;
            categoria.Ativo = true;

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            TempData["Mensagem"] =
                "Categoria cadastrada com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        // ABRIR FORMULÁRIO DE EDIÇÃO
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var categoria = await _context.Categorias
                .FindAsync(id);

            if (categoria is null)
                return NotFound();

            return View(categoria);
        }

        // ATUALIZAR CATEGORIA NO SQL SERVER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id, Categoria categoria)
        {
            if (id != categoria.Id)
                return NotFound();

            var existente = await _context.Categorias
                .FindAsync(id);

            if (existente is null)
                return NotFound();

            categoria.Nome = categoria.Nome?.Trim()
                ?? string.Empty;

            if (await NomeDuplicadoAsync(categoria.Nome, id))
            {
                ModelState.AddModelError(
                    nameof(Categoria.Nome),
                    "Já existe uma categoria com esse nome.");
            }

            if (!ModelState.IsValid)
                return View(categoria);

            existente.Nome = categoria.Nome;
            existente.Descricao = categoria.Descricao;

            // Preserva o campo Ativo existente.
            // O formulário atual não permite editá-lo.

            await _context.SaveChangesAsync();

            TempData["Mensagem"] =
                "Categoria atualizada com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        // EXCLUIR CATEGORIA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            var categoria = await _context.Categorias
                .FindAsync(id);

            if (categoria is null)
                return NotFound();

            // VERIFICAR SE EXISTEM PRODUTOS VINCULADOS
            var quantidadeProdutos = await _context.Produtos
                .CountAsync(p => p.CategoriaId == id);

            if (quantidadeProdutos > 0)
            {
                TempData["Erro"] =
                    $"Não é possível excluir a categoria " +
                    $"\"{categoria.Nome}\" pois ela possui " +
                    $"{quantidadeProdutos} produto(s).";

                return RedirectToAction(nameof(Index));
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            TempData["Mensagem"] =
                "Categoria excluída com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        // VERIFICAR SE O NOME JÁ EXISTE
        private async Task<bool> NomeDuplicadoAsync(
            string? nome, int? ignorarId)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return false;

            var nomeNormalizado = nome.Trim().ToLower();

            return await _context.Categorias
                .AnyAsync(c =>
                    (!ignorarId.HasValue ||
                     c.Id != ignorarId.Value) &&
                    c.Nome.ToLower() == nomeNormalizado);
        }
    }
}
