using Microsoft.EntityFrameworkCore;
using plani.Models.Data;
using plani.Models.Domain;
using plani.Models.ViewModels;

namespace plani.Models.Managers;

/// <summary>
/// Manager para la lógica de negocio de Sesiones de trabajo
/// </summary>
public class SesionesManager
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SesionesManager> _logger;

    public SesionesManager(ApplicationDbContext dbContext, ILogger<SesionesManager> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Consultas

    /// <summary>
    /// Obtiene sesiones filtradas por usuario, proyecto y rango de fechas
    /// </summary>
    public async Task<List<Sesion>> ObtenerSesionesFiltradas(
        string idUsuario = null,
        string idProyecto = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null)
    {
        var fechaFinAjustada = fechaFin?.Date.AddDays(1).AddSeconds(-1);

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
    /// Obtiene sesiones filtradas con límite opcional cuando no hay filtros
    /// </summary>
    public async Task<List<Sesion>> ObtenerSesionesFiltradasConLimite(
        string idUsuario,
        string idProyecto = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null,
        int limiteSinFiltros = 25)
    {
        var tieneFiltroDeFecha = fechaInicio != null || fechaFin != null;
        var tieneFiltrodeProyecto = !string.IsNullOrEmpty(idProyecto);

        if (!tieneFiltroDeFecha && !tieneFiltrodeProyecto)
        {
            return await ObtenerSesionesUsuario(idUsuario, limiteSinFiltros);
        }

        return await ObtenerSesionesFiltradas(idUsuario, idProyecto, fechaInicio, fechaFin);
    }

    /// <summary>
    /// Obtiene las últimas sesiones de un usuario específico
    /// </summary>
    public async Task<List<Sesion>> ObtenerSesionesUsuario(string idUsuario, int cantidad = 25)
    {
        return await _dbContext.Sesiones
            .Where(s => s.IdColaborador == idUsuario)
            .OrderByDescending(s => s.FechaInicio)
            .Take(cantidad)
            .Include(s => s.ApplicationUser)
            .Include(s => s.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Include(s => s.Logs)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene las sesiones activas o pausadas (sin finalizar) de un usuario
    /// </summary>
    public async Task<List<Sesion>> ObtenerSesionesActivas(string idUsuario)
    {
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
    /// Obtiene una sesión por ID con todos sus detalles
    /// </summary>
    public async Task<Sesion> ObtenerSesionPorId(Guid id)
    {
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
    /// Cuenta las sesiones activas (en curso) de un usuario
    /// </summary>
    public async Task<int> ContarSesionesActivas(string idUsuario)
    {
        return await _dbContext.Sesiones
            .CountAsync(s => s.IdColaborador == idUsuario && s.Estado != EstadoSesion.Finalizada);
    }

    #endregion

    #region Operaciones CRUD

    /// <summary>
    /// Crea una sesión manual (con fecha y horas específicas)
    /// </summary>
    public async Task<bool> CrearSesionManual(AgregarSesionModel model, string idColaborador, string userEmail)
    {
        var fechaUtc = model.Fecha.AddHours(6);

        var sesion = new Sesion
        {
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

        sesion.RegristrarCreacion(userEmail, DateTime.UtcNow);
        await _dbContext.Sesiones.AddAsync(sesion);

        // Crear log de inicio y finalización para sesiones manuales
        var logInicio = new SesionLog(sesion.Id, TipoEventoSesion.Inicio, fechaUtc, 0, 0, userEmail);
        var logFin = new SesionLog(sesion.Id, TipoEventoSesion.Finalizacion, fechaUtc, model.Horas, model.Minutos, userEmail);

        await _dbContext.SesionLogs.AddAsync(logInicio);
        await _dbContext.SesionLogs.AddAsync(logFin);

        return await _dbContext.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// Inicia una nueva sesión en tiempo real
    /// </summary>
    public async Task<(bool exito, string error)> IniciarSesion(AgregarSesionModel model, string idColaborador, string userEmail)
    {
        // Validar que no tenga más de 1 sesión activa
        var sesionesActivas = await ContarSesionesActivas(idColaborador);
        if (sesionesActivas > 1)
        {
            return (false, "No puede iniciar una nueva sesión si tiene dos sesiones activas.");
        }

        var ahora = DateTime.UtcNow;

        var sesion = new Sesion
        {
            IdColaborador = idColaborador,
            IdProyecto = model.IdProyecto,
            IdServicio = model.IdServicio,
            FechaInicio = ahora,
            Horas = 0,
            Minutes = 0,
            Descripcion = model.Descripcion,
            Estado = EstadoSesion.Activa
        };

        sesion.RegristrarCreacion(userEmail, ahora);
        await _dbContext.Sesiones.AddAsync(sesion);

        // Crear log de inicio
        var logInicio = new SesionLog(sesion.Id, TipoEventoSesion.Inicio, ahora, 0, 0, userEmail);
        await _dbContext.SesionLogs.AddAsync(logInicio);

        var guardado = await _dbContext.SaveChangesAsync() > 0;
        return (guardado, guardado ? null : "Error al iniciar la sesión.");
    }

    /// <summary>
    /// Pausa una sesión activa y calcula el tiempo transcurrido
    /// </summary>
    public async Task<(bool exito, string error)> PausarSesion(Guid idSesion, string descripcion, string userEmail)
    {
        var sesion = await _dbContext.Sesiones
            .Include(s => s.Logs)
            .FirstOrDefaultAsync(s => s.Id == idSesion);

        if (sesion == null)
            return (false, "Sesión no encontrada.");

        if (sesion.Estado != EstadoSesion.Activa)
            return (false, "Solo puede pausar una sesión activa.");

        var ahora = DateTime.UtcNow;

        // Calcular tiempo transcurrido desde el último evento de inicio o reanudación
        var (horas, minutos) = CalcularTiempoDesdeUltimoEvento(sesion.Logs, ahora);

        // Crear log de pausa
        var logPausa = new SesionLog(sesion.Id, TipoEventoSesion.Pausa, ahora, horas, minutos, userEmail);
        await _dbContext.SesionLogs.AddAsync(logPausa);

        // Actualizar tiempo acumulado en la sesión
        AgregarTiempo(sesion, horas, minutos);

        sesion.Estado = EstadoSesion.Pausada;
        sesion.Descripcion = descripcion;
        sesion.RegistrarActualizacion(userEmail, ahora);

        _dbContext.Sesiones.Update(sesion);
        var guardado = await _dbContext.SaveChangesAsync() > 0;

        return (guardado, guardado ? null : "Error al pausar la sesión.");
    }

    /// <summary>
    /// Reanuda una sesión pausada
    /// </summary>
    public async Task<(bool exito, string error)> ReanudarSesion(Guid idSesion, string descripcion, string userEmail)
    {
        var sesion = await _dbContext.Sesiones
            .Include(s => s.Logs)
            .FirstOrDefaultAsync(s => s.Id == idSesion);

        if (sesion == null)
            return (false, "Sesión no encontrada.");

        if (sesion.Estado != EstadoSesion.Pausada)
            return (false, "Solo puede reanudar una sesión pausada.");

        var ahora = DateTime.UtcNow;

        // Crear log de reanudación (tiempo 0, solo marca el punto de reinicio)
        var logReanudacion = new SesionLog(sesion.Id, TipoEventoSesion.Reanudacion, ahora, 0, 0, userEmail);
        await _dbContext.SesionLogs.AddAsync(logReanudacion);

        sesion.Estado = EstadoSesion.Activa;
        sesion.Descripcion = descripcion;
        sesion.RegistrarActualizacion(userEmail, ahora);

        _dbContext.Sesiones.Update(sesion);
        var guardado = await _dbContext.SaveChangesAsync() > 0;

        return (guardado, guardado ? null : "Error al reanudar la sesión.");
    }

    /// <summary>
    /// Finaliza una sesión y calcula el tiempo total
    /// </summary>
    public async Task<(bool exito, string error)> FinalizarSesion(Guid idSesion, string descripcion, string userEmail)
    {
        var sesion = await _dbContext.Sesiones
            .Include(s => s.Logs)
            .FirstOrDefaultAsync(s => s.Id == idSesion);

        if (sesion == null)
            return (false, "Sesión no encontrada.");

        if (sesion.Estado == EstadoSesion.Finalizada)
            return (false, "La sesión ya está finalizada.");

        if (sesion.Estado == EstadoSesion.Pausada)
            return (false, "Debe reanudar la sesión antes de finalizarla.");

        var ahora = DateTime.UtcNow;

        // Calcular tiempo transcurrido desde el último evento de inicio o reanudación
        var (horas, minutos) = CalcularTiempoDesdeUltimoEvento(sesion.Logs, ahora);

        // Crear log de finalización
        var logFin = new SesionLog(sesion.Id, TipoEventoSesion.Finalizacion, ahora, horas, minutos, userEmail);
        await _dbContext.SesionLogs.AddAsync(logFin);

        // Actualizar tiempo acumulado en la sesión
        AgregarTiempo(sesion, horas, minutos);

        sesion.FechaFin = ahora;
        sesion.Estado = EstadoSesion.Finalizada;
        sesion.Descripcion = descripcion;
        sesion.RegistrarActualizacion(userEmail, ahora);

        _dbContext.Sesiones.Update(sesion);
        var guardado = await _dbContext.SaveChangesAsync() > 0;

        return (guardado, guardado ? null : "Error al finalizar la sesión.");
    }

    #endregion

    #region Métodos auxiliares

    /// <summary>
    /// Calcula el tiempo transcurrido desde el último evento de inicio o reanudación
    /// </summary>
    private (int horas, int minutos) CalcularTiempoDesdeUltimoEvento(ICollection<SesionLog> logs, DateTime fechaHasta)
    {
        // Buscar el último evento de inicio o reanudación
        var ultimoEventoActivo = logs
            .Where(l => l.TipoEvento == TipoEventoSesion.Inicio || l.TipoEvento == TipoEventoSesion.Reanudacion)
            .OrderByDescending(l => l.Fecha)
            .FirstOrDefault();

        if (ultimoEventoActivo == null)
        {
            _logger.LogWarning("No se encontró evento de inicio o reanudación para calcular tiempo");
            return (0, 0);
        }

        TimeSpan diferencia = fechaHasta - ultimoEventoActivo.Fecha;

        int horas = (int)diferencia.TotalHours;
        int minutos = diferencia.Minutes;

        return (horas, minutos);
    }

    /// <summary>
    /// Agrega tiempo a la sesión y normaliza los minutos
    /// </summary>
    private void AgregarTiempo(Sesion sesion, int horas, int minutos)
    {
        sesion.Horas += horas;
        sesion.Minutes += minutos;

        // Normalizar si los minutos exceden 60
        if (sesion.Minutes >= 60)
        {
            sesion.Horas += sesion.Minutes / 60;
            sesion.Minutes = sesion.Minutes % 60;
        }
    }

    /// <summary>
    /// Obtiene el rango de fechas del mes actual
    /// </summary>
    public (DateTime inicio, DateTime fin) ObtenerRangoMesActual()
    {
        var hoy = DateTime.UtcNow.Date;
        var primerDiaMes = new DateTime(hoy.Year, hoy.Month, 1);
        var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

        return (primerDiaMes, ultimoDiaMes);
    }

    /// <summary>
    /// Obtiene los logs de una sesión ordenados cronológicamente
    /// </summary>
    public async Task<List<SesionLog>> ObtenerLogsSesion(Guid idSesion)
    {
        return await _dbContext.SesionLogs
            .Where(l => l.IdSesion == idSesion)
            .OrderBy(l => l.Fecha)
            .ToListAsync();
    }

    #endregion

    #region Exportación

    /// <summary>
    /// Genera archivo Excel con las sesiones
    /// </summary>
    public byte[] ExportarSesionesExcel(List<Sesion> sesiones)
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sesiones");

        worksheet.Cell(1, 1).Value = "Fecha";
        worksheet.Cell(1, 2).Value = "Colaborador";
        worksheet.Cell(1, 3).Value = "Cliente";
        worksheet.Cell(1, 4).Value = "Proyecto";
        worksheet.Cell(1, 5).Value = "Horas";
        worksheet.Cell(1, 6).Value = "Minutos";
        worksheet.Cell(1, 7).Value = "Estado";
        worksheet.Cell(1, 8).Value = "Detalle";

        var headerRange = worksheet.Range(1, 1, 1, 8);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#1e3a5f");
        headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;

        int row = 2;
        foreach (var sesion in sesiones)
        {
            worksheet.Cell(row, 1).Value = sesion.FechaInicio.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 2).Value = sesion.ApplicationUser?.FullName;
            worksheet.Cell(row, 3).Value = sesion.Proyecto?.Contrato?.Cliente?.Nombre;
            worksheet.Cell(row, 4).Value = sesion.Proyecto?.Nombre;
            worksheet.Cell(row, 5).Value = sesion.Horas;
            worksheet.Cell(row, 6).Value = sesion.Minutes;
            worksheet.Cell(row, 7).Value = sesion.EstadoDescripcion;
            worksheet.Cell(row, 8).Value = sesion.Descripcion;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion
}
