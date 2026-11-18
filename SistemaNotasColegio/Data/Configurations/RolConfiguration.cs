using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaNotasColegio.Models;

namespace SistemaNotasColegio.Data.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(rol => rol.Id);

        builder.Property(rol => rol.Nombre)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(rol => rol.Nombre)
            .IsUnique();

        builder.HasData(
            new Rol { Id = Roles.SecretariaId, Nombre = Roles.Secretaria },
            new Rol { Id = Roles.ProfesorId, Nombre = Roles.Profesor });
    }
}
