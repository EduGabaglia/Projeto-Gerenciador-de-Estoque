namespace projeto_estoque_api.Dtos;

public record EstoqueItemDto(
    int ProdutoId,
    string Nome,
    string CategoriaNome,
    int Quantidade,
    string? Localizacao,
    string Status,
    string StatusClasse);

public record EstoqueResumoDto(
    int TotalProdutos,
    int EstoqueBaixo,
    int Esgotados,
    decimal ValorTotalEstoque);

public record EstoquePaginaDto(
    IReadOnlyList<EstoqueItemDto> Itens,
    int Total,
    int Pagina,
    int TotalPaginas,
    int ItensPorPagina,
    EstoqueResumoDto Resumo);

public record MovimentacaoDto(
    int Id,
    int ProdutoId,
    string NomeProduto,
    string Tipo,
    int Quantidade,
    DateTime Data,
    string? Observacao);

/// <summary>Dados aceitos no registro de entrada, saída ou ajuste de estoque.</summary>
public class MovimentoSalvarDto
{
    public int ProdutoId { get; set; }

    /// <summary>ENTRADA, SAIDA ou AJUSTE.</summary>
    public string? Tipo { get; set; }

    public int? Quantidade { get; set; }

    public string? Observacao { get; set; }
}

public record MovimentoResultadoDto(
    int ProdutoId,
    string Nome,
    string Tipo,
    int QuantidadeMovimentada,
    int SaldoAtual,
    string Mensagem);
