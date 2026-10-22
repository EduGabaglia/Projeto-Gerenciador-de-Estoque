namespace projeto_estoque_api.Dtos;

public record CategoriaDto(
    int Id,
    string Nome,
    string Descricao,
    bool Ativo,
    int ProdutosVinculados);

public record CategoriasResumoDto(
    int TotalCategorias,
    int EmUso,
    int Vazias,
    decimal MediaProdutosPorCategoriaEmUso);

public record CategoriasPaginaDto(
    IReadOnlyList<CategoriaDto> Itens,
    int Total,
    int Pagina,
    int TotalPaginas,
    int ItensPorPagina,
    CategoriasResumoDto Resumo);

/// <summary>Dados aceitos no cadastro/edição de categoria.</summary>
public class CategoriaSalvarDto
{
    public string? Nome { get; set; }

    public string? Descricao { get; set; }
}
