namespace SistemaNotasColegio.DTOs.Auth;

public record LoginResponseDto(
    string Token,
    string TipoToken,
    DateTime ExpiraEn,
    UsuarioSesionDto Usuario);
