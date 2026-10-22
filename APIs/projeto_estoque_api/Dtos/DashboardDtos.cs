namespace projeto_estoque_api.Dtos;

public record DashboardResumoDto(
    int TotalProdutos,
    int EstoqueBaixo,
    int PedidosPendentes,
    decimal VendasMes);

public record UltimoPedidoDto(
    int Id,
    string Cliente,
    DateTime Data,
    string Status,
    string StatusLabel,
    string StatusClasse,
    int TotalItens,
    decimal ValorTotal);

public record ProdutoMaisVendidoDto(string NomeProduto, int Quantidade);

public record VendaDiaDto(DateTime Data, decimal Valor);

public record DashboardDto(
    DashboardResumoDto Resumo,
    IReadOnlyList<UltimoPedidoDto> UltimosPedidos,
    IReadOnlyList<ProdutoMaisVendidoDto> MaisVendidos,
    IReadOnlyList<VendaDiaDto> VendasUltimos7Dias);
