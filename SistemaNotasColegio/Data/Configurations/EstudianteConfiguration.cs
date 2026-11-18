using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaNotasColegio.Models;

namespace SistemaNotasColegio.Data.Configurations;

public class EstudianteConfiguration : IEntityTypeConfiguration<Estudiante>
{
    public void Configure(EntityTypeBuilder<Estudiante> builder)
    {
        builder.ToTable("Estudiantes");

        builder.HasKey(estudiante => estudiante.Id);

        builder.Property(estudiante => estudiante.Cedula)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(estudiante => estudiante.Nombres)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(estudiante => estudiante.Apellidos)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(estudiante => estudiante.Activo)
            .HasDefaultValue(true);

        builder.HasIndex(estudiante => estudiante.Cedula)
            .IsUnique();
    }
}
