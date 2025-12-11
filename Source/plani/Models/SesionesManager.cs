using Microsoft.EntityFrameworkCore;
using plani.Models.Data;

namespace plani.Models;

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
            .ToListAsync();
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
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene las sesiones activas (sin FechaFin) de un usuario
    /// </summary>
    public async Task<List<Sesion>> ObtenerSesionesActivas(string idUsuario)
    {
        return await _dbContext.Sesiones
            .Where(s => s.IdColaborador == idUsuario && s.FechaFin == null)
            .OrderByDescending(s => s.FechaInicio)
            .Include(s => s.ApplicationUser)
            .Include(s => s.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
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
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    /// <summary>
    /// Cuenta las sesiones activas de un usuario
    /// </summary>
    public async Task<int> ContarSesionesActivas(string idUsuario)
    {
        return await _dbContext.Sesiones
            .CountAsync(s => s.IdColaborador == idUsuario && s.FechaFin == null);
    }

    #endregion

    #region Operaciones CRUD

    /// <summary>
    /// Crea una sesión manual (con fecha y horas específicas)
    /// </summary>
    public async Task<bool> CrearSesionManual(AgregarSesionModel model, string idColaborador, string userEmail)
    {
        var sesion = new Sesion
        {
            IdColaborador = idColaborador,
            IdProyecto = model.IdProyecto,
            IdServicio = model.IdServicio,
            Horas = model.Horas,
            Minutes = model.Minutos,
            Descripcion = model.Descripcion,
            FechaInicio = model.Fecha.AddHours(6),
            FechaFin = model.Fecha.AddHours(6)
        };

        sesion.RegristrarCreacion(userEmail, DateTime.UtcNow);
        await _dbContext.Sesiones.AddAsync(sesion);

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

        var sesion = new Sesion
        {
            IdColaborador = idColaborador,
            IdProyecto = model.IdProyecto,
            IdServicio = model.IdServicio,
            FechaInicio = DateTime.UtcNow,
            Horas = 0,
            Minutes = 0,
            Descripcion = model.Descripcion
        };

        sesion.RegristrarCreacion(userEmail, DateTime.UtcNow);
        await _dbContext.Sesiones.AddAsync(sesion);

        var guardado = await _dbContext.SaveChangesAsync() > 0;
        return (guardado, guardado ? null : "Error al iniciar la sesión.");
    }

    /// <summary>
    /// Pausa una sesión activa y calcula el tiempo transcurrido
    /// </summary>
    public async Task<(bool exito, string error)> PausarSesion(Guid idSesion, string descripcion, string userEmail)
    {
        var sesion = await _dbContext.Sesiones.FindAsync(idSesion);

        if (sesion == null)
            return (false, "Sesión no encontrada.");

        sesion.FechaPausa = DateTime.UtcNow;

        // Calcular tiempo transcurrido desde FechaReinicio (si fue reanudada) o FechaInicio
        CalcularYAgregarTiempo(sesion, sesion.FechaPausa.Value);

        sesion.Descripcion = descripcion;

        _dbContext.Sesiones.Update(sesion);
        var guardado = await _dbContext.SaveChangesAsync() > 0;

        return (guardado, guardado ? null : "Error al pausar la sesión.");
    }

    /// <summary>
    /// Reanuda una sesión pausada
    /// </summary>
    public async Task<(bool exito, string error)> ReanudarSesion(Guid idSesion, string descripcion, string userEmail)
    {
        var sesion = await _dbContext.Sesiones.FindAsync(idSesion);

        if (sesion == null)
            return (false, "Sesión no encontrada.");

        sesion.FechaReinicio = DateTime.UtcNow;
        sesion.FechaPausa = null;
        sesion.Descripcion = descripcion;

        _dbContext.Sesiones.Update(sesion);
        var guardado = await _dbContext.SaveChangesAsync() > 0;

        return (guardado, guardado ? null : "Error al reanudar la sesión.");
    }

    /// <summary>
    /// Finaliza una sesión y calcula el tiempo total
    /// </summary>
    public async Task<(bool exito, string error)> FinalizarSesion(Guid idSesion, string descripcion, string userEmail)
    {
        var sesion = await _dbContext.Sesiones.FindAsync(idSesion);

        if (sesion == null)
            return (false, "Sesión no encontrada.");

        sesion.FechaFin = DateTime.UtcNow;

        // Solo calcular tiempo si la sesión NO está pausada
        // Si está pausada, el tiempo ya fue contabilizado en PausarSesion
        if (sesion.FechaPausa == null)
        {
            CalcularYAgregarTiempo(sesion, sesion.FechaFin.Value);
        }

        sesion.Descripcion = descripcion;

        _dbContext.Sesiones.Update(sesion);
        var guardado = await _dbContext.SaveChangesAsync() > 0;

        return (guardado, guardado ? null : "Error al finalizar la sesión.");
    }

    #endregion

    #region Métodos auxiliares

    /// <summary>
    /// Calcula el tiempo transcurrido y lo agrega a la sesión
    /// </summary>
    private void CalcularYAgregarTiempo(Sesion sesion, DateTime fechaHasta)
    {
        // Usar FechaReinicio si existe (sesión reanudada), sino FechaInicio
        DateTime fechaReferencia = sesion.FechaReinicio ?? sesion.FechaInicio;
        TimeSpan diferencia = fechaHasta - fechaReferencia;

        int horasTotales = (int)diferencia.TotalHours;
        int minutos = diferencia.Minutes;

        sesion.Horas += horasTotales;
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

    #endregion
}
