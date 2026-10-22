namespace projeto_estoque_api.Dtos;

/// <summary>Resultado paginado genérico com resumo de indicadores opcional.</summary>
public record PaginaDto<T>(
    IReadOnlyList<T> Itens,
    int Total,
    int Pagina,
    int TotalPaginas,
    int ItensPorPagina);

/// <summary>Item simples para seletores (id + nome).</summary>
public record OpcaoDto(int Id, string Nome);

/// <summary>Resposta padrão de erro/mensagem da API.</summary>
public record MensagemDto(string Mensagem);
