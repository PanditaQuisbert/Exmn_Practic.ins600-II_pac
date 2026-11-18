using Microsoft.EntityFrameworkCore;
using SistemaNotasColegio.Data;
using SistemaNotasColegio.DTOs.Roles;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Services;

public class RolService : IRolService
{
    private readonly AppDbContext _dbContext;

    public RolService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RolResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .OrderBy(rol => rol.Id)
            .Select(rol => new RolResponseDto(rol.Id, rol.Nombre))
            .ToListAsync(cancellationToken);
    }
}
