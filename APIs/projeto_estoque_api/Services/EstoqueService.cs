using Microsoft.EntityFrameworkCore;
using projeto_estoque_api.Common;
using projeto_estoque_api.Data;
using projeto_estoque_api.Dtos;
using projeto_estoque_api.Models;

namespace projeto_estoque_api.Services;

public class EstoqueService
{
    private readonly EstoqueDbContext _db;

    public EstoqueService(EstoqueDbContext db) => _db = db;

    public async Task<EstoquePaginaDto> ListarAsync(string? busca, int pagina, int itensPorPagina)
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

        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)itensPorPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var itens = await consulta
            .OrderBy(p => p.Nome)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .Select(p => new EstoqueItemDto(
                p.Id,
                p.Nome,
                p.Categoria.Nome,
                p.Quantidade,
                p.Localizacao != null ? p.Localizacao.Localizacao : null,
                string.Empty,
                string.Empty))
            .ToListAsync();

        itens = itens
            .Select(i => i with
            {
                Status = ProdutoService.StatusQuantidade(i.Quantidade),
                StatusClasse = ProdutoService.ClasseQuantidade(i.Quantidade)
            })
            .ToList();

        var resumo = await CalcularResumoAsync();

        return new EstoquePaginaDto(itens, total, pagina, totalPaginas, itensPorPagina, resumo);
    }

    public async Task<MovimentoResultadoDto> MovimentarAsync(MovimentoSalvarDto dados)
    {
        var tipo = (dados.Tipo ?? string.Empty).Trim().ToUpperInvariant();

        if (tipo is not ("ENTRADA" or "SAIDA" or "AJUSTE"))
            throw new ErroDeNegocioException("Tipo de movimentação inválido.");

        if (dados.Quantidade is null)
            throw new ErroDeNegocioException("Informe a quantidade da movimentação.");

        var quantidade = dados.Quantidade.Value;

        if (tipo == "AJUSTE")
        {
            if (quantidade < 0 || quantidade > 1_000_000)
                throw new ErroDeNegocioException("A quantidade do ajuste deve estar entre 0 e 1.000.000.");
        }
        else if (quantidade < 1 || quantidade > 1_000_000)
        {
            throw new ErroDeNegocioException("A quantidade deve estar entre 1 e 1.000.000.");
        }

        var produto = await _db.Produtos.FirstOrDefaultAsync(p => p.Id == dados.ProdutoId && p.Ativo)
            ?? throw new ErroDeNegocioException("Selecione um produto.");

        var saldoAnterior = produto.Quantidade;
        int novoSaldo;
        string mensagem;
        (string Tipo, int Quantidade, string? Observacao)? movimento = null;

        switch (tipo)
        {
            case "ENTRADA":
                novoSaldo = saldoAnterior + quantidade;
                movimento = ("ENTRADA", quantidade, MontarObservacao(null, dados.Observacao));
                mensagem = $"Entrada de {quantidade} unidade(s) registrada para \"{produto.Nome}\".";
                break;

            case "SAIDA":
                if (quantidade > saldoAnterior)
                {
                    throw new ErroDeNegocioException(
                        $"Estoque insuficiente para \"{produto.Nome}\". Saldo atual: {saldoAnterior}.");
                }

                novoSaldo = saldoAnterior - quantidade;
                movimento = ("SAIDA", quantidade, MontarObservacao(null, dados.Observacao));
                mensagem = $"Saída de {quantidade} unidade(s) registrada para \"{produto.Nome}\".";
                break;

            default: // AJUSTE
                novoSaldo = quantidade;
                var diferenca = Math.Abs(novoSaldo - saldoAnterior);

                if (diferenca > 0)
                {
                    movimento = ("AJUSTE", diferenca,
                        MontarObservacao($"Ajuste de {saldoAnterior} para {novoSaldo}", dados.Observacao));
                }

                mensagem = $"Saldo de \"{produto.Nome}\" ajustado para {novoSaldo}.";
                break;
        }

        if (novoSaldo > 1_000_000)
            throw new ErroDeNegocioException("O saldo não pode ultrapassar 1.000.000 de unidades.");

        await using var transacao = await _db.Database.BeginTransactionAsync();

        produto.Quantidade = novoSaldo;

        if (movimento is not null)
        {
            _db.MovimentacaoEstoques.Add(new MovimentacaoEstoque
            {
                ProdutoId = produto.Id,
                Tipo = movimento.Value.Tipo,
                Quantidade = movimento.Value.Quantidade,
                Data = DateTime.Now,
                Observacao = movimento.Value.Observacao
            });
        }

        await _db.SaveChangesAsync();
        await transacao.CommitAsync();

        return new MovimentoResultadoDto(produto.Id, produto.Nome, tipo, quantidade, novoSaldo, mensagem);
    }

    public async Task<PaginaDto<MovimentacaoDto>> ListarMovimentosAsync(int? produtoId, int pagina, int itensPorPagina)
    {
        pagina = Math.Max(1, pagina);
        itensPorPagina = Math.Clamp(itensPorPagina, 1, 100);

        var consulta = _db.MovimentacaoEstoques
            .AsNoTracking()
            .Include(m => m.Produto)
            .AsQueryable();

        if (produtoId is > 0)
            consulta = consulta.Where(m => m.ProdutoId == produtoId);

        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)itensPorPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var itens = await consulta
            .OrderByDescending(m => m.Data)
            .ThenByDescending(m => m.Id)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .Select(m => new MovimentacaoDto(
                m.Id,
                m.ProdutoId,
                m.Produto.Nome,
                m.Tipo,
                m.Quantidade,
                m.Data,
                m.Observacao))
            .ToListAsync();

        return new PaginaDto<MovimentacaoDto>(itens, total, pagina, totalPaginas, itensPorPagina);
    }

    private async Task<EstoqueResumoDto> CalcularResumoAsync()
    {
        var produtos = await _db.Produtos
            .AsNoTracking()
            .Where(p => p.Ativo)
            .Select(p => new { p.Preco, p.Quantidade })
            .ToListAsync();

        return new EstoqueResumoDto(
            produtos.Count,
            produtos.Count(p => p.Quantidade <= 5),
            produtos.Count(p => p.Quantidade <= 0),
            produtos.Sum(p => p.Preco * p.Quantidade));
    }

    private static string? MontarObservacao(string? prefixo, string? observacao)
    {
        var partes = new[] { prefixo, (observacao ?? string.Empty).Trim() }
            .Where(parte => !string.IsNullOrWhiteSpace(parte))
            .ToList();

        if (partes.Count == 0)
            return null;

        var texto = string.Join(" - ", partes);

        return texto.Length > 500 ? texto[..500] : texto;
    }
}
