# Plan: Rediseño del sistema de sesiones con logs de auditoría

## Problema actual

El sistema actual tiene campos `FechaPausa` y `FechaReinicio` en la tabla `Sesiones` que se sobrescriben en cada pausa/reanudación, perdiendo el historial. Esto causa cálculos incorrectos de horas cuando hay múltiples pausas/reanudaciones.

**Bug reportado**: Sesión con 13 horas registradas cuando el tiempo total entre inicio y fin fue ~8 horas.

---

## 1. Modelo de datos

### Tabla `Sesiones` (modificar)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | Guid | PK |
| `IdProyecto` | Guid | FK |
| `IdColaborador` | string | FK |
| `IdServicio` | Guid | FK |
| `Descripcion` | string | |
| `FechaInicio` | DateTime | Cuando se inició la sesión |
| `FechaFin` | DateTime? | Cuando se finalizó (null si activa/pausada) |
| `Horas` | int | Total acumulado |
| `Minutes` | int | Total acumulado |
| `Estado` | int | 1=Activa, 2=Pausada, 3=Finalizada |
| Campos auditoría | ... | CreatedBy, DateCreated, etc. |

**Eliminar**: `FechaPausa`, `FechaReinicio`

### Tabla `SesionLogs` (nueva)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | Guid | PK |
| `IdSesion` | Guid | FK a Sesiones |
| `TipoEvento` | int | 1=Inicio, 2=Pausa, 3=Reanudacion, 4=Finalizacion |
| `Fecha` | DateTime | UTC |
| `HorasCalculadas` | int | Tiempo desde evento anterior |
| `MinutosCalculados` | int | Tiempo desde evento anterior |
| `CreadoPor` | string | Email del usuario |
| `FechaCreacion` | DateTime | UTC |

---

## 2. Lógica de cálculo por evento

| Evento | Horas/Minutos en Log | Acción en Sesión |
|--------|---------------------|------------------|
| **Inicio** | 0, 0 | Crear con Estado=Activa |
| **Pausa** | Calcular desde Inicio o última Reanudación | Sumar a Horas/Minutes, Estado=Pausada |
| **Reanudación** | 0, 0 | Estado=Activa |
| **Finalización** | Calcular desde Inicio o última Reanudación (si no pausada) | Sumar a Horas/Minutes, Estado=Finalizada, FechaFin=now |

### Ejemplo de flujo

```
08:00 - Inicio      → Log(Inicio, 0h 0m)      → Sesion(Activa, 0h 0m)
10:00 - Pausa       → Log(Pausa, 2h 0m)       → Sesion(Pausada, 2h 0m)
10:30 - Reanudación → Log(Reanudacion, 0h 0m) → Sesion(Activa, 2h 0m)
12:30 - Pausa       → Log(Pausa, 2h 0m)       → Sesion(Pausada, 4h 0m)
14:00 - Reanudación → Log(Reanudacion, 0h 0m) → Sesion(Activa, 4h 0m)
18:00 - Finalizar   → Log(Finalizacion, 4h 0m)→ Sesion(Finalizada, 8h 0m)
```

---

## 3. Validaciones

### Backend (SesionesManager)

| Método | Validación |
|--------|------------|
| `IniciarSesion` | No permitir si ya tiene 2+ sesiones activas |
| `PausarSesion` | Solo si Estado = Activa |
| `ReanudarSesion` | Solo si Estado = Pausada |
| `FinalizarSesion` | Solo si Estado = Activa (**NO** permitir finalizar pausada) |

### Frontend

| Estado | Botones visibles |
|--------|------------------|
| Activa | Pausar, Finalizar |
| Pausada | Reanudar |
| Finalizada | Ninguno (solo ver detalle) |

Si está pausada y el usuario intenta finalizar → Mostrar mensaje: "Debe reanudar la sesión antes de finalizarla"

---

## 4. Archivos a modificar/crear

