-- Alinea la base con el modelo de dominio antes de regenerar el baseline de EF Core.
-- Ejecutar UNA sola vez sobre FinanzasDB.
-- Es seguro: Rol, RolPermiso y Usuario estan vacias.

SET NOCOUNT ON;
GO

IF EXISTS (SELECT 1 FROM sys.columns c
           JOIN sys.tables t ON t.object_id = c.object_id
           WHERE t.name = 'Rol' AND c.name = 'id' AND c.is_identity = 1)
BEGIN
    PRINT 'Rol.id ya es IDENTITY, nada que hacer.';
END
ELSE
BEGIN
    PRINT 'Recreando Rol.id como IDENTITY...';

    DECLARE @fks TABLE (
        Nombre       sysname,
        TablaHija    sysname,
        ColHija      sysname,
        DeleteAccion nvarchar(100),
        UpdateAccion nvarchar(100)
    );

    BEGIN TRANSACTION;

    -- Guardamos todas las FKs que apuntan a Rol para soltarlas y reponerlas,
    -- porque no se puede eliminar la PK mientras haya referencias.
    INSERT INTO @fks (Nombre, TablaHija, ColHija, DeleteAccion, UpdateAccion)
    SELECT fk.name, OBJECT_NAME(fk.parent_object_id), cp.name, fk.delete_referential_action_desc, fk.update_referential_action_desc
    FROM sys.foreign_keys fk
    JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
    JOIN sys.columns cp ON cp.object_id = fkc.parent_object_id AND cp.column_id = fkc.parent_column_id
    WHERE fk.referenced_object_id = OBJECT_ID('Rol');

    DECLARE @sql nvarchar(max) = N'';

    SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(TablaHija) + N' DROP CONSTRAINT ' + QUOTENAME(Nombre) + N';'
    FROM @fks;

    EXEC sp_executesql @sql;

    ALTER TABLE Rol DROP CONSTRAINT PK_Rol;
    ALTER TABLE Rol DROP COLUMN id;
    ALTER TABLE Rol ADD id INT NOT NULL IDENTITY(1, 1);
    ALTER TABLE Rol ADD CONSTRAINT PK_Rol PRIMARY KEY CLUSTERED (id);

    SET @sql = N'';

    SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(TablaHija) + N' ADD CONSTRAINT ' + QUOTENAME(Nombre)
               + N' FOREIGN KEY (' + QUOTENAME(ColHija) + N') REFERENCES Rol (id)'
               + CASE WHEN UpdateAccion <> 'NO_ACTION' THEN N' ON UPDATE ' + UpdateAccion ELSE N'' END
               + CASE WHEN DeleteAccion <> 'NO_ACTION' THEN N' ON DELETE ' + DeleteAccion ELSE N'' END
               + N';'
    FROM @fks;

    EXEC sp_executesql @sql;

    COMMIT TRANSACTION;

    PRINT 'Rol.id ahora es IDENTITY.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes i
               JOIN sys.tables t ON t.object_id = i.object_id
               WHERE t.name = 'Usuario' AND i.is_unique = 1 AND i.is_primary_key = 0)
BEGIN
    PRINT 'Creando indice unico en Usuario.Gmail...';
    CREATE UNIQUE INDEX UX_Usuario_Gmail ON Usuario (Gmail);
    PRINT 'Indice unico creado.';
END
GO

PRINT 'Listo.';
GO