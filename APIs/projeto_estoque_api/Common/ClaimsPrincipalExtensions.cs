using System.Security.Claims;

namespace projeto_estoque_api.Common;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Id do usuário autenticado, ou null em requisições anônimas (web).</summary>
    public static int? UsuarioId(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(valor, out var id) ? id : null;
    }
}
