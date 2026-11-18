namespace SistemaNotasColegio.Data.Seed;

public interface IDatabaseSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
