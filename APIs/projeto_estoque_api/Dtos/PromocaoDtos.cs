namespace projeto_estoque_api.Dtos;

public record PromocaoItemDto(
    int ProdutoId,
    string NomeProduto,
    decimal PrecoTabela,
    decimal PrecoPromocional,
    decimal Economia);

public record PromocaoDto(
    int Id,
    string Nome,
    decimal DescontoPercentual,
    bool Ativa,
    int TotalProdutos,
    decimal PrecoPromocionalTotal,
    decimal EconomiaTotal,
    string Status,
    string StatusClasse,
    IReadOnlyList<PromocaoItemDto> Itens);

public record PromocoesResumoDto(
    int TotalPromocoes,
    int PromocoesAtivas,
    int ProdutosEmPromocao,
    decimal DescontoMedio,
    decimal EconomiaPotencial);

public record PromocoesPaginaDto(
    IReadOnlyList<PromocaoDto> Itens,
    int Total,
    int Pagina,
    int TotalPaginas,
    int ItensPorPagina,
    PromocoesResumoDto Resumo);

/// <summary>Dados aceitos no cadastro/edição de promoção.</summary>
public class PromocaoSalvarDto
{
    public string? Nome { get; set; }

    public decimal? DescontoPercentual { get; set; }

    public bool Ativa { get; set; } = true;

    public List<int>? ProdutosIds { get; set; }
}
