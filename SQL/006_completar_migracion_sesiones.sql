-- =============================================
-- Script: 006_completar_migracion_sesiones.sql
-- Descripción: Completa la migración de sesiones en PRODUCCIÓN.
--              El 005 creó la tabla SesionLogs y la columna Estado, pero los pasos
--              de datos (migrar Estado + backfill de logs) nunca corrieron en prod
--              (todas las sesiones quedaron en Estado=1 y sin logs).
--              Necesario ANTES/junto con el deploy del código nuevo (logs + máquina de estados),
--              porque ese código filtra por Estado y calcula tiempo desde el último log.
--
--              Diferencia vs 005: para sesiones EN CURSO el log de Inicio se crea en
--              COALESCE(FechaReinicio, Fecha) — no en Fecha — para no re-contar el tramo
--              ya acumulado cuando el código nuevo finalice la sesión.
--
--              Idempotente: se puede re-ejecutar (usa NOT EXISTS y UPDATEs deterministas).
-- Fecha: 2026-05-29
-- =============================================

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- 1. Alinear Estado según los campos existentes (FechaFin / FechaPausa)
--    Finalizada=3, Pausada=2, Activa=1
UPDATE Sesiones SET Estado = 3 WHERE FechaFin IS NOT NULL AND Estado <> 3;
UPDATE Sesiones SET Estado = 2 WHERE FechaFin IS NULL AND FechaPausa IS NOT NULL AND Estado <> 2;
UPDATE Sesiones SET Estado = 1 WHERE FechaFin IS NULL AND FechaPausa IS NULL AND Estado <> 1;

-- 2. Log de Inicio para sesiones FINALIZADAS (solo auditoría; no se recalculan).
--    Se ancla en Fecha (inicio real de la sesión).
INSERT INTO SesionLogs (Id, IdSesion, TipoEvento, Fecha, HorasCalculadas, MinutosCalculados, CreadoPor, FechaCreacion)
SELECT NEWID(), s.Id, 1 /*Inicio*/, s.Fecha, 0, 0, s.CreatedBy, s.DateCreated
FROM Sesiones s
WHERE s.FechaFin IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM SesionLogs l WHERE l.IdSesion = s.Id AND l.TipoEvento = 1);

-- 3. Log de Finalización para sesiones FINALIZADAS, con las horas/minutos ya guardados.
--    (Se registra el valor histórico tal cual; no corregimos el inflado aquí.)
INSERT INTO SesionLogs (Id, IdSesion, TipoEvento, Fecha, HorasCalculadas, MinutosCalculados, CreadoPor, FechaCreacion)
SELECT NEWID(), s.Id, 4 /*Finalizacion*/, s.FechaFin, s.Horas, s.Minutes,
       ISNULL(s.UpdatedBy, s.CreatedBy), ISNULL(s.DateUpdated, s.FechaFin)
FROM Sesiones s
WHERE s.FechaFin IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM SesionLogs l WHERE l.IdSesion = s.Id AND l.TipoEvento = 4);

-- 4. Log de Inicio para sesiones EN CURSO (Activa o Pausada).
--    CLAVE: se ancla en COALESCE(FechaReinicio, Fecha) para marcar el inicio del tramo
--    que todavía NO está contado en Horas/Minutes. Así, cuando el código nuevo finalice,
--    sumará SOLO ese tramo y no re-contará lo ya acumulado.
INSERT INTO SesionLogs (Id, IdSesion, TipoEvento, Fecha, HorasCalculadas, MinutosCalculados, CreadoPor, FechaCreacion)
SELECT NEWID(), s.Id, 1 /*Inicio*/, COALESCE(s.FechaReinicio, s.Fecha), 0, 0, s.CreatedBy, s.DateCreated
FROM Sesiones s
WHERE s.FechaFin IS NULL
  AND NOT EXISTS (SELECT 1 FROM SesionLogs l WHERE l.IdSesion = s.Id);

COMMIT;

-- 5. Verificación
SELECT CASE Estado WHEN 1 THEN 'Activa' WHEN 2 THEN 'Pausada' WHEN 3 THEN 'Finalizada' END AS Estado,
       COUNT(*) AS Cantidad
FROM Sesiones GROUP BY Estado;

SELECT CASE TipoEvento WHEN 1 THEN 'Inicio' WHEN 4 THEN 'Finalizacion' ELSE 'Otro' END AS TipoEvento,
       COUNT(*) AS Cantidad
FROM SesionLogs GROUP BY TipoEvento;

-- Sanity: ninguna sesión en curso debe quedar sin log
SELECT COUNT(*) AS EnCursoSinLog
FROM Sesiones s
WHERE s.FechaFin IS NULL AND NOT EXISTS (SELECT 1 FROM SesionLogs l WHERE l.IdSesion = s.Id);
