namespace projeto_estoque_api.Dtos;

public record PedidoItemDto(
    int ProdutoId,
    string NomeProduto,
    int Quantidade,
    decimal PrecoUnitario,
    decimal Subtotal);

public record PedidoDto(
    int Id,
    int UsuarioId,
    string Cliente,
    DateTime Data,
    string Status,
    string StatusLabel,
    string StatusClasse,
    int TotalItens,
    decimal ValorTotal,
    string? Observacao,
    IReadOnlyList<PedidoItemDto> Itens);

public record PedidosResumoDto(
    int TotalPedidos,
    int Pendentes,
    int Retirados,
    decimal ReceitaTotal);

public record PedidosPaginaDto(
    IReadOnlyList<PedidoDto> Itens,
    int Total,
    int Pagina,
    int TotalPaginas,
    int ItensPorPagina,
    PedidosResumoDto Resumo);

public record ItemPedidoSalvarDto(int ProdutoId, int Quantidade);

/// <summary>Dados aceitos na criação/edição de pedido.</summary>
public class PedidoSalvarDto
{
    public string? Cliente { get; set; }

    public DateTime? Data { get; set; }

    public string? Status { get; set; }

    public string? Observacao { get; set; }

    public List<ItemPedidoSalvarDto>? Itens { get; set; }
}

/// <summary>Dados aceitos na alteração de status.</summary>
public class StatusPedidoDto
{
    public string? Status { get; set; }
}
