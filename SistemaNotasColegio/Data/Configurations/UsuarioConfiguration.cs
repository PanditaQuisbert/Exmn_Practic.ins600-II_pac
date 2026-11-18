using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaNotasColegio.Models;

namespace SistemaNotasColegio.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(usuario => usuario.Id);

        builder.Property(usuario => usuario.Cedula)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(usuario => usuario.Nombres)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(usuario => usuario.Apellidos)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(usuario => usuario.Correo)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(usuario => usuario.NombreUsuario)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(usuario => usuario.PasswordHash)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(usuario => usuario.Activo)
            .HasDefaultValue(true);

        builder.HasIndex(usuario => usuario.Cedula)
            .IsUnique();

        builder.HasIndex(usuario => usuario.Correo)
            .IsUnique();

        builder.HasIndex(usuario => usuario.NombreUsuario)
            .IsUnique();

        builder.HasOne(usuario => usuario.Rol)
            .WithMany(rol => rol.Usuarios)
            .HasForeignKey(usuario => usuario.RolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
