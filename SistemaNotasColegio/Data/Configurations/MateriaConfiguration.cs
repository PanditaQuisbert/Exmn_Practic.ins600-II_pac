using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaNotasColegio.Models;

namespace SistemaNotasColegio.Data.Configurations;

public class MateriaConfiguration : IEntityTypeConfiguration<Materia>
{
    public void Configure(EntityTypeBuilder<Materia> builder)
    {
        builder.ToTable("Materias");

        builder.HasKey(materia => materia.Id);

        builder.Property(materia => materia.Codigo)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(materia => materia.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(materia => materia.Activo)
            .HasDefaultValue(true);

        builder.HasIndex(materia => materia.Codigo)
            .IsUnique();
    }
}
