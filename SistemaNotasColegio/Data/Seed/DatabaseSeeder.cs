using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SistemaNotasColegio.Configuration;
using SistemaNotasColegio.Models;
using SistemaNotasColegio.Services.Interfaces;

namespace SistemaNotasColegio.Data.Seed;

public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly InitialSecretariaOptions _secretariaOptions;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        AppDbContext dbContext,
        IPasswordService passwordService,
        IOptions<InitialSecretariaOptions> secretariaOptions,
        ILogger<DatabaseSeeder> logger)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _secretariaOptions = secretariaOptions.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var secretariaExists = await _dbContext.Usuarios
            .AnyAsync(usuario => usuario.RolId == Roles.SecretariaId, cancellationToken);

        if (secretariaExists)
        {
            _logger.LogInformation(
                "Ya existe al menos un usuario de Secretaría. No se crea el usuario inicial.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_secretariaOptions.NombreUsuario)
            && string.IsNullOrWhiteSpace(_secretariaOptions.Password))
        {
            _logger.LogWarning(
                "No se configuró el usuario inicial de Secretaría (Seed:Secretaria). " +
                "Configúrelo mediante User Secrets o variables de entorno antes de iniciar la API.");
            return;
        }

        ValidateOptions();

        var nombreUsuario = _secretariaOptions.NombreUsuario.Trim();
        var cedula = _secretariaOptions.Cedula.Trim();
        var correo = _secretariaOptions.Correo.Trim();

        var duplicate = await _dbContext.Usuarios
            .AsNoTracking()
            .SingleOrDefaultAsync(
                usuario => usuario.NombreUsuario == nombreUsuario
                    || usuario.Cedula == cedula
                    || usuario.Correo == correo,
                cancellationToken);

        if (duplicate is not null)
        {
            throw new InvalidOperationException(
                "No se puede crear el usuario inicial de Secretaría porque ya existe un usuario " +
                "con la misma cédula, correo o nombre de usuario. Revise la configuración de Seed:Secretaria.");
        }

        var secretaria = new Usuario
        {
            Cedula = cedula,
            Nombres = _secretariaOptions.Nombres.Trim(),
            Apellidos = _secretariaOptions.Apellidos.Trim(),
            Correo = correo,
            NombreUsuario = nombreUsuario,
            PasswordHash = _passwordService.Hash(_secretariaOptions.Password),
            RolId = Roles.SecretariaId,
            Activo = true
        };

        _dbContext.Usuarios.Add(secretaria);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Usuario inicial de Secretaría creado correctamente: {NombreUsuario}.",
            nombreUsuario);
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_secretariaOptions.Cedula)
            || string.IsNullOrWhiteSpace(_secretariaOptions.Nombres)
            || string.IsNullOrWhiteSpace(_secretariaOptions.Apellidos)
            || string.IsNullOrWhiteSpace(_secretariaOptions.Correo)
            || string.IsNullOrWhiteSpace(_secretariaOptions.NombreUsuario)
            || string.IsNullOrWhiteSpace(_secretariaOptions.Password))
        {
            throw new InvalidOperationException(
                "La configuración Seed:Secretaria está incompleta. " +
                "Configure Cedula, Nombres, Apellidos, Correo, NombreUsuario y Password.");
        }

        if (!PasswordPolicy.IsStrong(_secretariaOptions.Password))
        {
            throw new InvalidOperationException(
                "La contraseña inicial de Secretaría no cumple la política de seguridad: " +
                $"mínimo {PasswordPolicy.InitialSecretariaMinimumLength} caracteres, " +
                "con mayúscula, minúscula, número y símbolo.");
        }
    }
}
