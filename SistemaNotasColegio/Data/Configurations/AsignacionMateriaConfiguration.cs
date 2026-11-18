using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaNotasColegio.Models;

namespace SistemaNotasColegio.Data.Configurations;

public class AsignacionMateriaConfiguration : IEntityTypeConfiguration<AsignacionMateria>
{
    public void Configure(EntityTypeBuilder<AsignacionMateria> builder)
    {
        builder.ToTable("AsignacionesMaterias");

        builder.HasKey(asignacion => asignacion.Id);

        builder.HasIndex(asignacion => new { asignacion.ProfesorId, asignacion.MateriaId })
            .IsUnique();

        builder.HasOne(asignacion => asignacion.Profesor)
            .WithMany(usuario => usuario.AsignacionesComoProfesor)
            .HasForeignKey(asignacion => asignacion.ProfesorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(asignacion => asignacion.Materia)
            .WithMany(materia => materia.Asignaciones)
            .HasForeignKey(asignacion => asignacion.MateriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
