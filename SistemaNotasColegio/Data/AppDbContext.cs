using Microsoft.EntityFrameworkCore;
using SistemaNotasColegio.Models;
using SistemaNotasColegio.Models.Common;

namespace SistemaNotasColegio.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();

    public DbSet<Materia> Materias => Set<Materia>();

    public DbSet<AsignacionMateria> AsignacionesMaterias => Set<AsignacionMateria>();

    public DbSet<Calificacion> Calificaciones => Set<Calificacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ApplyAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditFields()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.FechaRegistro == default)
            {
                entry.Entity.FechaRegistro = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.FechaActualizacion = now;
            }
        }
    }
}
