namespace projeto_estoque_api.Common;

/// <summary>
/// Lista fechada de status de pedido aceita pelo banco (CK_Pedido_Status)
/// e conversão entre os valores gravados e os rótulos exibidos no sistema.
/// </summary>
public static class StatusPedido
{
    public static readonly string[] Validos =
    {
        "PENDENTE", "PAGO", "PROCESSANDO", "PRONTO_PARA_RETIRADA", "RETIRADO", "CANCELADO"
    };

    public static bool Valido(string status) => Validos.Contains(status);

    /// <summary>
    /// Normaliza qualquer entrada (rótulos do sistema web, minúsculas etc.)
    /// para um dos valores aceitos pelo banco; entrada vazia ou desconhecida
    /// cai em PENDENTE (mesma regra do sistema atual).
    /// </summary>
    public static string Normalizar(string? valor)
    {
        var status = Converter(valor);

        return status is not null && Valido(status) ? status : "PENDENTE";
    }

    /// <summary>
    /// Converte o valor informado; devolve null quando não corresponde a
    /// nenhum status conhecido (permite ao chamador decidir entre erro ou padrão).
    /// </summary>
    public static string? Converter(string? valor)
    {
        var texto = (valor ?? string.Empty).Trim().ToUpperInvariant();

        return texto switch
        {
            "" => null,
            "PREPARANDO" => "PROCESSANDO",
            "PRONTO" => "PRONTO_PARA_RETIRADA",
            "PRONTO PARA RETIRADA" => "PRONTO_PARA_RETIRADA",
            "PRONTO_PARA_RETIRADA" => "PRONTO_PARA_RETIRADA",
            // Compatibilidade com valores usados antes da retirada na loja.
            "ENVIADO" => "PRONTO_PARA_RETIRADA",
            "ENTREGUE" => "RETIRADO",
            _ => Valido(texto) ? texto : null
        };
    }

    /// <summary>Rótulos usados nas telas do sistema.</summary>
    public static string Rotulo(string status) => status switch
    {
        "PENDENTE" => "Pendente",
        "PAGO" => "Pago",
        "PROCESSANDO" => "Preparando",
        "PRONTO_PARA_RETIRADA" => "Pronto para retirada",
        "RETIRADO" => "Retirado",
        "CANCELADO" => "Cancelado",
        _ => status
    };

    /// <summary>Classe CSS da badge de status nas telas.</summary>
    public static string Classe(string status) => status.ToLowerInvariant() switch
    {
        "pendente" => "pendente",
        "pago" => "pago",
        "processando" => "preparando",
        "pronto_para_retirada" => "pronto",
        "retirado" => "retirado",
        "enviado" => "pronto",
        "entregue" => "retirado",
        "cancelado" => "cancelado",
        _ => "pendente"
    };
}
