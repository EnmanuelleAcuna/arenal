-- =============================================
-- Script: 006_completar_migracion_sesiones.sql
-- Descripción: Completa la migración de sesiones en PRODUCCIÓN.
--              El 005 corrió una sola vez (~2026-02-06): creó la tabla SesionLogs, la columna
--              Estado, y backfilleó Inicio/Finalizacion para las sesiones existentes ENTONCES.
--              Desde febrero el código viejo siguió creando sesiones SIN logs y con Estado=1,
--              así que las sesiones posteriores a feb-2026 quedaron sin migrar.
--
--              Este script completa lo faltante. Necesario ANTES/junto con el deploy del código
--              nuevo (logs + máquina de estados), porque ese código filtra por Estado y calcula
--              el tiempo desde el último log de Inicio/Reanudación.
--
--              Diferencia vs 005: para sesiones EN CURSO el log de Inicio se ancla en
--              COALESCE(FechaReinicio, Fecha) — no en Fecha — para no re-contar el tramo ya
--              acumulado cuando el código nuevo finalice la sesión.
--
--              Idempotente: se puede re-ejecutar (UPDATEs deterministas + guards NOT EXISTS).
--              No duplica lo que el 005 ya backfilleó.
-- Fecha: 2026-05-29
-- =============================================

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- 1. Alinear Estado según los campos existentes (FechaFin / FechaPausa)
--    Finalizada=3, Pausada=2, Activa=1
UPDATE Sesiones SET Estado = 3 WHERE FechaFin IS NOT NULL AND Estado <> 3;
UPDATE Sesiones SET Estado = 2 WHERE FechaFin IS NULL AND FechaPausa IS NOT NULL AND Estado <> 2;
UPDATE Sesiones SET Estado = 1 WHERE FechaFin IS NULL AND FechaPausa IS NULL AND Estado <> 1;

-- 2. Log de Inicio para sesiones FINALIZADAS sin él (solo auditoría; no se recalculan).
--    Se ancla en Fecha (inicio real de la sesión).
INSERT INTO SesionLogs (Id, IdSesion, TipoEvento, Fecha, HorasCalculadas, MinutosCalculados, CreadoPor, FechaCreacion)
SELECT NEWID(), s.Id, 1 /*Inicio*/, s.Fecha, 0, 0, s.CreatedBy, s.DateCreated
FROM Sesiones s
WHERE s.FechaFin IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM SesionLogs l WHERE l.IdSesion = s.Id AND l.TipoEvento = 1);

-- 3. Log de Finalización para sesiones FINALIZADAS sin él, con las horas/minutos ya guardados.
--    (Se registra el valor histórico tal cual; el inflado histórico no se corrige aquí.)
INSERT INTO SesionLogs (Id, IdSesion, TipoEvento, Fecha, HorasCalculadas, MinutosCalculados, CreadoPor, FechaCreacion)
SELECT NEWID(), s.Id, 4 /*Finalizacion*/, s.FechaFin, s.Horas, s.Minutes,
       ISNULL(s.UpdatedBy, s.CreatedBy), ISNULL(s.DateUpdated, s.FechaFin)
FROM Sesiones s
WHERE s.FechaFin IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM SesionLogs l WHERE l.IdSesion = s.Id AND l.TipoEvento = 4);

-- 4. Log de Inicio para sesiones EN CURSO (Activa o Pausada) sin ningún log.
--    CLAVE: se ancla en COALESCE(FechaReinicio, Fecha) para marcar el inicio del tramo que
--    todavía NO está contado en Horas/Minutes. Así, cuando el código nuevo finalice la sesión,
--    sumará SOLO ese tramo y no re-contará lo ya acumulado.
INSERT INTO SesionLogs (Id, IdSesion, TipoEvento, Fecha, HorasCalculadas, MinutosCalculados, CreadoPor, FechaCreacion)
SELECT NEWID(), s.Id, 1 /*Inicio*/, COALESCE(s.FechaReinicio, s.Fecha), 0, 0, s.CreatedBy, s.DateCreated
FROM Sesiones s
WHERE s.FechaFin IS NULL
  AND NOT EXISTS (SELECT 1 FROM SesionLogs l WHERE l.IdSesion = s.Id);

COMMIT;

-- =============================================
-- Verificación (read-only)
-- =============================================

-- Sesiones por estado
SELECT CASE Estado WHEN 1 THEN 'Activa' WHEN 2 THEN 'Pausada' WHEN 3 THEN 'Finalizada' ELSE 'Otro' END AS Estado,
       COUNT(*) AS Cantidad
FROM Sesiones GROUP BY Estado;

-- Logs por tipo
SELECT CASE TipoEvento WHEN 1 THEN 'Inicio' WHEN 2 THEN 'Pausa' WHEN 3 THEN 'Reanudacion' WHEN 4 THEN 'Finalizacion' ELSE 'Otro' END AS TipoEvento,
       COUNT(*) AS Cantidad
FROM SesionLogs GROUP BY TipoEvento;

-- Sanity: ninguna sesión en curso debe quedar sin log de Inicio
SELECT COUNT(*) AS EnCursoSinLog
FROM Sesiones s
WHERE s.FechaFin IS NULL AND NOT EXISTS (SELECT 1 FROM SesionLogs l WHERE l.IdSesion = s.Id);

-- Sanity: ninguna sesión finalizada debe quedar sin log de Finalización
SELECT COUNT(*) AS FinalizadasSinLogFin
FROM Sesiones s
WHERE s.FechaFin IS NOT NULL AND NOT EXISTS (SELECT 1 FROM SesionLogs l WHERE l.IdSesion = s.Id AND l.TipoEvento = 4);
