using System.Globalization;
using Microsoft.EntityFrameworkCore;
using plani.Identity;
using plani.Models.Data;
using plani.Models.ViewModels;

using plani.Models.Domain;

namespace plani.Models.Managers;

/// <summary>
/// Manager para la lógica de negocio del Dashboard de administración
/// </summary>
public class DashboardManager
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DashboardManager> _logger;
    private static readonly CultureInfo CultureEs = new("es-ES");

    public DashboardManager(ApplicationDbContext dbContext, ILogger<DashboardManager> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los datos del dashboard
    /// </summary>
    public async Task<DashboardViewModel> ObtenerDatosDashboardAsync()
    {
        try
        {
            var viewModel = new DashboardViewModel
            {
                Stats = await ObtenerEstadisticasAsync(),
                SesionesPorMes = await ObtenerSesionesPorMesAsync(),
                HorasPorMes = await ObtenerHorasPorMesAsync(),
                HorasPorServicioPorMes = await ObtenerHorasPorServicioPorMesAsync(),
                ProyectosPorEstado = await ObtenerProyectosPorEstadoAsync(),
                TopClientes = await ObtenerTopClientesAsync(),
                TopColaboradores = await ObtenerTopColaboradoresAsync()
            };

            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener datos del dashboard");
            throw;
        }
    }

    /// <summary>
    /// Obtiene las estadísticas generales (tarjetas superiores)
    /// </summary>
    private async Task<DashboardStatsViewModel> ObtenerEstadisticasAsync()
    {
        var hoy = DateTime.UtcNow.Date;

        var stats = new DashboardStatsViewModel
        {
            TotalClientes = await _dbContext.Clientes.CountAsync(),
            TotalProyectos = await _dbContext.Proyectos.CountAsync(),
            TotalColaboradores = await _dbContext.Usuarios.CountAsync(),
            SesionesHoy = await _dbContext.Sesiones
                .Where(s => s.FechaInicio.Date == hoy)
                .CountAsync()
        };

        return stats;
    }

    /// <summary>
    /// Obtiene sesiones agrupadas por mes (últimos 3 meses)
    /// </summary>
    private async Task<SesionesPorMesViewModel> ObtenerSesionesPorMesAsync()
    {
        var mesesAtras = ObtenerUltimosTresMeses();
        var viewModel = new SesionesPorMesViewModel();

        foreach (var mes in mesesAtras)
        {
            var cantidadSesiones = await _dbContext.Sesiones
                .Where(s => s.FechaInicio.Year == mes.Year && s.FechaInicio.Month == mes.Month)
                .CountAsync();

            viewModel.Meses.Add(CapitalizarPrimeraLetra(mes.ToString("MMMM", CultureEs)));
            viewModel.CantidadSesiones.Add(cantidadSesiones);
        }

        return viewModel;
    }

    /// <summary>
    /// Obtiene horas trabajadas agrupadas por mes (últimos 3 meses)
    /// </summary>
    private async Task<HorasPorMesViewModel> ObtenerHorasPorMesAsync()
    {
        var mesesAtras = ObtenerUltimosTresMeses();
        var viewModel = new HorasPorMesViewModel();

        foreach (var mes in mesesAtras)
        {
            var sesionesDelMes = await _dbContext.Sesiones
                .Where(s => s.FechaInicio.Year == mes.Year && s.FechaInicio.Month == mes.Month)
                .Select(s => new { s.Horas, s.Minutes })
                .ToListAsync();

            var totalHoras = CalcularTotalHoras(sesionesDelMes.Select(s => (s.Horas, s.Minutes)));

            viewModel.Meses.Add(CapitalizarPrimeraLetra(mes.ToString("MMMM", CultureEs)));
            viewModel.Horas.Add(totalHoras);
        }

        return viewModel;
    }

    /// <summary>
    /// Obtiene horas por servicio para cada uno de los últimos 3 meses
    /// </summary>
    private async Task<Dictionary<int, HorasPorServicioViewModel>> ObtenerHorasPorServicioPorMesAsync()
    {
        var mesesAtras = ObtenerUltimosTresMeses();
        var resultado = new Dictionary<int, HorasPorServicioViewModel>();

        foreach (var mes in mesesAtras)
        {
            var sesionesDelMes = await _dbContext.Sesiones
                .Include(s => s.Servicio)
                .Where(s => s.FechaInicio.Year == mes.Year && s.FechaInicio.Month == mes.Month)
                .ToListAsync();

            var horasPorServicio = sesionesDelMes
                .GroupBy(s => new { s.IdServicio, s.Servicio.Nombre })
                .Select(g => new
                {
                    Servicio = g.Key.Nombre,
                    Horas = CalcularTotalHoras(g.Select(s => (s.Horas, s.Minutes)))
                })
                .OrderByDescending(x => x.Horas)
                .ToList();

            var viewModel = new HorasPorServicioViewModel
            {
                Servicios = horasPorServicio.Select(x => x.Servicio).ToList(),
                Horas = horasPorServicio.Select(x => x.Horas).ToList()
            };

            resultado[mes.Month] = viewModel;
        }

        return resultado;
    }

    /// <summary>
    /// Obtiene proyectos por estado (activos vs finalizados)
    /// </summary>
    private async Task<ProyectosPorEstadoViewModel> ObtenerProyectosPorEstadoAsync()
    {
        var viewModel = new ProyectosPorEstadoViewModel
        {
            ProyectosActivos = await _dbContext.Proyectos
                .Where(p => p.FechaFin == null)
                .CountAsync(),
            ProyectosFinalizados = await _dbContext.Proyectos
                .Where(p => p.FechaFin != null)
                .CountAsync()
        };

        return viewModel;
    }

    /// <summary>
    /// Obtiene top 5 clientes por horas trabajadas del mes actual
    /// </summary>
    private async Task<TopClientesViewModel> ObtenerTopClientesAsync()
    {
        var mesActual = DateTime.UtcNow;

        var sesionesDelMes = await _dbContext.Sesiones
            .Include(s => s.Proyecto)
                .ThenInclude(p => p.Contrato)
                .ThenInclude(c => c.Cliente)
            .Where(s => s.FechaInicio.Year == mesActual.Year && s.FechaInicio.Month == mesActual.Month)
            .ToListAsync();

        var horasPorCliente = sesionesDelMes
            .GroupBy(s => new { s.Proyecto.Contrato.Cliente.Id, s.Proyecto.Contrato.Cliente.Nombre })
            .Select(g => new
            {
                Cliente = g.Key.Nombre,
                Horas = CalcularTotalHoras(g.Select(s => (s.Horas, s.Minutes)))
            })
            .OrderByDescending(x => x.Horas)
            .Take(5)
            .ToList();

        var viewModel = new TopClientesViewModel
        {
            Clientes = horasPorCliente.Select(x => x.Cliente).ToList(),
            Horas = horasPorCliente.Select(x => x.Horas).ToList()
        };

        return viewModel;
    }

    /// <summary>
    /// Obtiene top 5 colaboradores por horas trabajadas del mes actual
    /// </summary>
    private async Task<TopColaboradoresViewModel> ObtenerTopColaboradoresAsync()
    {
        var mesActual = DateTime.UtcNow;

        var sesionesDelMes = await _dbContext.Sesiones
            .Include(s => s.ApplicationUser)
            .Where(s => s.FechaInicio.Year == mesActual.Year && s.FechaInicio.Month == mesActual.Month)
            .ToListAsync();

        var horasPorColaborador = sesionesDelMes
            .GroupBy(s => new
            {
                s.ApplicationUser.Id,
                NombreCompleto = $"{s.ApplicationUser.Name} {s.ApplicationUser.FirstLastName}"
            })
            .Select(g => new
            {
                Colaborador = g.Key.NombreCompleto,
                Horas = CalcularTotalHoras(g.Select(s => (s.Horas, s.Minutes)))
            })
            .OrderByDescending(x => x.Horas)
            .Take(5)
            .ToList();

        var viewModel = new TopColaboradoresViewModel
        {
            Colaboradores = horasPorColaborador.Select(x => x.Colaborador).ToList(),
            Horas = horasPorColaborador.Select(x => x.Horas).ToList()
        };

        return viewModel;
    }

    #region Métodos auxiliares

    /// <summary>
    /// Obtiene los últimos 3 meses desde el mes actual
    /// </summary>
    private List<DateTime> ObtenerUltimosTresMeses()
    {
        var mesActual = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        return new List<DateTime>
        {
            mesActual.AddMonths(-2),
            mesActual.AddMonths(-1),
            mesActual
        };
    }

    /// <summary>
    /// Calcula el total de horas desde una colección de (Horas, Minutes)
    /// </summary>
    private int CalcularTotalHoras(IEnumerable<(int Horas, int Minutes)> horasYMinutos)
    {
        var totalMinutos = horasYMinutos.Sum(x => x.Horas * 60 + x.Minutes);
        return (int)Math.Round(totalMinutos / 60.0);
    }

    /// <summary>
    /// Capitaliza la primera letra de una cadena
    /// </summary>
    private string CapitalizarPrimeraLetra(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;

        return char.ToUpper(texto[0]) + texto.Substring(1);
    }

    #endregion
}
