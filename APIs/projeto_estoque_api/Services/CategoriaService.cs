using Microsoft.EntityFrameworkCore;
using projeto_estoque_api.Common;
using projeto_estoque_api.Data;
using projeto_estoque_api.Dtos;
using projeto_estoque_api.Models;

namespace projeto_estoque_api.Services;

public class CategoriaService
{
    private readonly EstoqueDbContext _db;

    public CategoriaService(EstoqueDbContext db) => _db = db;

    public async Task<CategoriasPaginaDto> ListarAsync(string? busca, int pagina, int itensPorPagina)
    {
        pagina = Math.Max(1, pagina);
        itensPorPagina = Math.Clamp(itensPorPagina, 1, 100);

        var consulta = _db.Categorias
            .AsNoTracking()
            .Where(c => c.Ativo);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            consulta = consulta.Where(c =>
                c.Nome.ToLower().Contains(termo) ||
                (c.Descricao ?? string.Empty).ToLower().Contains(termo));
        }

        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)itensPorPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var itens = await consulta
            .OrderBy(c => c.Nome)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .Select(c => new CategoriaDto(
                c.Id,
                c.Nome,
                c.Descricao ?? string.Empty,
                c.Ativo,
                c.Produtos.Count(p => p.Ativo)))
            .ToListAsync();

        var totalCategorias = await _db.Categorias.CountAsync(c => c.Ativo);
        var emUso = await _db.Categorias.CountAsync(c => c.Ativo && c.Produtos.Any(p => p.Ativo));
        var resumo = new CategoriasResumoDto(
            totalCategorias,
            emUso,
            totalCategorias - emUso,
            await CalcularMediaAsync(emUso));

        return new CategoriasPaginaDto(itens, total, pagina, totalPaginas, itensPorPagina, resumo);
    }

    public async Task<IReadOnlyList<OpcaoDto>> OpcoesAsync()
    {
        return await _db.Categorias
            .AsNoTracking()
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .Select(c => new OpcaoDto(c.Id, c.Nome))
            .ToListAsync();
    }

    public async Task<CategoriaDto> ObterAsync(int id)
    {
        var categoria = await _db.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw ErroDeNegocioException.NaoEncontrado("Categoria não encontrada.");

        var vinculados = await _db.Produtos.CountAsync(p => p.Ativo && p.CategoriaId == id);

        return new CategoriaDto(
            categoria.Id,
            categoria.Nome,
            categoria.Descricao ?? string.Empty,
            categoria.Ativo,
            vinculados);
    }

    public async Task<CategoriaDto> CriarAsync(CategoriaSalvarDto dados)
    {
        var nome = ValidarNome(dados.Nome);
        var descricao = ValidarDescricao(dados.Descricao);

        await GarantirNomeUnicoAsync(nome, null);

        var categoria = new Categoria
        {
            Nome = nome,
            Descricao = descricao,
            Ativo = true
        };

        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync();

        return new CategoriaDto(categoria.Id, categoria.Nome, categoria.Descricao ?? string.Empty, categoria.Ativo, 0);
    }

    public async Task<CategoriaDto> AtualizarAsync(int id, CategoriaSalvarDto dados)
    {
        var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw ErroDeNegocioException.NaoEncontrado("Categoria não encontrada.");

        var nome = ValidarNome(dados.Nome);
        var descricao = ValidarDescricao(dados.Descricao);

        await GarantirNomeUnicoAsync(nome, id);

        // O vínculo com os produtos é feito por CategoriaId (FK), então o
        // renome não exige propagação manual como no sistema antigo.
        categoria.Nome = nome;
        categoria.Descricao = descricao;

        await _db.SaveChangesAsync();

        var vinculados = await _db.Produtos.CountAsync(p => p.Ativo && p.CategoriaId == id);

        return new CategoriaDto(categoria.Id, categoria.Nome, categoria.Descricao ?? string.Empty, categoria.Ativo, vinculados);
    }

    public async Task ExcluirAsync(int id)
    {
        var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw ErroDeNegocioException.NaoEncontrado("Categoria não encontrada.");

        var vinculados = await _db.Produtos.CountAsync(p => p.Ativo && p.CategoriaId == id);

        if (vinculados > 0)
        {
            throw ErroDeNegocioException.Conflito(
                $"Não é possível excluir \"{categoria.Nome}\": " +
                $"há {vinculados} produto(s) vinculado(s) a esta categoria.");
        }

        // Exclusão lógica: permite reutilizar o histórico sem apagar registros.
        categoria.Ativo = false;
        await _db.SaveChangesAsync();
    }

    private async Task<decimal> CalcularMediaAsync(int categoriasEmUso)
    {
        if (categoriasEmUso == 0)
            return 0m;

        var totalProdutos = await _db.Produtos.CountAsync(p => p.Ativo);

        return Math.Round(totalProdutos / (decimal)categoriasEmUso, 1, MidpointRounding.AwayFromZero);
    }

    private async Task GarantirNomeUnicoAsync(string nome, int? ignorarId)
    {
        var duplicado = await _db.Categorias
            .AsNoTracking()
            .AnyAsync(c => c.Id != (ignorarId ?? 0) && c.Nome.ToLower() == nome.ToLower());

        if (duplicado)
            throw ErroDeNegocioException.Conflito("Já existe uma categoria com este nome.");
    }

    private static string ValidarNome(string? valor)
    {
        var nome = (valor ?? string.Empty).Trim();

        if (nome.Length == 0)
            throw new ErroDeNegocioException("Informe o nome da categoria.");

        if (nome.Length > 80)
            throw new ErroDeNegocioException("O nome deve ter no máximo 80 caracteres.");

        return nome;
    }

    private static string ValidarDescricao(string? valor)
    {
        var descricao = (valor ?? string.Empty).Trim();

        if (descricao.Length == 0)
            throw new ErroDeNegocioException("Informe a descrição da categoria.");

        if (descricao.Length > 255)
            throw new ErroDeNegocioException("A descrição deve ter no máximo 255 caracteres.");

        return descricao;
    }
}
