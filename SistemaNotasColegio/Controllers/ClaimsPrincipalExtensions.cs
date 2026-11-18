using System.Security.Claims;

namespace SistemaNotasColegio.Controllers;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUsuarioId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("nameid")?.Value
            ?? user.FindFirst("sub")?.Value;

        return int.TryParse(value, out var usuarioId) ? usuarioId : null;
    }
}
