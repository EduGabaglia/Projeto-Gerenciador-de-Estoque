using Microsoft.AspNetCore.Mvc;
using projeto_estoque_web.Models;
using projeto_estoque_web.Models.view_models;

namespace projeto_estoque_web.Controllers
{
    public class ConfiguracoesController : Controller
    {
        private static DadosEmpresa _dados = new()
        {
            Id = 1,
            Nome = "Minha Empresa",
            Cnpj = "00.000.000/0000-00",
            Endereco = "Rua Exemplo, 123",
            Cidade = "São Paulo",
            Uf = "SP",
            Telefone = "(11) 0000-0000",
            Email = "contato@empresa.com.br"
        };

        private static bool _dadosDeExemplo = true;

        public IActionResult Index()
            => View(ViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ConfiguracoesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Resumo = _dados;
                model.DadosDeExemplo = _dadosDeExemplo;

                return View(model);
            }

            _dados.Nome = model.Dados.Nome.Trim();
            _dados.Cnpj = model.Dados.Cnpj?.Trim();
            _dados.Endereco = model.Dados.Endereco.Trim();
            _dados.Cidade = model.Dados.Cidade.Trim();
            _dados.Uf = model.Dados.Uf?.Trim().ToUpper();
            _dados.Telefone = model.Dados.Telefone.Trim();
            _dados.Email = model.Dados.Email.Trim();

            _dadosDeExemplo = false;

            TempData["Mensagem"] = "Dados da empresa atualizados com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        private static ConfiguracoesViewModel ViewModel()
            => new()
            {
                Dados = _dadosDeExemplo ? new DadosEmpresa() : _dados,
                Resumo = _dados,
                DadosDeExemplo = _dadosDeExemplo
            };
    }
}
