using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using projeto_estoque_api.Common;
using projeto_estoque_api.Data;
using projeto_estoque_api.Dtos;
using projeto_estoque_api.Models;

namespace projeto_estoque_api.Services;

public class ConfiguracaoService
{
    private readonly EstoqueDbContext _db;

    public ConfiguracaoService(EstoqueDbContext db) => _db = db;

    public async Task<EmpresaDto> ObterAsync()
    {
        var empresa = await _db.DadosEmpresas
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .FirstOrDefaultAsync();

        return empresa is null ? Vazio() : ParaDto(empresa);
    }

    public async Task<EmpresaDto> SalvarAsync(EmpresaSalvarDto dados)
    {
        var nome = Obrigatorio(dados.Nome, "Informe o nome da empresa.", 150, "O nome deve ter no máximo 150 caracteres.");

        var cnpj = SomenteDigitos(Obrigatorio(dados.Cnpj, "Informe um CNPJ válido.", null, null));

        if (cnpj.Length != 14)
            throw new ErroDeNegocioException("Informe um CNPJ válido, ex.: 00.000.000/0000-00.");

        var cep = SomenteDigitos(Obrigatorio(dados.Cep, "Informe o CEP.", null, null));

        if (cep.Length != 8)
            throw new ErroDeNegocioException("Informe um CEP válido, ex.: 00000-000.");

        var endereco = Obrigatorio(dados.Endereco, "Informe o endereço.", 150, "O endereço deve ter no máximo 150 caracteres.");
        var cidade = Obrigatorio(dados.Cidade, "Informe a cidade.", 100, "A cidade deve ter no máximo 100 caracteres.");
        var uf = Obrigatorio(dados.Uf, "Informe a UF.", null, null).ToUpperInvariant();

        if (uf.Length != 2)
            throw new ErroDeNegocioException("A UF deve ter 2 letras.");

        var telefone = Obrigatorio(dados.Telefone, "Informe o telefone.", 15, "O telefone deve ter no máximo 15 caracteres.");
        var email = Obrigatorio(dados.Email, "Informe o e-mail da empresa.", 254, "O e-mail deve ter no máximo 254 caracteres.");

        if (!EmailValido(email))
            throw new ErroDeNegocioException("Informe um e-mail válido.");

        // Cadastro único: aproveita o registro existente ou cria o primeiro.
        var empresa = await _db.DadosEmpresas.OrderBy(e => e.Id).FirstOrDefaultAsync();

        if (empresa is null)
        {
            empresa = new DadosEmpresa { Ativo = true };
            _db.DadosEmpresas.Add(empresa);
        }

        empresa.Nome = nome;
        empresa.Cnpj = cnpj;
        empresa.Cep = cep;
        empresa.Endereco = endereco;
        empresa.Cidade = cidade;
        empresa.Uf = uf;
        empresa.Telefone = telefone;
        empresa.Email = email;

        await _db.SaveChangesAsync();

        return ParaDto(empresa);
    }

    private static string Obrigatorio(string? valor, string mensagemVazio, int? tamanhoMaximo, string? mensagemTamanho)
    {
        var texto = (valor ?? string.Empty).Trim();

        if (texto.Length == 0)
            throw new ErroDeNegocioException(mensagemVazio);

        if (tamanhoMaximo is not null && texto.Length > tamanhoMaximo)
            throw new ErroDeNegocioException(mensagemTamanho ?? $"O campo deve ter no máximo {tamanhoMaximo} caracteres.");

        return texto;
    }

    private static string SomenteDigitos(string valor)
        => new(valor.Where(char.IsDigit).ToArray());

    private static bool EmailValido(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static EmpresaDto Vazio() => new(
        0,
        false,
        string.Empty,
        null,
        string.Empty,
        string.Empty,
        string.Empty,
        null,
        null,
        null,
        string.Empty,
        string.Empty,
        null,
        null);

    private static EmpresaDto ParaDto(DadosEmpresa empresa) => new(
        empresa.Id,
        true,
        empresa.Nome,
        empresa.NomeFantasia,
        empresa.Cnpj,
        empresa.Cep,
        empresa.Endereco,
        empresa.Numero,
        empresa.Complemento,
        empresa.Bairro,
        empresa.Cidade,
        empresa.Uf,
        empresa.Telefone,
        empresa.Email);
}
