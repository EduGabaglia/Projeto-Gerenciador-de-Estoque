using Microsoft.AspNetCore.Mvc;
using projeto_estoque_web.Models;
using projeto_estoque_web.Models.view_models;

namespace projeto_estoque_web.Controllers
{
    public class CategoriasController : Controller
    {
        private const int ItensPorPagina = 8;

        private static readonly List<Categoria> _categorias = new()
        {
            new Categoria { Id = 1, Nome = "Alimentos", Descricao = "Produtos alimentícios em geral" },
            new Categoria { Id = 2, Nome = "Bebidas", Descricao = "Sucos, refrigerantes, água e lácteos" },
            new Categoria { Id = 3, Nome = "Higiene", Descricao = "Produtos de cuidado pessoal" },
            new Categoria { Id = 4, Nome = "Limpeza", Descricao = "Produtos de limpeza doméstica" },
            new Categoria { Id = 5, Nome = "Padaria", Descricao = "Pães, bolos e confeitaria" }
        };

        private static int _proximoId = _categorias.Count + 1;

        internal static List<Categoria> TodasAsCategorias => _categorias;

        public IActionResult Index(string busca, int pagina = 1)
        {
            var lista = _categorias.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                lista = lista.Where(c =>
                    c.Nome.ToLower().Contains(termo) ||
                    c.Descricao.ToLower().Contains(termo));
            }

            var ordenadas = lista.OrderBy(c => c.Nome).ToList();

            var totalPaginas = Math.Max(1, (int)Math.Ceiling(ordenadas.Count / (double)ItensPorPagina));
            pagina = Math.Clamp(pagina, 1, totalPaginas);

            var paginaItens = ordenadas
                .Skip((pagina - 1) * ItensPorPagina)
                .Take(ItensPorPagina)
                .ToList();

            var quantidades = ProdutosController.TodosOsProdutos
                .GroupBy(p => p.Categoria)
                .ToDictionary(g => g.Key, g => g.Count());

            var categorias = paginaItens.Select(c => new CategoriaResumoViewModel
            {
                Id = c.Id,
                Nome = c.Nome,
                Descricao = c.Descricao,
                QuantidadeProdutos = quantidades.GetValueOrDefault(c.Nome)
            }).ToList();

            var emUso = _categorias.Count(c => quantidades.ContainsKey(c.Nome));

            var model = new CategoriasViewModel
            {
                TotalCategorias = _categorias.Count,
                CategoriasEmUso = emUso,
                CategoriasVazias = _categorias.Count - emUso,
                MediaProdutosPorCategoria = emUso == 0
                    ? 0
                    : Math.Round(ProdutosController.TodosOsProdutos.Count / (decimal)emUso, 1),
                Categorias = categorias,
                Busca = busca ?? string.Empty,
                PaginaAtual = pagina,
                TotalPaginas = totalPaginas
            };

            return View(model);
        }

        public IActionResult Criar()
        {
            return View(new Categoria());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Criar(Categoria categoria)
        {
            if (NomeDuplicado(categoria.Nome, null))
                ModelState.AddModelError(nameof(Categoria.Nome), "Já existe uma categoria com esse nome.");

            if (!ModelState.IsValid)
                return View(categoria);

            categoria.Id = _proximoId++;
            _categorias.Add(categoria);

            TempData["Mensagem"] = "Categoria cadastrada com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var categoria = _categorias.FirstOrDefault(c => c.Id == id);

            if (categoria is null)
                return NotFound();

            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Categoria categoria)
        {
            if (id != categoria.Id)
                return NotFound();

            var existente = _categorias.FirstOrDefault(c => c.Id == id);

            if (existente is null)
                return NotFound();

            if (NomeDuplicado(categoria.Nome, id))
                ModelState.AddModelError(nameof(Categoria.Nome), "Já existe uma categoria com esse nome.");

            if (!ModelState.IsValid)
                return View(categoria);

            var nomeAntigo = existente.Nome;

            existente.Nome = categoria.Nome;
            existente.Descricao = categoria.Descricao;

            if (!nomeAntigo.Equals(categoria.Nome, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var produto in ProdutosController.TodosOsProdutos.Where(p => p.Categoria == nomeAntigo))
                    produto.Categoria = categoria.Nome;
            }

            TempData["Mensagem"] = "Categoria atualizada com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            var categoria = _categorias.FirstOrDefault(c => c.Id == id);

            if (categoria is not null)
            {
                var quantidade = ProdutosController.TodosOsProdutos.Count(p => p.Categoria == categoria.Nome);

                if (quantidade > 0)
                {
                    TempData["Erro"] = $"Não é possível excluir a categoria \"{categoria.Nome}\" pois ela possui {quantidade} produto(s).";
                    return RedirectToAction(nameof(Index));
                }

                _categorias.Remove(categoria);
                TempData["Mensagem"] = "Categoria excluída com sucesso.";
            }

            return RedirectToAction(nameof(Index));
        }

        private static bool NomeDuplicado(string? nome, int? ignorarId)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return false;

            return _categorias.Any(c =>
                c.Id != ignorarId &&
                c.Nome.Equals(nome.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}