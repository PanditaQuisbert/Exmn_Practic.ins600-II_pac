using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaNotasColegio.Models;

namespace SistemaNotasColegio.Data.Configurations;

public class CalificacionConfiguration : IEntityTypeConfiguration<Calificacion>
{
    public void Configure(EntityTypeBuilder<Calificacion> builder)
    {
        builder.ToTable("Calificaciones", tabla =>
        {
            tabla.HasCheckConstraint("CK_Calificaciones_Nota", "[Nota] >= 0 AND [Nota] <= 100");
            tabla.HasCheckConstraint("CK_Calificaciones_Trimestre", "[Trimestre] BETWEEN 1 AND 3");
        });

        builder.HasKey(calificacion => calificacion.Id);

        builder.Property(calificacion => calificacion.Nota)
            .HasPrecision(5, 2);

        builder.Property(calificacion => calificacion.Trimestre)
            .HasConversion<int>();

        builder.HasIndex(calificacion => new
            {
                calificacion.EstudianteId,
                calificacion.MateriaId,
                calificacion.Trimestre
            })
            .IsUnique();

        builder.HasOne(calificacion => calificacion.Estudiante)
            .WithMany(estudiante => estudiante.Calificaciones)
            .HasForeignKey(calificacion => calificacion.EstudianteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(calificacion => calificacion.Materia)
            .WithMany(materia => materia.Calificaciones)
            .HasForeignKey(calificacion => calificacion.MateriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(calificacion => calificacion.RegistradoPor)
            .WithMany()
            .HasForeignKey(calificacion => calificacion.RegistradoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(calificacion => calificacion.ActualizadoPor)
            .WithMany()
            .HasForeignKey(calificacion => calificacion.ActualizadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
