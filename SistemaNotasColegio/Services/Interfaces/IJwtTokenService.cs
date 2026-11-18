using SistemaNotasColegio.Models;

namespace SistemaNotasColegio.Services.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiraEn) CreateToken(Usuario usuario);
}
