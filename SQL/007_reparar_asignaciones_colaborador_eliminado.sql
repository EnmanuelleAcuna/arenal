-- =============================================
-- Script: 007_reparar_asignaciones_colaborador_eliminado.sql
-- Descripción: Repara asignaciones huérfanas en PRODUCCIÓN.
--              EliminarUsuario no validaba dependencias, por lo que existen asignaciones
--              activas (IsDeleted=0) cuyo colaborador ya fue eliminado (IsDeleted=1).
--              El filtro global de soft-delete de EF deja ApplicationUser en null para esas
--              filas (LEFT JOIN por ser FK opcional) y /Clientes/Asignaciones lanza
--              NullReferenceException ("Object reference not set to an instance of an object").
--              Se marcan como eliminadas porque una asignación sin colaborador no es accionable.
--              Las sesiones históricas de colaboradores eliminados NO se tocan (son registro
--              de horas trabajadas); las vistas quedan null-safe con el deploy de este fix.
--              Idempotente: se puede re-ejecutar (el UPDATE solo afecta filas aún activas).
-- Fecha: 2026-07-09
-- =============================================

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- 1. Diagnóstico: asignaciones activas cuyo colaborador fue eliminado (las filas que rompen la página)
SELECT a.Id, a.IdColaborador, u.UserName AS Colaborador, u.DateDeleted AS ColaboradorEliminadoEl,
       p.Nombre AS Proyecto, a.HorasEstimadas, a.Descripcion
FROM Asignaciones a
    INNER JOIN Usuarios u ON u.Id = a.IdColaborador
    INNER JOIN Proyectos p ON p.Id = a.IdProyecto
WHERE a.IsDeleted = 0
  AND u.IsDeleted = 1;

-- 2. Soft-delete de esas asignaciones
UPDATE a
SET a.IsDeleted = 1,
    a.DateDeleted = GETUTCDATE(),
    a.DeletedBy = 'script-007'
FROM Asignaciones a
    INNER JOIN Usuarios u ON u.Id = a.IdColaborador
WHERE a.IsDeleted = 0
  AND u.IsDeleted = 1;

PRINT CONCAT('Asignaciones huérfanas marcadas como eliminadas: ', @@ROWCOUNT);

-- 3. Diagnóstico informativo: sesiones SIN FINALIZAR de colaboradores eliminados.
--    No se reparan aquí (decisión de negocio pendiente: nadie puede finalizarlas);
--    Estado: 1=Activa, 2=Pausada, 3=Finalizada.
SELECT s.Id, s.IdColaborador, u.UserName AS Colaborador, s.Estado, s.Fecha
FROM Sesiones s
    INNER JOIN Usuarios u ON u.Id = s.IdColaborador
WHERE u.IsDeleted = 1
  AND s.Estado <> 3
  AND ISNULL(s.IsDeleted, 0) = 0;

COMMIT TRANSACTION;
