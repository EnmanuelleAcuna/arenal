-- =============================================
-- Script: 005_sesion_logs.sql
-- Descripción: Rediseño del sistema de sesiones con logs de auditoría
-- Fecha: 2026-02-05
-- =============================================

-- 1. Crear tabla SesionLogs
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SesionLogs')
BEGIN
    CREATE TABLE SesionLogs (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        IdSesion UNIQUEIDENTIFIER NOT NULL,
        TipoEvento INT NOT NULL, -- 1=Inicio, 2=Pausa, 3=Reanudacion, 4=Finalizacion
        Fecha DATETIME NOT NULL,
        HorasCalculadas INT NOT NULL DEFAULT 0,
        MinutosCalculados INT NOT NULL DEFAULT 0,
        CreadoPor NVARCHAR(256),
        FechaCreacion DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_SesionLogs_Sesiones FOREIGN KEY (IdSesion) REFERENCES Sesiones(Id)
    );

    CREATE INDEX IX_SesionLogs_IdSesion ON SesionLogs(IdSesion);
    CREATE INDEX IX_SesionLogs_Fecha ON SesionLogs(Fecha);

    PRINT 'Tabla SesionLogs creada exitosamente';
END
GO

-- 2. Agregar campo Estado a Sesiones (si no existe)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sesiones') AND name = 'Estado')
BEGIN
    ALTER TABLE Sesiones ADD Estado INT NOT NULL DEFAULT 1;
    PRINT 'Campo Estado agregado a Sesiones';
END
GO

-- 3. Migrar datos existentes - Establecer Estado basado en campos actuales
UPDATE Sesiones SET Estado = 3 WHERE FechaFin IS NOT NULL; -- Finalizada
UPDATE Sesiones SET Estado = 2 WHERE FechaFin IS NULL AND FechaPausa IS NOT NULL; -- Pausada
UPDATE Sesiones SET Estado = 1 WHERE FechaFin IS NULL AND FechaPausa IS NULL; -- Activa

PRINT 'Estados de sesiones migrados';
GO

-- 4. Crear logs de Inicio para todas las sesiones existentes (si no existen)
INSERT INTO SesionLogs (Id, IdSesion, TipoEvento, Fecha, HorasCalculadas, MinutosCalculados, CreadoPor, FechaCreacion)
SELECT NEWID(), s.Id, 1, s.Fecha, 0, 0, s.CreatedBy, s.DateCreated
FROM Sesiones s
WHERE NOT EXISTS (
    SELECT 1 FROM SesionLogs sl WHERE sl.IdSesion = s.Id AND sl.TipoEvento = 1
);

PRINT 'Logs de inicio creados para sesiones existentes';
GO

-- 5. Crear logs de Finalización para sesiones finalizadas (si no existen)
INSERT INTO SesionLogs (Id, IdSesion, TipoEvento, Fecha, HorasCalculadas, MinutosCalculados, CreadoPor, FechaCreacion)
SELECT NEWID(), s.Id, 4, s.FechaFin, s.Horas, s.Minutes, ISNULL(s.UpdatedBy, s.CreatedBy), ISNULL(s.DateUpdated, s.FechaFin)
FROM Sesiones s
WHERE s.FechaFin IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM SesionLogs sl WHERE sl.IdSesion = s.Id AND sl.TipoEvento = 4
);

PRINT 'Logs de finalización creados para sesiones finalizadas';
GO

-- 6. Verificar migración
SELECT
    'Sesiones por Estado' as Descripcion,
    CASE Estado
        WHEN 1 THEN 'Activa'
        WHEN 2 THEN 'Pausada'
        WHEN 3 THEN 'Finalizada'
    END as Estado,
    COUNT(*) as Cantidad
FROM Sesiones
GROUP BY Estado;

SELECT
    'Logs por Tipo' as Descripcion,
    CASE TipoEvento
        WHEN 1 THEN 'Inicio'
        WHEN 2 THEN 'Pausa'
        WHEN 3 THEN 'Reanudacion'
        WHEN 4 THEN 'Finalizacion'
    END as TipoEvento,
    COUNT(*) as Cantidad
FROM SesionLogs
GROUP BY TipoEvento;
GO

-- 7. NOTA: Las columnas FechaPausa y FechaReinicio se eliminarán en un script posterior
-- después de verificar que la migración funcionó correctamente.
-- Por ahora se mantienen para rollback si es necesario.

-- ALTER TABLE Sesiones DROP COLUMN FechaPausa;
-- ALTER TABLE Sesiones DROP COLUMN FechaReinicio;

PRINT '=== Migración completada exitosamente ===';
GO
