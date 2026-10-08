-- Registra el baseline en __EFMigrationsHistory para que EF considere el esquema
-- ya aplicado y no intente volver a crear las tablas.
-- Ejecutar DESPUES de alinear el esquema con 01_Alinear_Esquema_Base.sql
-- y solo si el modelo coincide con la base (ver doc de despliegue).

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260930185857_Baseline_EsquemaReal', N'9.0.2');