/*
================================================================================
 SISTEMA DE NOTAS ESCOLAR - CREACIÓN DE LA BASE DE DATOS
================================================================================
 Base de datos : SistemaNotasColegioDB
 Motor         : Microsoft SQL Server 2016 o superior
 Collation     : Modern_Spanish_CI_AS
 Origen        : Migración EF Core 20261118140736_InitialCreate (EF Core 10.0.15)
 Script        : Idempotente (se puede ejecutar mas de una vez sin duplicar objetos)
================================================================================

 COMO EJECUTARLO EN SQL SERVER MANAGEMENT STUDIO (SSMS)
 ------------------------------------------------------
 1. Abra SSMS y conéctese a la instancia de SQL Server (por ejemplo
    localhost\SQLEXPRESS) con autenticación de Windows.
 2. Menú Archivo > Abrir > Archivo... y seleccione este archivo
    (database\SistemaNotasColegioDB.sql).
 3. No hace falta seleccionar una base de datos en la barra de herramientas:
    el script crea SistemaNotasColegioDB si no existe y se posiciona en ella
    mediante USE.
 4. Presione F5 (o el botón Ejecutar).
 5. Actualice el nodo "Bases de datos" del Explorador de objetos para ver
    SistemaNotasColegioDB con las 6 tablas del sistema.

 CONTENIDO
 ---------
 - Creación de la base de datos con collation Modern_Spanish_CI_AS.
 - Tablas: Roles, Usuarios, Estudiantes, Materias, AsignacionesMaterias,
   Calificaciones y la tabla de control __EFMigrationsHistory.
 - Claves primarias, claves foráneas (ON DELETE NO ACTION), índices únicos y
   restricciones CHECK de Calificaciones (Nota entre 0 y 100, Trimestre 1-3).
 - Roles iniciales: Id 1 = Secretaría, Id 2 = Profesor.
 - Registro de la migración inicial en __EFMigrationsHistory.

 IMPORTANTE
 ----------
 - El script NO incluye usuarios, contraseñas, hashes BCrypt, claves JWT ni
   ningún otro secreto.
 - El usuario inicial de Secretaría se crea automáticamente la primera vez
   que se inicia la API, a partir de la configuración segura
   "Seed:Secretaria" (User Secrets o variables de entorno).
 - El script no depende de rutas MDF/LDF ni de una instancia concreta.
================================================================================
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF DB_ID(N'SistemaNotasColegioDB') IS NULL
BEGIN
    CREATE DATABASE [SistemaNotasColegioDB] COLLATE Modern_Spanish_CI_AS;
END;
GO

USE [SistemaNotasColegioDB];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE TABLE [Estudiantes] (
        [Id] int NOT NULL IDENTITY,
        [Cedula] nvarchar(20) NOT NULL,
        [Nombres] nvarchar(100) NOT NULL,
        [Apellidos] nvarchar(100) NOT NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaRegistro] datetime2 NOT NULL,
        [FechaActualizacion] datetime2 NULL,
        CONSTRAINT [PK_Estudiantes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE TABLE [Materias] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(20) NOT NULL,
        [Nombre] nvarchar(150) NOT NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaRegistro] datetime2 NOT NULL,
        [FechaActualizacion] datetime2 NULL,
        CONSTRAINT [PK_Materias] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] int NOT NULL IDENTITY,
        [Cedula] nvarchar(20) NOT NULL,
        [Nombres] nvarchar(100) NOT NULL,
        [Apellidos] nvarchar(100) NOT NULL,
        [Correo] nvarchar(150) NOT NULL,
        [NombreUsuario] nvarchar(50) NOT NULL,
        [PasswordHash] nvarchar(255) NOT NULL,
        [RolId] int NOT NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [FechaRegistro] datetime2 NOT NULL,
        [FechaActualizacion] datetime2 NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Usuarios_Roles_RolId] FOREIGN KEY ([RolId]) REFERENCES [Roles] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE TABLE [AsignacionesMaterias] (
        [Id] int NOT NULL IDENTITY,
        [ProfesorId] int NOT NULL,
        [MateriaId] int NOT NULL,
        [FechaRegistro] datetime2 NOT NULL,
        [FechaActualizacion] datetime2 NULL,
        CONSTRAINT [PK_AsignacionesMaterias] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AsignacionesMaterias_Materias_MateriaId] FOREIGN KEY ([MateriaId]) REFERENCES [Materias] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AsignacionesMaterias_Usuarios_ProfesorId] FOREIGN KEY ([ProfesorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE TABLE [Calificaciones] (
        [Id] int NOT NULL IDENTITY,
        [EstudianteId] int NOT NULL,
        [MateriaId] int NOT NULL,
        [Trimestre] int NOT NULL,
        [Nota] decimal(5,2) NOT NULL,
        [RegistradoPorUsuarioId] int NOT NULL,
        [ActualizadoPorUsuarioId] int NULL,
        [FechaRegistro] datetime2 NOT NULL,
        [FechaActualizacion] datetime2 NULL,
        CONSTRAINT [PK_Calificaciones] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Calificaciones_Nota] CHECK ([Nota] >= 0 AND [Nota] <= 100),
        CONSTRAINT [CK_Calificaciones_Trimestre] CHECK ([Trimestre] BETWEEN 1 AND 3),
        CONSTRAINT [FK_Calificaciones_Estudiantes_EstudianteId] FOREIGN KEY ([EstudianteId]) REFERENCES [Estudiantes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Calificaciones_Materias_MateriaId] FOREIGN KEY ([MateriaId]) REFERENCES [Materias] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Calificaciones_Usuarios_ActualizadoPorUsuarioId] FOREIGN KEY ([ActualizadoPorUsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Calificaciones_Usuarios_RegistradoPorUsuarioId] FOREIGN KEY ([RegistradoPorUsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Roles]'))
        SET IDENTITY_INSERT [Roles] ON;
    EXEC(N'INSERT INTO [Roles] ([Id], [Nombre])
    VALUES (1, N''Secretaría''),
    (2, N''Profesor'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Roles]'))
        SET IDENTITY_INSERT [Roles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AsignacionesMaterias_MateriaId] ON [AsignacionesMaterias] ([MateriaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AsignacionesMaterias_ProfesorId_MateriaId] ON [AsignacionesMaterias] ([ProfesorId], [MateriaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Calificaciones_ActualizadoPorUsuarioId] ON [Calificaciones] ([ActualizadoPorUsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Calificaciones_EstudianteId_MateriaId_Trimestre] ON [Calificaciones] ([EstudianteId], [MateriaId], [Trimestre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Calificaciones_MateriaId] ON [Calificaciones] ([MateriaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Calificaciones_RegistradoPorUsuarioId] ON [Calificaciones] ([RegistradoPorUsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Estudiantes_Cedula] ON [Estudiantes] ([Cedula]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Materias_Codigo] ON [Materias] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_Nombre] ON [Roles] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Cedula] ON [Usuarios] ([Cedula]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Correo] ON [Usuarios] ([Correo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_NombreUsuario] ON [Usuarios] ([NombreUsuario]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Usuarios_RolId] ON [Usuarios] ([RolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261118140736_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261118140736_InitialCreate', N'10.0.15');
END;

COMMIT;
GO

