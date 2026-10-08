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
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE TABLE [Cliente] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(300) NOT NULL,
        [DocumentoIdentidad] nvarchar(40) NULL,
        [Telefono] nvarchar(40) NULL,
        [FechaRegistro] datetime2 NULL,
        CONSTRAINT [PK_Cliente] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE TABLE [MetodoPago] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_MetodoPago] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE TABLE [Permiso] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Permiso] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE TABLE [Rol] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Rol] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE TABLE [TipoPropiedad] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_TipoPropiedad] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE TABLE [Prestamo] (
        [Id] int NOT NULL IDENTITY,
        [MontoCapital] decimal(10,2) NOT NULL,
        [Interes] decimal(10,2) NOT NULL,
        [TotalCuotas] decimal(10,2) NOT NULL,
        [FechaInicio] datetime2 NOT NULL,
        [Estado] bit NOT NULL DEFAULT CAST(1 AS bit),
        [ClienteId] int NOT NULL,
        CONSTRAINT [PK_Prestamo] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Prestamo_Cliente_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Cliente] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE TABLE [RolPermiso] (
        [RolId] int NOT NULL,
        [PermisoId] int NOT NULL,
        CONSTRAINT [PK_RolPermiso] PRIMARY KEY ([RolId], [PermisoId]),
        CONSTRAINT [FK_RolPermiso_Permiso_PermisoId] FOREIGN KEY ([PermisoId]) REFERENCES [Permiso] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolPermiso_Rol_RolId] FOREIGN KEY ([RolId]) REFERENCES [Rol] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE TABLE [Usuario] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Gmail] nvarchar(200) NOT NULL,
        [Password] nvarchar(512) NOT NULL,
        [Estado] bit NOT NULL DEFAULT CAST(1 AS bit),
        [RolId] int NOT NULL,
        CONSTRAINT [PK_Usuario] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Usuario_Rol_RolId] FOREIGN KEY ([RolId]) REFERENCES [Rol] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE TABLE [Propiedad] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Direccion] nvarchar(512) NULL,
        [CodigoLuz] nvarchar(100) NOT NULL,
        [CodigoAgua] nvarchar(100) NOT NULL,
        [CodigoGas] nvarchar(100) NULL,
        [Estado] bit NOT NULL DEFAULT CAST(1 AS bit),
        [TipoPropiedadId] int NOT NULL,
        CONSTRAINT [PK_Propiedad] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Propiedad_TipoPropiedad_TipoPropiedadId] FOREIGN KEY ([TipoPropiedadId]) REFERENCES [TipoPropiedad] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE TABLE [Pago] (
        [Id] int NOT NULL IDENTITY,
        [Fecha] datetime2 NOT NULL,
        [MontoTotal] decimal(10,2) NOT NULL,
        [MetodoPagoId] int NOT NULL,
        [UsuarioId] int NOT NULL,
        CONSTRAINT [PK_Pago] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Pago_MetodoPago_MetodoPagoId] FOREIGN KEY ([MetodoPagoId]) REFERENCES [MetodoPago] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Pago_Usuario_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE TABLE [Contrato] (
        [Id] int NOT NULL IDENTITY,
        [MontoAlquiler] decimal(10,2) NOT NULL,
        [MontoGarantia] decimal(10,2) NOT NULL,
        [DiaPago] datetime2 NOT NULL,
        [FechaInicio] datetime2 NOT NULL,
        [FechaFin] datetime2 NOT NULL,
        [Estado] bit NOT NULL DEFAULT CAST(1 AS bit),
        [PropiedadId] int NOT NULL,
        [ClienteId] int NOT NULL,
        [UsuarioId] int NOT NULL,
        CONSTRAINT [PK_Contrato] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Contrato_Cliente_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Cliente] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Contrato_Propiedad_PropiedadId] FOREIGN KEY ([PropiedadId]) REFERENCES [Propiedad] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Contrato_Usuario_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE TABLE [PagoDetalle] (
        [Id] int NOT NULL IDENTITY,
        [MontoAplicado] decimal(10,2) NOT NULL,
        [FechaPago] datetime2 NOT NULL,
        [PagoId] int NOT NULL,
        [PrestamoId] int NULL,
        [ContratoId] int NULL,
        CONSTRAINT [PK_PagoDetalle] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PagoDetalle_Contrato_ContratoId] FOREIGN KEY ([ContratoId]) REFERENCES [Contrato] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PagoDetalle_Pago_PagoId] FOREIGN KEY ([PagoId]) REFERENCES [Pago] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PagoDetalle_Prestamo_PrestamoId] FOREIGN KEY ([PrestamoId]) REFERENCES [Prestamo] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE INDEX [IX_Contrato_ClienteId] ON [Contrato] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE INDEX [IX_Contrato_PropiedadId] ON [Contrato] ([PropiedadId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE INDEX [IX_Contrato_UsuarioId] ON [Contrato] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE INDEX [IX_Pago_MetodoPagoId] ON [Pago] ([MetodoPagoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE INDEX [IX_Pago_UsuarioId] ON [Pago] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE INDEX [IX_PagoDetalle_ContratoId] ON [PagoDetalle] ([ContratoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE INDEX [IX_PagoDetalle_PagoId] ON [PagoDetalle] ([PagoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE INDEX [IX_PagoDetalle_PrestamoId] ON [PagoDetalle] ([PrestamoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Permiso_Nombre] ON [Permiso] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE INDEX [IX_Prestamo_ClienteId] ON [Prestamo] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE INDEX [IX_Propiedad_TipoPropiedadId] ON [Propiedad] ([TipoPropiedadId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Rol_Nombre] ON [Rol] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE INDEX [IX_RolPermiso_PermisoId] ON [RolPermiso] ([PermisoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuario_Gmail] ON [Usuario] ([Gmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    CREATE INDEX [IX_Usuario_RolId] ON [Usuario] ([RolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930185857_Baseline_EsquemaReal'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260930185857_Baseline_EsquemaReal', N'9.0.2');
END;

COMMIT;
GO

