namespace projeto_estoque_web.Services
{
    /// <summary>
    /// Erro devolvido pela API (mensagem de negócio em { "mensagem": "..." })
    /// ou falha de comunicação que impeça concluir a operação.
    /// </summary>
    public class ApiException : Exception
    {
        public int StatusCode { get; }

        public ApiException(int statusCode, string mensagem) : base(mensagem)
        {
            StatusCode = statusCode;
        }
    }
}