### SQL
- [ ] `SQL/005_sesion_logs.sql` - Crear tabla SesionLogs, agregar campo Estado, eliminar FechaPausa/FechaReinicio, migrar datos

### Backend - Entidades
- [ ] `Models/Domain/SesionLog.cs` - Nueva entidad
- [ ] `Models/Domain/EstadoSesion.cs` - Enum (Activa=1, Pausada=2, Finalizada=3)
- [ ] `Models/Domain/TipoEventoSesion.cs` - Enum (Inicio=1, Pausa=2, Reanudacion=3, Finalizacion=4)
- [ ] `Models/Domain/ClientesModels.cs` - Modificar entidad Sesion (agregar Estado, eliminar FechaPausa/FechaReinicio)

### Backend - Data
- [ ] `Models/Data/ApplicationDbContext.cs` - Agregar DbSet<SesionLog>

### Backend - Lógica
- [ ] `Models/Managers/SesionesManager.cs` - Reescribir lógica completa

### Frontend - Vistas
- [ ] `Views/Clientes/MisSesiones.cshtml` - Ajustar botones según Estado
- [ ] `Views/Clientes/Sesiones.cshtml` - Mostrar Estado

### Tests
- [ ] `plani.Tests/SesionesManagerTests.cs` - Actualizar tests con nueva lógica

---

## 5. Script SQL de migración

```sql
-- 1. Crear tabla SesionLogs
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

-- 2. Agregar campo Estado a Sesiones
ALTER TABLE Sesiones ADD Estado INT NOT NULL DEFAULT 1;

-- 3. Migrar datos existentes - Establecer Estado
UPDATE Sesiones SET Estado = 3 WHERE FechaFin IS NOT NULL; -- Finalizada
UPDATE Sesiones SET Estado = 2 WHERE FechaFin IS NULL AND FechaPausa IS NOT NULL; -- Pausada
UPDATE Sesiones SET Estado = 1 WHERE FechaFin IS NULL AND FechaPausa IS NULL; -- Activa

-- 4. Crear logs de Inicio para todas las sesiones existentes
INSERT INTO SesionLogs (Id, IdSesion, TipoEvento, Fecha, HorasCalculadas, MinutosCalculados, CreadoPor, FechaCreacion)
SELECT NEWID(), Id, 1, Fecha, 0, 0, CreatedBy, DateCreated
FROM Sesiones;

-- 5. Crear logs de Finalización para sesiones finalizadas
INSERT INTO SesionLogs (Id, IdSesion, TipoEvento, Fecha, HorasCalculadas, MinutosCalculados, CreadoPor, FechaCreacion)
SELECT NEWID(), Id, 4, FechaFin, Horas, Minutes, UpdatedBy, ISNULL(DateUpdated, FechaFin)
FROM Sesiones
WHERE FechaFin IS NOT NULL;

-- 6. Eliminar columnas obsoletas (DESPUÉS de verificar migración)
-- ALTER TABLE Sesiones DROP COLUMN FechaPausa;
-- ALTER TABLE Sesiones DROP COLUMN FechaReinicio;
```

---

## 6. Orden de implementación

1. **SQL**: Ejecutar script de migración en desarrollo
2. **Entidades**: Crear enums y SesionLog, modificar Sesion
3. **DbContext**: Agregar DbSet y configuración
4. **SesionesManager**: Reescribir lógica
5. **Tests**: Actualizar y verificar
6. **Vistas**: Ajustar frontend
7. **Pruebas manuales**: Verificar flujo completo
8. **SQL Producción**: Ejecutar migración
9. **Deploy**

---

## 7. Rollback

En caso de problemas:
- Los campos `FechaPausa` y `FechaReinicio` no se eliminan hasta verificar que todo funciona
- La tabla `SesionLogs` puede eliminarse si es necesario
- El campo `Estado` puede recalcularse desde FechaPausa/FechaFin
