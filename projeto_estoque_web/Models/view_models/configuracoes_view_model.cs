using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace projeto_estoque_web.Models.view_models
{
    public class ConfiguracoesViewModel
    {
        public DadosEmpresa Dados { get; set; } = new();

        [ValidateNever]
        public DadosEmpresa Resumo { get; set; } = new();

        public bool DadosDeExemplo { get; set; }

        public bool Completo => CamposPreenchidos == TotalCampos;

        public int TotalCampos => 7;

        public int CamposPreenchidos => new[]
        {
            Resumo.Nome,
            Resumo.Cnpj,
            Resumo.Endereco,
            Resumo.Cidade,
            Resumo.Uf,
            Resumo.Telefone,
            Resumo.Email
        }.Count(c => !string.IsNullOrWhiteSpace(c));
    }
}
