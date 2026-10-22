using Microsoft.AspNetCore.Mvc;
using projeto_estoque_web.Models;
using projeto_estoque_web.Models.view_models;
using projeto_estoque_web.Services;

namespace projeto_estoque_web.Controllers
{
    public class ConfiguracoesController : Controller
    {
        private readonly EstoqueApiClient _api;

        public ConfiguracoesController(EstoqueApiClient api) => _api = api;

        public async Task<IActionResult> Index()
            => View(await ViewModelAsync());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ConfiguracoesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PreencherResumoAsync(model);

                return View(model);
            }

            try
            {
                await _api.SalvarEmpresaAsync(new EmpresaSalvarApiDto
                {
                    Nome = model.Dados.Nome,
                    Cnpj = model.Dados.Cnpj,
                    Cep = model.Dados.Cep,
                    Endereco = model.Dados.Endereco,
                    Cidade = model.Dados.Cidade,
                    Uf = model.Dados.Uf,
                    Telefone = model.Dados.Telefone,
                    Email = model.Dados.Email
                });

                TempData["Mensagem"] = "Dados da empresa atualizados com sucesso.";

                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                await PreencherResumoAsync(model);

                ModelState.AddModelError(string.Empty, ex.Message);

                return View(model);
            }
        }

        private async Task<ConfiguracoesViewModel> ViewModelAsync()
        {
            var empresa = await _api.ObterEmpresaAsync();
            var dados = MapearEmpresa(empresa);

            return new ConfiguracoesViewModel
            {
                Dados = empresa.Preenchido ? dados : new DadosEmpresa(),
                Resumo = dados,
                DadosDeExemplo = !empresa.Preenchido
            };
        }

        private async Task PreencherResumoAsync(ConfiguracoesViewModel model)
        {
            var empresa = await _api.ObterEmpresaAsync();

            model.Resumo = MapearEmpresa(empresa);
            model.DadosDeExemplo = !empresa.Preenchido;
        }

        private static DadosEmpresa MapearEmpresa(EmpresaApiDto empresa) => new()
        {
            Id = empresa.Id,
            Nome = empresa.Nome,
            Cnpj = FormatarCnpj(empresa.Cnpj),
            Cep = FormatarCep(empresa.Cep),
            Endereco = empresa.Endereco,
            Cidade = empresa.Cidade,
            Uf = empresa.Uf,
            Telefone = empresa.Telefone ?? string.Empty,
            Email = empresa.Email ?? string.Empty
        };

        private static string FormatarCnpj(string? valor)
        {
            var digitos = new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());

            return digitos.Length == 14
                ? $"{digitos[..2]}.{digitos[2..5]}.{digitos[5..8]}/{digitos[8..12]}-{digitos[12..]}"
                : valor ?? string.Empty;
        }

        private static string FormatarCep(string? valor)
        {
            var digitos = new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());

            return digitos.Length == 8
                ? $"{digitos[..5]}-{digitos[5..]}"
                : valor ?? string.Empty;
        }
    }
}
