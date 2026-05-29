using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using plani.Models.Data;
using plani.Models.Domain;
using plani.Models.ViewModels;

namespace plani.Models.Managers;

/// <summary>
///     Manager para la lógica de negocio de Sesiones de trabajo
/// </summary>
public class SesionesManager {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SesionesManager> _logger;
    private readonly TimeZoneInfo _zonaHoraria;

    public SesionesManager(
        ApplicationDbContext dbContext,
        ILogger<SesionesManager> logger,
        TimeZoneInfo zonaHoraria) {
        _dbContext = dbContext;
        _logger = logger;
        _zonaHoraria = zonaHoraria;
    }

    #region Exportación

    /// <summary>
    ///     Genera archivo Excel con las sesiones
    /// </summary>
    public byte[] ExportarSesionesExcel(List<Sesion> sesiones) {
        using XLWorkbook workbook = new();
        IXLWorksheet worksheet = workbook.Worksheets.Add("Sesiones");

        worksheet.Cell(row: 1, column: 1).Value = "Fecha";
        worksheet.Cell(row: 1, column: 2).Value = "Colaborador";
        worksheet.Cell(row: 1, column: 3).Value = "Cliente";
        worksheet.Cell(row: 1, column: 4).Value = "Proyecto";
        worksheet.Cell(row: 1, column: 5).Value = "Horas";
        worksheet.Cell(row: 1, column: 6).Value = "Minutos";
        worksheet.Cell(row: 1, column: 7).Value = "Estado";
        worksheet.Cell(row: 1, column: 8).Value = "Detalle";

        IXLRange headerRange = worksheet.Range(firstCellRow: 1, firstCellColumn: 1, lastCellRow: 1, lastCellColumn: 8);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
        headerRange.Style.Font.FontColor = XLColor.White;

        int row = 2;
        foreach (Sesion sesion in sesiones) {
            worksheet.Cell(row: row, column: 1).Value = sesion.FechaInicio.ToString("dd/MM/yyyy");
            worksheet.Cell(row: row, column: 2).Value = sesion.ApplicationUser?.FullName;
            worksheet.Cell(row: row, column: 3).Value = sesion.Proyecto?.Contrato?.Cliente?.Nombre;
            worksheet.Cell(row: row, column: 4).Value = sesion.Proyecto?.Nombre;
            worksheet.Cell(row: row, column: 5).Value = sesion.Horas;
            worksheet.Cell(row: row, column: 6).Value = sesion.Minutes;
            worksheet.Cell(row: row, column: 7).Value = sesion.EstadoDescripcion;
            worksheet.Cell(row: row, column: 8).Value = sesion.Descripcion;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using MemoryStream stream = new();
        workbook.SaveAs(stream: stream);
        return stream.ToArray();
    }

    #endregion

    #region Consultas

    /// <summary>
    ///     Obtiene sesiones filtradas por usuario, proyecto y rango de fechas
    /// </summary>
    public async Task<List<Sesion>> ObtenerSesionesFiltradas(
        string idUsuario = null,
        string idProyecto = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null) {
        DateTime? fechaFinAjustada = fechaFin?.Date.AddDays(value: 1).AddSeconds(value: -1);

        return await _dbContext.Sesiones
            .Where(s => (fechaInicio == null || s.FechaInicio >= fechaInicio) &&
                        (fechaFinAjustada == null || s.FechaInicio <= fechaFinAjustada) &&
                        (string.IsNullOrEmpty(idUsuario) || s.IdColaborador == idUsuario) &&
                        (string.IsNullOrEmpty(idProyecto) || s.IdProyecto.ToString() == idProyecto))
            .OrderByDescending(s => s.FechaInicio)
            .Include(s => s.ApplicationUser)
            .Include(s => s.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Include(s => s.Logs)
            .ToListAsync();
    }

    /// <summary>
    ///     Obtiene sesiones filtradas con límite opcional cuando no hay filtros
    /// </summary>
    public async Task<List<Sesion>> ObtenerSesionesFiltradasConLimite(
        string idUsuario,
        string idProyecto = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null,
        int limiteSinFiltros = 25) {
        bool tieneFiltroDeFecha = fechaInicio != null || fechaFin != null;
        bool tieneFiltrodeProyecto = !string.IsNullOrEmpty(value: idProyecto);

        if (!tieneFiltroDeFecha && !tieneFiltrodeProyecto) {
            return await ObtenerSesionesUsuario(idUsuario: idUsuario, cantidad: limiteSinFiltros);
        }

        return await ObtenerSesionesFiltradas(idUsuario: idUsuario, idProyecto: idProyecto, fechaInicio: fechaInicio, fechaFin: fechaFin);
    }

    /// <summary>
    ///     Obtiene las últimas sesiones de un usuario específico
    /// </summary>
    public async Task<List<Sesion>> ObtenerSesionesUsuario(string idUsuario, int cantidad = 25) {
        return await _dbContext.Sesiones
            .Where(s => s.IdColaborador == idUsuario)
            .OrderByDescending(s => s.FechaInicio)
            .Take(count: cantidad)
            .Include(s => s.ApplicationUser)
            .Include(s => s.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Include(s => s.Logs)
            .ToListAsync();
    }

    /// <summary>
    ///     Obtiene las sesiones activas o pausadas (sin finalizar) de un usuario
    /// </summary>
    public async Task<List<Sesion>> ObtenerSesionesActivas(string idUsuario) {
        return await _dbContext.Sesiones
            .Where(s => s.IdColaborador == idUsuario && s.Estado != EstadoSesion.Finalizada)
            .OrderByDescending(s => s.FechaInicio)
            .Include(s => s.ApplicationUser)
            .Include(s => s.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Include(s => s.Logs)
            .ToListAsync();
    }

    /// <summary>
    ///     Obtiene una sesión por ID con todos sus detalles
    /// </summary>
    public async Task<Sesion> ObtenerSesionPorId(Guid id) {
        return await _dbContext.Sesiones
            .Include(s => s.ApplicationUser)
            .Include(s => s.Servicio)
            .Include(s => s.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Include(s => s.Logs.OrderBy(l => l.Fecha))
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    /// <summary>
    ///     Cuenta las sesiones activas (en curso) de un usuario
    /// </summary>
    public async Task<int> ContarSesionesActivas(string idUsuario) {
        return await _dbContext.Sesiones
            .CountAsync(s => s.IdColaborador == idUsuario && s.Estado != EstadoSesion.Finalizada);
    }

    #endregion

    #region Operaciones CRUD

    /// <summary>
    ///     Crea una sesión manual (con fecha y horas específicas)
    /// </summary>
    public async Task<bool> CrearSesionManual(AgregarSesionModel model, string idColaborador, string userEmail) {
        DateTime fechaUtc = TimeZoneInfo.ConvertTimeToUtc(dateTime: model.Fecha, sourceTimeZone: _zonaHoraria);

        Sesion sesion = new() {
            IdColaborador = idColaborador,
            IdProyecto = model.IdProyecto,
            IdServicio = model.IdServicio,
            Horas = model.Horas,
            Minutes = model.Minutos,
            Descripcion = model.Descripcion,
            FechaInicio = fechaUtc,
            FechaFin = fechaUtc,
            Estado = EstadoSesion.Finalizada
        };

        sesion.RegristrarCreacion(creadoPor: userEmail, creadoEl: DateTime.UtcNow);
        await _dbContext.Sesiones.AddAsync(entity: sesion);

        // Crear log de inicio y finalización para sesiones manuales
        SesionLog logInicio = new(idSesion: sesion.Id, tipoEvento: TipoEventoSesion.Inicio, fecha: fechaUtc, horas: 0, minutos: 0, creadoPor: userEmail);
        SesionLog logFin = new(idSesion: sesion.Id, tipoEvento: TipoEventoSesion.Finalizacion, fecha: fechaUtc, horas: model.Horas, minutos: model.Minutos, creadoPor: userEmail);

        await _dbContext.SesionLogs.AddAsync(entity: logInicio);
        await _dbContext.SesionLogs.AddAsync(entity: logFin);

        return await _dbContext.SaveChangesAsync() > 0;
    }

    /// <summary>
    ///     Inicia una nueva sesión en tiempo real
    /// </summary>
    public async Task<(bool exito, string error)> IniciarSesion(AgregarSesionModel model, string idColaborador, string userEmail) {
        // Validar que no tenga más de 1 sesión activa
        int sesionesActivas = await ContarSesionesActivas(idUsuario: idColaborador);
        if (sesionesActivas > 1) {
            return (false, "No puede iniciar una nueva sesión si tiene dos sesiones activas.");
        }

        DateTime ahora = DateTime.UtcNow;

        Sesion sesion = new() {
            IdColaborador = idColaborador,
            IdProyecto = model.IdProyecto,
            IdServicio = model.IdServicio,
            FechaInicio = ahora,
            Horas = 0,
            Minutes = 0,
            Descripcion = model.Descripcion,
            Estado = EstadoSesion.Activa
        };

        sesion.RegristrarCreacion(creadoPor: userEmail, creadoEl: ahora);
        await _dbContext.Sesiones.AddAsync(entity: sesion);

        // Crear log de inicio
        SesionLog logInicio = new(idSesion: sesion.Id, tipoEvento: TipoEventoSesion.Inicio, fecha: ahora, horas: 0, minutos: 0, creadoPor: userEmail);
        await _dbContext.SesionLogs.AddAsync(entity: logInicio);

        bool guardado = await _dbContext.SaveChangesAsync() > 0;
        return (guardado, guardado ? null : "Error al iniciar la sesión.");
    }

    /// <summary>
    ///     Pausa una sesión activa y calcula el tiempo transcurrido
    /// </summary>
    public async Task<(bool exito, string error)> PausarSesion(Guid idSesion, string descripcion, string userEmail) {
        Sesion sesion = await _dbContext.Sesiones
            .Include(s => s.Logs)
            .FirstOrDefaultAsync(s => s.Id == idSesion);

        if (sesion == null) {
            return (false, "Sesión no encontrada.");
        }

        if (sesion.Estado != EstadoSesion.Activa) {
            return (false, "Solo puede pausar una sesión activa.");
        }

        DateTime ahora = DateTime.UtcNow;

        // Calcular tiempo transcurrido desde el último evento de inicio o reanudación
        (int horas, int minutos) = CalcularTiempoDesdeUltimoEvento(logs: sesion.Logs, fechaHasta: ahora);

        // Crear log de pausa
        SesionLog logPausa = new(idSesion: sesion.Id, tipoEvento: TipoEventoSesion.Pausa, fecha: ahora, horas: horas, minutos: minutos, creadoPor: userEmail);
        await _dbContext.SesionLogs.AddAsync(entity: logPausa);

        // Actualizar tiempo acumulado en la sesión
        AgregarTiempo(sesion: sesion, horas: horas, minutos: minutos);

        sesion.Estado = EstadoSesion.Pausada;
        sesion.Descripcion = descripcion;
        sesion.RegistrarActualizacion(actualizadoPor: userEmail, actualizadoEl: ahora);

        _dbContext.Sesiones.Update(entity: sesion);
        bool guardado = await _dbContext.SaveChangesAsync() > 0;

        return (guardado, guardado ? null : "Error al pausar la sesión.");
    }

    /// <summary>
    ///     Reanuda una sesión pausada
    /// </summary>
    public async Task<(bool exito, string error)> ReanudarSesion(Guid idSesion, string descripcion, string userEmail) {
        Sesion sesion = await _dbContext.Sesiones
            .Include(s => s.Logs)
            .FirstOrDefaultAsync(s => s.Id == idSesion);

        if (sesion == null) {
            return (false, "Sesión no encontrada.");
        }

        if (sesion.Estado != EstadoSesion.Pausada) {
            return (false, "Solo puede reanudar una sesión pausada.");
        }

        DateTime ahora = DateTime.UtcNow;

        // Crear log de reanudación (tiempo 0, solo marca el punto de reinicio)
        SesionLog logReanudacion = new(idSesion: sesion.Id, tipoEvento: TipoEventoSesion.Reanudacion, fecha: ahora, horas: 0, minutos: 0, creadoPor: userEmail);
        await _dbContext.SesionLogs.AddAsync(entity: logReanudacion);

        sesion.Estado = EstadoSesion.Activa;
        sesion.Descripcion = descripcion;
        sesion.RegistrarActualizacion(actualizadoPor: userEmail, actualizadoEl: ahora);

        _dbContext.Sesiones.Update(entity: sesion);
        bool guardado = await _dbContext.SaveChangesAsync() > 0;

        return (guardado, guardado ? null : "Error al reanudar la sesión.");
    }

    /// <summary>
    ///     Finaliza una sesión y calcula el tiempo total
    /// </summary>
    public async Task<(bool exito, string error)> FinalizarSesion(Guid idSesion, string descripcion, string userEmail) {
        Sesion sesion = await _dbContext.Sesiones
            .Include(s => s.Logs)
            .FirstOrDefaultAsync(s => s.Id == idSesion);

        if (sesion == null) {
            return (false, "Sesión no encontrada.");
        }

        if (sesion.Estado == EstadoSesion.Finalizada) {
            return (false, "La sesión ya está finalizada.");
        }

        if (sesion.Estado == EstadoSesion.Pausada) {
            return (false, "Debe reanudar la sesión antes de finalizarla.");
        }

        DateTime ahora = DateTime.UtcNow;

        // Calcular tiempo transcurrido desde el último evento de inicio o reanudación
        (int horas, int minutos) = CalcularTiempoDesdeUltimoEvento(logs: sesion.Logs, fechaHasta: ahora);

        // Crear log de finalización
        SesionLog logFin = new(idSesion: sesion.Id, tipoEvento: TipoEventoSesion.Finalizacion, fecha: ahora, horas: horas, minutos: minutos, creadoPor: userEmail);
        await _dbContext.SesionLogs.AddAsync(entity: logFin);

        // Actualizar tiempo acumulado en la sesión
        AgregarTiempo(sesion: sesion, horas: horas, minutos: minutos);

        sesion.FechaFin = ahora;
        sesion.Estado = EstadoSesion.Finalizada;
        sesion.Descripcion = descripcion;
        sesion.RegistrarActualizacion(actualizadoPor: userEmail, actualizadoEl: ahora);

        _dbContext.Sesiones.Update(entity: sesion);
        bool guardado = await _dbContext.SaveChangesAsync() > 0;

        return (guardado, guardado ? null : "Error al finalizar la sesión.");
    }

    #endregion

    #region Métodos auxiliares

    /// <summary>
    ///     Calcula el tiempo transcurrido desde el último evento de inicio o reanudación
    /// </summary>
    private (int horas, int minutos) CalcularTiempoDesdeUltimoEvento(ICollection<SesionLog> logs, DateTime fechaHasta) {
        // Buscar el último evento de inicio o reanudación
        SesionLog ultimoEventoActivo = logs
            .Where(l => l.TipoEvento == TipoEventoSesion.Inicio || l.TipoEvento == TipoEventoSesion.Reanudacion)
            .OrderByDescending(l => l.Fecha)
            .FirstOrDefault();

        if (ultimoEventoActivo == null) {
            _logger.LogWarning("No se encontró evento de inicio o reanudación para calcular tiempo");
            return (0, 0);
        }

        TimeSpan diferencia = fechaHasta - ultimoEventoActivo.Fecha;

        int horas = (int)diferencia.TotalHours;
        int minutos = diferencia.Minutes;

        return (horas, minutos);
    }

    /// <summary>
    ///     Agrega tiempo a la sesión y normaliza los minutos
    /// </summary>
    private void AgregarTiempo(Sesion sesion, int horas, int minutos) {
        sesion.Horas += horas;
        sesion.Minutes += minutos;

        // Normalizar si los minutos exceden 60
        if (sesion.Minutes >= 60) {
            sesion.Horas += sesion.Minutes / 60;
            sesion.Minutes = sesion.Minutes % 60;
        }
    }

    /// <summary>
    ///     Obtiene el rango de fechas del mes actual
    /// </summary>
    public (DateTime inicio, DateTime fin) ObtenerRangoMesActual() {
        DateTime hoy = DateTime.UtcNow.Date;
        DateTime primerDiaMes = new(year: hoy.Year, month: hoy.Month, day: 1);
        DateTime ultimoDiaMes = primerDiaMes.AddMonths(months: 1).AddDays(value: -1);

        return (primerDiaMes, ultimoDiaMes);
    }

    /// <summary>
    ///     Obtiene los logs de una sesión ordenados cronológicamente
    /// </summary>
    public async Task<List<SesionLog>> ObtenerLogsSesion(Guid idSesion) {
        return await _dbContext.SesionLogs
            .Where(l => l.IdSesion == idSesion)
            .OrderBy(l => l.Fecha)
            .ToListAsync();
    }

    #endregion
}