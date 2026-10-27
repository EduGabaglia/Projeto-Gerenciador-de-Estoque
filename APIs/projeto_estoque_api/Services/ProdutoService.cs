using Microsoft.EntityFrameworkCore;
using projeto_estoque_api.Common;
using projeto_estoque_api.Data;
using projeto_estoque_api.Dtos;
using projeto_estoque_api.Models;

namespace projeto_estoque_api.Services;

public class ProdutoService
{
    private readonly EstoqueDbContext _db;

    public ProdutoService(EstoqueDbContext db) => _db = db;

    public async Task<ProdutosPaginaDto> ListarAsync(
        string? busca, int? categoriaId, bool? emPromocao, int pagina, int itensPorPagina)
    {
        pagina = Math.Max(1, pagina);
        itensPorPagina = Math.Clamp(itensPorPagina, 1, 100);

        var consulta = _db.Produtos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Localizacao)
            .Where(p => p.Ativo);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            consulta = consulta.Where(p =>
                p.Nome.ToLower().Contains(termo) ||
                p.Categoria.Nome.ToLower().Contains(termo));
        }

        if (categoriaId is > 0)
            consulta = consulta.Where(p => p.CategoriaId == categoriaId);

        if (emPromocao == true)
            consulta = consulta.Where(p => p.PromocaoItems.Any(i => i.Promocao.Ativa));

        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)itensPorPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var itens = await consulta
            .OrderBy(p => p.Nome)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        var descontos = await DescontosAtivosAsync(itens.Select(p => p.Id));
        var dtos = itens
            .Select(p => ParaDto(p, p.Categoria.Nome, p.Localizacao?.Localizacao, DescontoOuNulo(descontos, p.Id)))
            .ToList();

        var resumo = await CalcularResumoAsync();

        return new ProdutosPaginaDto(dtos, total, pagina, totalPaginas, itensPorPagina, resumo);
    }

    public async Task<ProdutoDto> ObterAsync(int id)
    {
        var produto = await _db.Produtos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Localizacao)
            .FirstOrDefaultAsync(p => p.Id == id && p.Ativo)
            ?? throw ErroDeNegocioException.NaoEncontrado("Produto não encontrado.");

        var descontos = await DescontosAtivosAsync(new[] { id });

        return ParaDto(produto, produto.Categoria.Nome, produto.Localizacao?.Localizacao, DescontoOuNulo(descontos, id));
    }

    public async Task<ProdutoDto> CriarAsync(ProdutoSalvarDto dados)
    {
        var nome = ValidarNome(dados.Nome);
        var descricao = ValidarDescricao(dados.Descricao);
        var preco = ValidarPreco(dados.Preco);
        var quantidade = ValidarQuantidade(dados.Quantidade);
        var categoria = await ValidarCategoriaAsync(dados.CategoriaId);
        var localizacao = await ValidarLocalizacaoAsync(dados.LocalizacaoId);

        await using var transacao = await _db.Database.BeginTransactionAsync();

        var produto = new Produto
        {
            Nome = nome,
            CategoriaId = categoria.Id,
            Descricao = descricao,
            Preco = preco,
            Quantidade = quantidade,
            LocalizacaoId = localizacao?.Id,
            Ativo = true,
            DataCadastro = DateTime.Now
        };

        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync();

        if (quantidade > 0)
        {
            _db.MovimentacaoEstoques.Add(new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                Tipo = "AJUSTE",
                Quantidade = quantidade,
                Data = DateTime.Now,
                Observacao = "Saldo inicial informado no cadastro do produto"
            });

            await _db.SaveChangesAsync();
        }

        await transacao.CommitAsync();

        return ParaDto(produto, categoria.Nome, localizacao?.Localizacao, null);
    }

    public async Task<ProdutoDto> AtualizarAsync(int id, ProdutoSalvarDto dados)
    {
        var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id && p.Ativo)
            ?? throw ErroDeNegocioException.NaoEncontrado("Produto não encontrado.");

        var nome = ValidarNome(dados.Nome);
        var descricao = ValidarDescricao(dados.Descricao);
        var preco = ValidarPreco(dados.Preco);
        var quantidade = ValidarQuantidade(dados.Quantidade);
        var categoria = await ValidarCategoriaAsync(dados.CategoriaId);
        var localizacao = await ValidarLocalizacaoAsync(dados.LocalizacaoId);

        var saldoAnterior = produto.Quantidade;

        produto.Nome = nome;
        produto.CategoriaId = categoria.Id;
        produto.Descricao = descricao;
        produto.Preco = preco;
        produto.Quantidade = quantidade;
        produto.LocalizacaoId = localizacao?.Id;

        await using var transacao = await _db.Database.BeginTransactionAsync();

        if (quantidade != saldoAnterior)
        {
            _db.MovimentacaoEstoques.Add(new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                Tipo = "AJUSTE",
                Quantidade = Math.Abs(quantidade - saldoAnterior),
                Data = DateTime.Now,
                Observacao = $"Ajuste de {saldoAnterior} para {quantidade} pela edição do cadastro do produto"
            });
        }

        await _db.SaveChangesAsync();
        await transacao.CommitAsync();

        var descontos = await DescontosAtivosAsync(new[] { produto.Id });

        return ParaDto(produto, categoria.Nome, localizacao?.Localizacao, DescontoOuNulo(descontos, produto.Id));
    }

    public async Task ExcluirAsync(int id)
    {
        var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == id && p.Ativo)
            ?? throw ErroDeNegocioException.NaoEncontrado("Produto não encontrado.");

        var emPromocaoAtiva = await _db.PromocaoItems
            .AsNoTracking()
            .AnyAsync(i => i.ProdutoId == id && i.Promocao.Ativa);

        if (emPromocaoAtiva)
        {
            throw ErroDeNegocioException.Conflito(
                $"\"{produto.Nome}\" está em uma promoção ativa. " +
                "Desative ou exclua a promoção antes de excluir o produto.");
        }

        // Exclusão lógica: preserva pedidos, promoções inativas e histórico de movimentações.
        produto.Ativo = false;
        await _db.SaveChangesAsync();
    }

    private async Task<ProdutosResumoDto> CalcularResumoAsync()
    {
        var produtos = await _db.Produtos
            .AsNoTracking()
            .Where(p => p.Ativo)
            .Select(p => new { p.Preco, p.Quantidade, p.CategoriaId })
            .ToListAsync();

        return new ProdutosResumoDto(
            produtos.Count,
            produtos.Count(p => p.Quantidade <= 5),
            produtos.Select(p => p.CategoriaId).Distinct().Count(),
            produtos.Sum(p => p.Preco * p.Quantidade));
    }

    private async Task<Dictionary<int, decimal>> DescontosAtivosAsync(IEnumerable<int> produtoIds)
    {
        var ids = produtoIds.ToList();

        if (ids.Count == 0)
            return new Dictionary<int, decimal>();

        var descontos = await _db.PromocaoItems
            .AsNoTracking()
            .Where(i => i.Promocao.Ativa && ids.Contains(i.ProdutoId))
            .Select(i => new { i.ProdutoId, i.Promocao.DescontoPercentual })
            .ToListAsync();

        return descontos
            .GroupBy(d => d.ProdutoId)
            .ToDictionary(g => g.Key, g => g.First().DescontoPercentual);
    }

    private static string ValidarNome(string? valor)
    {
        var nome = (valor ?? string.Empty).Trim();

        if (nome.Length == 0)
            throw new ErroDeNegocioException("Informe o nome do produto.");

        if (nome.Length > 150)
            throw new ErroDeNegocioException("O nome deve ter no máximo 150 caracteres.");

        return nome;
    }

    private static string ValidarDescricao(string? valor)
    {
        var descricao = (valor ?? string.Empty).Trim();

        if (descricao.Length > 500)
            throw new ErroDeNegocioException("A descrição deve ter no máximo 500 caracteres.");

        return descricao;
    }

    private static decimal ValidarPreco(decimal? valor)
    {
        if (valor is null)
            throw new ErroDeNegocioException("Informe o preço do produto.");

        if (valor < 0.01m || valor > 1_000_000m)
            throw new ErroDeNegocioException("O preço deve estar entre 0,01 e 1.000.000,00.");

        return Math.Round(valor.Value, 2, MidpointRounding.AwayFromZero);
    }

    private static int ValidarQuantidade(int? valor)
    {
        if (valor is null)
            throw new ErroDeNegocioException("Informe a quantidade em estoque.");

        if (valor < 0 || valor > 1_000_000)
            throw new ErroDeNegocioException("A quantidade deve estar entre 0 e 1.000.000.");

        return valor.Value;
    }

    private async Task<Categoria> ValidarCategoriaAsync(int categoriaId)
    {
        if (categoriaId <= 0)
            throw new ErroDeNegocioException("Selecione uma categoria válida.");

        return await _db.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == categoriaId && c.Ativo)
            ?? throw new ErroDeNegocioException("Selecione uma categoria válida.");
    }

    private async Task<LocalizacaoGalpao?> ValidarLocalizacaoAsync(int? localizacaoId)
    {
        if (localizacaoId is null or 0)
            return null;

        return await _db.LocalizacaoGalpaos
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == localizacaoId.Value)
            ?? throw new ErroDeNegocioException("Selecione uma localização válida.");
    }

    private static decimal? DescontoOuNulo(Dictionary<int, decimal> descontos, int produtoId)
        => descontos.TryGetValue(produtoId, out var desconto) ? desconto : null;

    private static ProdutoDto ParaDto(Produto produto, string categoriaNome, string? localizacao, decimal? descontoPercentual)
    {
        var precoPromocional = descontoPercentual is null
            ? (decimal?)null
            : Math.Round(produto.Preco * (1 - descontoPercentual.Value / 100m), 2, MidpointRounding.AwayFromZero);

        return new ProdutoDto(
            produto.Id,
            produto.Nome,
            produto.CategoriaId,
            categoriaNome,
            produto.Descricao,
            produto.Preco,
            precoPromocional,
            descontoPercentual is not null,
            produto.Quantidade,
            produto.LocalizacaoId,
            localizacao,
            StatusQuantidade(produto.Quantidade),
            ClasseQuantidade(produto.Quantidade));
    }

    public static string StatusQuantidade(int quantidade) => quantidade switch
    {
        <= 0 => "Esgotado",
        <= 5 => "Estoque baixo",
        _ => "Disponível"
    };

    public static string ClasseQuantidade(int quantidade) => quantidade switch
    {
        <= 0 => "esgotado",
        <= 5 => "baixo",
        _ => "disponivel"
    };
}
