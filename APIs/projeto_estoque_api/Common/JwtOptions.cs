namespace projeto_estoque_api.Common;

/// <summary>Configuração do token JWT usada pelo mobile.</summary>
public class JwtOptions
{
    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "projeto_estoque_api";

    public string Audience { get; set; } = "projeto_estoque_mobile";

    public int ExpiraEmDias { get; set; } = 7;
}
