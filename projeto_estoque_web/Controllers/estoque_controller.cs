using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using projeto_estoque_web.Models;
using projeto_estoque_web.Models.view_models;

namespace projeto_estoque_web.Controllers
{
    public class EstoqueController : Controller
    {
        private const int ItensPorPagina = 8;

        public IActionResult Index(string busca, int pagina = 1)
        {
            var lista = ProdutosController.TodosOsProdutos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                lista = lista.Where(p =>
                    p.Nome.ToLower().Contains(termo) ||
                    p.Categoria.ToLower().Contains(termo));
            }

            var ordenados = lista.OrderBy(p => p.Nome).ToList();

            var totalPaginas = Math.Max(1, (int)Math.Ceiling(ordenados.Count / (double)ItensPorPagina));
            pagina = Math.Clamp(pagina, 1, totalPaginas);

            var paginaItens = ordenados
                .Skip((pagina - 1) * ItensPorPagina)
                .Take(ItensPorPagina)
                .ToList();

            var model = new EstoqueViewModel
            {
                TotalProdutos = ProdutosController.TodosOsProdutos.Count,
                EstoqueBaixo = ProdutosController.TodosOsProdutos.Count(p => p.Quantidade <= 5),
                CategoriasCadastradas = ProdutosController.TodosOsProdutos.Select(p => p.Categoria).Distinct().Count(),
                ValorTotalEstoque = ProdutosController.TodosOsProdutos.Sum(p => p.Preco * p.Quantidade),
                Produtos = paginaItens,
                Busca = busca ?? string.Empty,
                PaginaAtual = pagina,
                TotalPaginas = totalPaginas
            };

            return View(model);
        }
        public IActionResult Entrada(MovimentoEstoqueViewModel movimento)

        {

            ViewBag.Produtos = ListaProdutos();



            if (!ModelState.IsValid)

                return View(movimento);



            var produto = BuscarProduto(movimento.ProdutoId);



            if (produto is null)

            {

                ModelState.AddModelError(nameof(MovimentoEstoqueViewModel.ProdutoId), "Selecione um produto.");

                return View(movimento);

            }



            produto.Quantidade += movimento.Quantidade;



            TempData["Mensagem"] = $"Entrada de {movimento.Quantidade} unidade(s) de \"{produto.Nome}\" registrada com sucesso.";



            return RedirectToAction(nameof(Index));

        }



        public IActionResult Saida()

        {

            ViewBag.Produtos = ListaProdutos();

            return View(new MovimentoEstoqueViewModel());

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public IActionResult Saida(MovimentoEstoqueViewModel movimento)

        {

            ViewBag.Produtos = ListaProdutos();



            if (!ModelState.IsValid)

                return View(movimento);



            var produto = BuscarProduto(movimento.ProdutoId);



            if (produto is null)

            {

                ModelState.AddModelError(nameof(MovimentoEstoqueViewModel.ProdutoId), "Selecione um produto.");

                return View(movimento);

            }



            if (movimento.Quantidade > produto.Quantidade)

            {

                ModelState.AddModelError(nameof(MovimentoEstoqueViewModel.Quantidade),

                    $"Quantidade insuficiente em estoque ({produto.Quantidade} disponíveis).");

                return View(movimento);

            }



            produto.Quantidade -= movimento.Quantidade;



            TempData["Mensagem"] = $"Saída de {movimento.Quantidade} unidade(s) de \"{produto.Nome}\" registrada com sucesso.";



            return RedirectToAction(nameof(Index));

        }



        public IActionResult Ajuste()

        {

            ViewBag.Produtos = ListaProdutos();

            return View(new AjusteEstoqueViewModel());

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public IActionResult Ajuste(AjusteEstoqueViewModel movimento)

        {

            ViewBag.Produtos = ListaProdutos();



            if (!ModelState.IsValid)

                return View(movimento);



            var produto = BuscarProduto(movimento.ProdutoId);



            if (produto is null)

            {

                ModelState.AddModelError(nameof(AjusteEstoqueViewModel.ProdutoId), "Selecione um produto.");

                return View(movimento);

            }



            produto.Quantidade = movimento.NovaQuantidade;



            TempData["Mensagem"] = $"Estoque de \"{produto.Nome}\" ajustado para {movimento.NovaQuantidade} unidade(s).";



            return RedirectToAction(nameof(Index));

        }



        private static Produto? BuscarProduto(int id)

            => ProdutosController.TodosOsProdutos.FirstOrDefault(p => p.Id == id);



        private static SelectList ListaProdutos()

        {

            var produtos = ProdutosController.TodosOsProdutos

                .OrderBy(p => p.Nome)

                .ToDictionary(

                    p => p.Id.ToString(),

                    p => $"{p.Nome} — Estoque: {p.Quantidade}");



            return new SelectList(produtos, "Key", "Value");

        }

    }

}



