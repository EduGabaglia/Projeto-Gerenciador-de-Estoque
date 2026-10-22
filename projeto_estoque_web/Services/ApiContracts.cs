namespace projeto_estoque_web.Services
{
    // Contratos de resposta da API (JSON camelCase). O web mantém os seus
    // próprios models de tela; estes tipos apenas transportam os dados.

    public record OpcaoApiDto(int Id, string Nome);

    public record ProdutoApiDto(
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

    public record ProdutosResumoApiDto(
        int TotalProdutos,
        int EstoqueBaixo,
        int CategoriasDistintas,
        decimal ValorTotalEstoque);

    public record ProdutosPaginaApiDto(
        IReadOnlyList<ProdutoApiDto> Itens,
        int Total,
        int Pagina,
        int TotalPaginas,
        int ItensPorPagina,
        ProdutosResumoApiDto Resumo);

    public record CategoriaApiDto(
        int Id,
        string Nome,
        string Descricao,
        bool Ativo,
        int ProdutosVinculados);

    public record CategoriasResumoApiDto(
        int TotalCategorias,
        int EmUso,
        int Vazias,
        decimal MediaProdutosPorCategoriaEmUso);

    public record CategoriasPaginaApiDto(
        IReadOnlyList<CategoriaApiDto> Itens,
        int Total,
        int Pagina,
        int TotalPaginas,
        int ItensPorPagina,
        CategoriasResumoApiDto Resumo);

    public record EstoqueItemApiDto(
        int ProdutoId,
        string Nome,
        string CategoriaNome,
        int Quantidade,
        string? Localizacao,
        string Status,
        string StatusClasse);

    public record EstoqueResumoApiDto(
        int TotalProdutos,
        int EstoqueBaixo,
        int Esgotados,
        decimal ValorTotalEstoque);

    public record EstoquePaginaApiDto(
        IReadOnlyList<EstoqueItemApiDto> Itens,
        int Total,
        int Pagina,
        int TotalPaginas,
        int ItensPorPagina,
        EstoqueResumoApiDto Resumo);

    public record MovimentoResultadoApiDto(
        int ProdutoId,
        string Nome,
        string Tipo,
        int QuantidadeMovimentada,
        int SaldoAtual,
        string Mensagem);

    public record PedidoItemApiDto(
        int ProdutoId,
        string NomeProduto,
        int Quantidade,
        decimal PrecoUnitario,
        decimal Subtotal);

    public record PedidoApiDto(
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
        IReadOnlyList<PedidoItemApiDto> Itens);

    public record PedidosResumoApiDto(
        int TotalPedidos,
        int Pendentes,
        int Entregues,
        decimal ReceitaTotal);

    public record PedidosPaginaApiDto(
        IReadOnlyList<PedidoApiDto> Itens,
        int Total,
        int Pagina,
        int TotalPaginas,
        int ItensPorPagina,
        PedidosResumoApiDto Resumo);

    public record PromocaoItemApiDto(
        int ProdutoId,
        string NomeProduto,
        decimal PrecoTabela,
        decimal PrecoPromocional,
        decimal Economia);

    public record PromocaoApiDto(
        int Id,
        string Nome,
        decimal DescontoPercentual,
        bool Ativa,
        int TotalProdutos,
        decimal PrecoPromocionalTotal,
        decimal EconomiaTotal,
        string Status,
        string StatusClasse,
        IReadOnlyList<PromocaoItemApiDto> Itens);

    public record PromocoesResumoApiDto(
        int TotalPromocoes,
        int PromocoesAtivas,
        int ProdutosEmPromocao,
        decimal DescontoMedio,
        decimal EconomiaPotencial);

    public record PromocoesPaginaApiDto(
        IReadOnlyList<PromocaoApiDto> Itens,
        int Total,
        int Pagina,
        int TotalPaginas,
        int ItensPorPagina,
        PromocoesResumoApiDto Resumo);

    public record EmpresaApiDto(
        int Id,
        bool Preenchido,
        string Nome,
        string? NomeFantasia,
        string Cnpj,
        string Cep,
        string Endereco,
        string? Numero,
        string? Complemento,
        string? Bairro,
        string Cidade,
        string Uf,
        string? Telefone,
        string? Email);

    public record DashboardResumoApiDto(
        int TotalProdutos,
        int EstoqueBaixo,
        int PedidosPendentes,
        decimal VendasMes);

    public record UltimoPedidoApiDto(
        int Id,
        string Cliente,
        DateTime Data,
        string Status,
        string StatusLabel,
        string StatusClasse,
        int TotalItens,
        decimal ValorTotal);

    public record ProdutoMaisVendidoApiDto(string NomeProduto, int Quantidade);

    public record VendaDiaApiDto(DateTime Data, decimal Valor);

    public record DashboardApiDto(
        DashboardResumoApiDto Resumo,
        IReadOnlyList<UltimoPedidoApiDto> UltimosPedidos,
        IReadOnlyList<ProdutoMaisVendidoApiDto> MaisVendidos,
        IReadOnlyList<VendaDiaApiDto> VendasUltimos7Dias);

    // Contratos de envio (corpo JSON).

    public class ProdutoSalvarApiDto
    {
        public string? Nome { get; set; }
        public int CategoriaId { get; set; }
        public string? Descricao { get; set; }
        public decimal? Preco { get; set; }
        public int? Quantidade { get; set; }
        public int? LocalizacaoId { get; set; }
    }

    public class CategoriaSalvarApiDto
    {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
    }

    public class MovimentoSalvarApiDto
    {
        public int ProdutoId { get; set; }
        public string? Tipo { get; set; }
        public int? Quantidade { get; set; }
        public string? Observacao { get; set; }
    }

    public class ItemPedidoSalvarApiDto
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }

    public class PedidoSalvarApiDto
    {
        public string? Cliente { get; set; }
        public DateTime? Data { get; set; }
        public string? Status { get; set; }
        public string? Observacao { get; set; }
        public List<ItemPedidoSalvarApiDto>? Itens { get; set; }
    }

    public class PromocaoSalvarApiDto
    {
        public string? Nome { get; set; }
        public decimal? DescontoPercentual { get; set; }
        public bool Ativa { get; set; } = true;
        public List<int>? ProdutosIds { get; set; }
    }

    public class EmpresaSalvarApiDto
    {
        public string? Nome { get; set; }
        public string? Cnpj { get; set; }
        public string? Cep { get; set; }
        public string? Endereco { get; set; }
        public string? Cidade { get; set; }
        public string? Uf { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
    }
}
