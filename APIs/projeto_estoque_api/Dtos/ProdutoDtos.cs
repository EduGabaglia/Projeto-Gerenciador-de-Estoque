namespace projeto_estoque_api.Dtos;

public record ProdutoDto(
    int Id,
    string Nome,
    int CategoriaId,
    string CategoriaNome,
    string? Descricao,
    decimal Preco,
    decimal? PrecoPromocional,
    bool EmPromocao,
    int Quantidade,
    int? LocalizacaoId,
    string? Localizacao,
    string Status,
    string StatusClasse);

public record ProdutosResumoDto(
    int TotalProdutos,
    int EstoqueBaixo,
    int CategoriasDistintas,
    decimal ValorTotalEstoque);

public record ProdutosPaginaDto(
    IReadOnlyList<ProdutoDto> Itens,
    int Total,
    int Pagina,
    int TotalPaginas,
    int ItensPorPagina,
    ProdutosResumoDto Resumo);

/// <summary>Dados aceitos no cadastro/edição de produto.</summary>
public class ProdutoSalvarDto
{
    public string? Nome { get; set; }

    public int CategoriaId { get; set; }

    public string? Descricao { get; set; }

    public decimal? Preco { get; set; }

    public int? Quantidade { get; set; }

    public int? LocalizacaoId { get; set; }
}
