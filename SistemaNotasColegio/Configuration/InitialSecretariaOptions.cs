namespace SistemaNotasColegio.Configuration;

public class InitialSecretariaOptions
{
    public const string SectionName = "Seed:Secretaria";

    public string Cedula { get; set; } = string.Empty;

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string NombreUsuario { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
