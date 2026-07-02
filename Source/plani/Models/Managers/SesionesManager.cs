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

        Sesion sesion = Sesion.CrearManual(
            idProyecto: model.IdProyecto, idServicio: model.IdServicio,
            horas: model.Horas, minutos: model.Minutos, descripcion: model.Descripcion,
            idColaborador: idColaborador, usuario: userEmail, fecha: fechaUtc, ahora: DateTime.UtcNow);

        await _dbContext.Sesiones.AddAsync(entity: sesion);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    /// <summary>
    ///     Inicia una nueva sesión en tiempo real
    /// </summary>
    public async Task<(bool exito, string error)> IniciarSesion(AgregarSesionModel model, string idColaborador, string userEmail) {
        // Validar que no tenga más de 1 sesión activa (regla cross-entity: vive en el manager)
        int sesionesActivas = await ContarSesionesActivas(idUsuario: idColaborador);
        if (sesionesActivas > 1) {
            return (false, "No puede iniciar una nueva sesión si tiene dos sesiones activas.");
        }

        Sesion sesion = Sesion.Iniciar(
            idProyecto: model.IdProyecto, idServicio: model.IdServicio,
            descripcion: model.Descripcion, idColaborador: idColaborador,
            usuario: userEmail, ahora: DateTime.UtcNow);

        await _dbContext.Sesiones.AddAsync(entity: sesion);
        bool guardado = await _dbContext.SaveChangesAsync() > 0;
        return (guardado, guardado ? null : "Error al iniciar la sesión.");
    }

    /// <summary>
    ///     Pausa una sesión activa. La entidad valida la transición y calcula el tiempo del tramo.
    /// </summary>
    public async Task<(bool exito, string error)> PausarSesion(Guid idSesion, string descripcion, string userEmail) {
        Sesion sesion = await _dbContext.Sesiones
            .Include(s => s.Logs)
            .FirstOrDefaultAsync(s => s.Id == idSesion);

        if (sesion == null) {
            return (false, "Sesión no encontrada.");
        }

        try {
            sesion.Pausar(descripcion: descripcion, usuario: userEmail, ahora: DateTime.UtcNow);
        }
        catch (InvalidOperationException ex) {
            _logger.LogWarning(exception: ex, "Transición inválida al pausar la sesión {IdSesion}", idSesion);
            return (false, ex.Message);
        }

        // La sesión ya viene trackeada: el cambio de estado y el log nuevo se detectan en SaveChanges.
        bool guardado = await _dbContext.SaveChangesAsync() > 0;
        return (guardado, guardado ? null : "Error al pausar la sesión.");
    }

    /// <summary>
    ///     Reanuda una sesión pausada. La entidad valida la transición.
    /// </summary>
    public async Task<(bool exito, string error)> ReanudarSesion(Guid idSesion, string descripcion, string userEmail) {
        Sesion sesion = await _dbContext.Sesiones
            .Include(s => s.Logs)
            .FirstOrDefaultAsync(s => s.Id == idSesion);

        if (sesion == null) {
            return (false, "Sesión no encontrada.");
        }

        try {
            sesion.Reanudar(descripcion: descripcion, usuario: userEmail, ahora: DateTime.UtcNow);
        }
        catch (InvalidOperationException ex) {
            _logger.LogWarning(exception: ex, "Transición inválida al reanudar la sesión {IdSesion}", idSesion);
            return (false, ex.Message);
        }

        bool guardado = await _dbContext.SaveChangesAsync() > 0;
        return (guardado, guardado ? null : "Error al reanudar la sesión.");
    }

    /// <summary>
    ///     Finaliza una sesión. La entidad valida la transición y calcula el tiempo del último tramo.
    /// </summary>
    public async Task<(bool exito, string error)> FinalizarSesion(Guid idSesion, string descripcion, string userEmail) {
        Sesion sesion = await _dbContext.Sesiones
            .Include(s => s.Logs)
            .FirstOrDefaultAsync(s => s.Id == idSesion);

        if (sesion == null) {
            return (false, "Sesión no encontrada.");
        }

        try {
            sesion.Finalizar(descripcion: descripcion, usuario: userEmail, ahora: DateTime.UtcNow);
        }
        catch (InvalidOperationException ex) {
            _logger.LogWarning(exception: ex, "Transición inválida al finalizar la sesión {IdSesion}", idSesion);
            return (false, ex.Message);
        }

        bool guardado = await _dbContext.SaveChangesAsync() > 0;
        return (guardado, guardado ? null : "Error al finalizar la sesión.");
    }

    #endregion

    #region Métodos auxiliares

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