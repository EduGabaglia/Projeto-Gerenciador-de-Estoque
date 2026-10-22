using System.Text.Json;
using projeto_estoque_api.Common;

namespace projeto_estoque_api.Middlewares;

/// <summary>
/// Converte ErroDeNegocioException em resposta JSON { "mensagem": "..." } com o
/// status adequado e evita que o cliente web receba stack trace em erros 500.
/// </summary>
public class ErroMiddleware
{
    private static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _proxima;
    private readonly ILogger<ErroMiddleware> _logger;

    public ErroMiddleware(RequestDelegate proxima, ILogger<ErroMiddleware> logger)
    {
        _proxima = proxima;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _proxima(contexto);
        }
        catch (ErroDeNegocioException erro)
        {
            await EscreverRespostaAsync(contexto, erro.StatusCode, erro.Message);
        }
        catch (Exception erro)
        {
            _logger.LogError(erro, "Erro inesperado ao processar a requisição");

            await EscreverRespostaAsync(
                contexto,
                StatusCodes.Status500InternalServerError,
                "Erro inesperado no servidor. Tente novamente em instantes.");
        }
    }

    private async Task EscreverRespostaAsync(HttpContext contexto, int statusCode, string mensagem)
    {
        if (contexto.Response.HasStarted)
            return;

        contexto.Response.Clear();
        contexto.Response.StatusCode = statusCode;
        contexto.Response.ContentType = "application/json; charset=utf-8";

        var corpo = JsonSerializer.Serialize(new { mensagem }, OpcoesJson);

        await contexto.Response.WriteAsync(corpo);
    }
}
