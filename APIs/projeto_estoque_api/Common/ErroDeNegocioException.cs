namespace projeto_estoque_api.Common;

/// <summary>
/// Erro de regra de negócio que deve virar uma resposta HTTP com mensagem
/// amigável (pt-BR) para o cliente — sem stack trace.
/// </summary>
public class ErroDeNegocioException : Exception
{
    public int StatusCode { get; }

    public ErroDeNegocioException(string mensagem, int statusCode = StatusCodes.Status400BadRequest)
        : base(mensagem)
    {
        StatusCode = statusCode;
    }

    public static ErroDeNegocioException NaoEncontrado(string mensagem)
        => new(mensagem, StatusCodes.Status404NotFound);

    public static ErroDeNegocioException Conflito(string mensagem)
        => new(mensagem, StatusCodes.Status409Conflict);

    public static ErroDeNegocioException NaoAutorizado(string mensagem)
        => new(mensagem, StatusCodes.Status401Unauthorized);

    public static ErroDeNegocioException Proibido(string mensagem)
        => new(mensagem, StatusCodes.Status403Forbidden);
}
