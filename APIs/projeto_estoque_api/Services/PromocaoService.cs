using Microsoft.EntityFrameworkCore;
using projeto_estoque_api.Common;
using projeto_estoque_api.Data;
using projeto_estoque_api.Dtos;
using projeto_estoque_api.Models;

namespace projeto_estoque_api.Services;

public class PromocaoService
{
    private readonly EstoqueDbContext _db;

    public PromocaoService(EstoqueDbContext db) => _db = db;

    public async Task<PromocoesPaginaDto> ListarAsync(string? busca, int pagina, int itensPorPagina)
    {
        pagina = Math.Max(1, pagina);
        itensPorPagina = Math.Clamp(itensPorPagina, 1, 100);

        var consulta = _db.Promocaos
            .AsNoTracking()
            .Include(p => p.PromocaoItems)
            .ThenInclude(i => i.Produto)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            consulta = consulta.Where(p =>
                p.Nome.ToLower().Contains(termo) ||
                (p.Ativa ? "ativa" : "inativa").Contains(termo) ||
                p.PromocaoItems.Any(i => i.Produto.Nome.ToLower().Contains(termo)));
        }

        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)itensPorPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var itens = await consulta
            .OrderByDescending(p => p.Ativa)
            .ThenBy(p => p.Nome)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        var resumo = await CalcularResumoAsync();

        return new PromocoesPaginaDto(
            itens.Select(ParaDto).ToList(),
            total,
            pagina,
            totalPaginas,
            itensPorPagina,
            resumo);
    }

    public async Task<PromocaoDto> ObterAsync(int id)
    {
        var promocao = await _db.Promocaos
            .AsNoTracking()
            .Include(p => p.PromocaoItems)
            .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw ErroDeNegocioException.NaoEncontrado("Promoção não encontrada.");

        return ParaDto(promocao);
    }

    public async Task<PromocaoDto> CriarAsync(PromocaoSalvarDto dados)
    {
        var nome = ValidarNome(dados.Nome);
        var desconto = ValidarDesconto(dados.DescontoPercentual);
        var produtos = await CarregarProdutosAsync(dados.ProdutosIds);

        await GarantirNomeUnicoAsync(nome, null);

        if (dados.Ativa)
            await GarantirProdutosLivresAsync(produtos, null);

        var promocao = new Promocao
        {
            Nome = nome,
            DescontoPercentual = desconto,
            Ativa = dados.Ativa,
            DataCadastro = DateTime.Now
        };

        foreach (var produto in produtos)
            promocao.PromocaoItems.Add(new PromocaoItem { ProdutoId = produto.Id, PrecoTabela = produto.Preco });

        _db.Promocaos.Add(promocao);
        await _db.SaveChangesAsync();

        return ParaDto(promocao);
    }

    public async Task<PromocaoDto> AtualizarAsync(int id, PromocaoSalvarDto dados)
    {
        var promocao = await _db.Promocaos
            .Include(p => p.PromocaoItems)
            .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw ErroDeNegocioException.NaoEncontrado("Promoção não encontrada.");

        var nome = ValidarNome(dados.Nome);
        var desconto = ValidarDesconto(dados.DescontoPercentual);
        var produtos = await CarregarProdutosAsync(dados.ProdutosIds);

        await GarantirNomeUnicoAsync(nome, id);

        if (dados.Ativa)
            await GarantirProdutosLivresAsync(produtos, id);

        promocao.Nome = nome;
        promocao.DescontoPercentual = desconto;
        promocao.Ativa = dados.Ativa;

        _db.PromocaoItems.RemoveRange(promocao.PromocaoItems);
        await _db.SaveChangesAsync(); // libera o índice único (PromocaoId, ProdutoId)

        promocao.PromocaoItems = produtos
            .Select(produto => new PromocaoItem
            {
                PromocaoId = promocao.Id,
                ProdutoId = produto.Id,
                PrecoTabela = produto.Preco,
                Produto = produto
            })
            .ToList();

        await _db.SaveChangesAsync();

        return ParaDto(promocao);
    }

    public async Task<PromocaoDto> AlternarStatusAsync(int id)
    {
        var promocao = await _db.Promocaos
            .Include(p => p.PromocaoItems)
            .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw ErroDeNegocioException.NaoEncontrado("Promoção não encontrada.");

        if (!promocao.Ativa)
        {
            var produtos = promocao.PromocaoItems
                .Select(i => i.Produto)
                .Where(p => p is not null)
                .ToList();

            await GarantirProdutosLivresAsync(produtos, id);
        }

        promocao.Ativa = !promocao.Ativa;
        await _db.SaveChangesAsync();

        return ParaDto(promocao);
    }

    public async Task ExcluirAsync(int id)
    {
        var promocao = await _db.Promocaos
            .Include(p => p.PromocaoItems)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw ErroDeNegocioException.NaoEncontrado("Promoção não encontrada.");

        _db.PromocaoItems.RemoveRange(promocao.PromocaoItems);
        _db.Promocaos.Remove(promocao);

        await _db.SaveChangesAsync();
    }

    private async Task<PromocoesResumoDto> CalcularResumoAsync()
    {
        var promocoes = await _db.Promocaos
            .AsNoTracking()
            .Select(p => new
            {
                p.Ativa,
                p.DescontoPercentual,
                Itens = p.PromocaoItems.Select(i => new { i.ProdutoId, i.PrecoTabela }).ToList()
            })
            .ToListAsync();

        var ativas = promocoes.Where(p => p.Ativa).ToList();

        var produtosEmPromocao = ativas
            .SelectMany(p => p.Itens)
            .Select(i => i.ProdutoId)
            .Distinct()
            .Count();

        var descontoMedio = ativas.Count == 0
            ? 0m
            : Math.Round(ativas.Average(p => p.DescontoPercentual), 2, MidpointRounding.AwayFromZero);

        var economiaPotencial = ativas.Sum(p => p.Itens.Sum(i =>
            Math.Round(i.PrecoTabela - CalcularPrecoPromocional(i.PrecoTabela, p.DescontoPercentual),
                2, MidpointRounding.AwayFromZero)));

        return new PromocoesResumoDto(
            promocoes.Count,
            ativas.Count,
            produtosEmPromocao,
            descontoMedio,
            economiaPotencial);
    }

    private async Task<List<Produto>> CarregarProdutosAsync(List<int>? produtosIds)
    {
        var ids = (produtosIds ?? new List<int>())
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            throw new ErroDeNegocioException("Adicione pelo menos um produto à promoção.");

        var produtos = await _db.Produtos
            .Where(p => ids.Contains(p.Id) && p.Ativo)
            .ToListAsync();

        if (produtos.Count != ids.Count)
            throw new ErroDeNegocioException("Um dos produtos selecionados não existe mais no cadastro.");

        return produtos;
    }

    private async Task GarantirProdutosLivresAsync(IEnumerable<Produto?> produtos, int? ignorarPromocaoId)
    {
        foreach (var produto in produtos)
        {
            if (produto is null)
                continue;

            var conflito = await _db.PromocaoItems
                .AsNoTracking()
                .Include(i => i.Promocao)
                .FirstOrDefaultAsync(i =>
                    i.ProdutoId == produto.Id &&
                    i.Promocao.Ativa &&
                    i.PromocaoId != (ignorarPromocaoId ?? 0));

            if (conflito is not null)
            {
                throw ErroDeNegocioException.Conflito(
                    $"\"{produto.Nome}\" já está na promoção \"{conflito.Promocao.Nome}\". " +
                    "Desative ou exclua essa promoção, ou remova o produto.");
            }
        }
    }

    private async Task GarantirNomeUnicoAsync(string nome, int? ignorarId)
    {
        var duplicado = await _db.Promocaos
            .AsNoTracking()
            .AnyAsync(p => p.Id != (ignorarId ?? 0) && p.Nome.ToLower() == nome.ToLower());

        if (duplicado)
            throw ErroDeNegocioException.Conflito("Já existe uma promoção com este nome.");
    }

    private static string ValidarNome(string? valor)
    {
        var nome = (valor ?? string.Empty).Trim();

        if (nome.Length == 0)
            throw new ErroDeNegocioException("Informe o nome da promoção.");

        if (nome.Length > 150)
            throw new ErroDeNegocioException("O nome deve ter no máximo 150 caracteres.");

        return nome;
    }

    private static decimal ValidarDesconto(decimal? valor)
    {
        if (valor is null)
            throw new ErroDeNegocioException("Informe o percentual de desconto.");

        if (valor < 0.01m || valor > 99.99m)
            throw new ErroDeNegocioException("O desconto deve estar entre 0,01 e 99,99 %.");

        return Math.Round(valor.Value, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal CalcularPrecoPromocional(decimal precoTabela, decimal descontoPercentual)
    {
        if (precoTabela <= 0)
            return 0;

        return Math.Round(
            precoTabela * (1 - descontoPercentual / 100m),
            2,
            MidpointRounding.AwayFromZero);
    }

    private static PromocaoDto ParaDto(Promocao promocao)
    {
        var itens = promocao.PromocaoItems
            .OrderBy(i => i.Produto?.Nome ?? string.Empty)
            .Select(i =>
            {
                var precoPromocional = CalcularPrecoPromocional(i.PrecoTabela, promocao.DescontoPercentual);
                var economia = Math.Round(i.PrecoTabela - precoPromocional, 2, MidpointRounding.AwayFromZero);

                return new PromocaoItemDto(
                    i.ProdutoId,
                    i.Produto?.Nome ?? $"Produto #{i.ProdutoId}",
                    i.PrecoTabela,
                    precoPromocional,
                    economia);
            })
            .ToList();

        return new PromocaoDto(
            promocao.Id,
            promocao.Nome,
            promocao.DescontoPercentual,
            promocao.Ativa,
            itens.Count,
            itens.Sum(i => i.PrecoPromocional),
            itens.Sum(i => i.Economia),
            promocao.Ativa ? "Ativa" : "Inativa",
            promocao.Ativa ? "ativa" : "inativa",
            itens);
    }
}
