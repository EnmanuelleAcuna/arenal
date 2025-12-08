using System.ComponentModel;

namespace plani.Models.ViewModels;

/// <summary>
/// ViewModel principal para el dashboard de administración
/// </summary>
public class DashboardViewModel
{
    /// <summary>
    /// Estadísticas generales para las tarjetas superiores
    /// </summary>
    public DashboardStatsViewModel Stats { get; set; }

    /// <summary>
    /// Datos para el gráfico de sesiones por mes (últimos 3 meses)
    /// </summary>
    public SesionesPorMesViewModel SesionesPorMes { get; set; }

    /// <summary>
    /// Datos para el gráfico de horas trabajadas por mes (últimos 3 meses)
    /// </summary>
    public HorasPorMesViewModel HorasPorMes { get; set; }

    /// <summary>
    /// Datos para el gráfico de horas por servicio por cada mes (últimos 3 meses)
    /// Diccionario: Key = número de mes (1-12), Value = datos del mes
    /// </summary>
    public Dictionary<int, HorasPorServicioViewModel> HorasPorServicioPorMes { get; set; }

    /// <summary>
    /// Datos para el gráfico de proyectos por estado
    /// </summary>
    public ProyectosPorEstadoViewModel ProyectosPorEstado { get; set; }

    /// <summary>
    /// Datos para el gráfico de top 5 clientes por horas trabajadas del mes actual
    /// </summary>
    public TopClientesViewModel TopClientes { get; set; }

    /// <summary>
    /// Datos para el gráfico de top 5 colaboradores por horas trabajadas del mes actual
    /// </summary>
    public TopColaboradoresViewModel TopColaboradores { get; set; }
}

/// <summary>
/// Estadísticas generales del dashboard
/// </summary>
public class DashboardStatsViewModel
{
    public int TotalClientes { get; set; }
    public int TotalProyectos { get; set; }
    public int TotalColaboradores { get; set; }
    public int SesionesHoy { get; set; }
}

/// <summary>
/// Datos de sesiones por mes (últimos 3 meses)
/// </summary>
public class SesionesPorMesViewModel
{
    public List<string> Meses { get; set; } = new();
    public List<int> CantidadSesiones { get; set; } = new();
}

/// <summary>
/// Datos de horas trabajadas por mes (últimos 3 meses)
/// </summary>
public class HorasPorMesViewModel
{
    public List<string> Meses { get; set; } = new();
    public List<int> Horas { get; set; } = new();
}

/// <summary>
/// Datos de horas por servicio para un mes específico de los últimos 3 meses
/// </summary>
public class HorasPorServicioViewModel
{
    public List<string> Servicios { get; set; } = new();
    public List<int> Horas { get; set; } = new();
}

/// <summary>
/// Datos de proyectos por estado (activos vs finalizados)
/// </summary>
public class ProyectosPorEstadoViewModel
{
    public int ProyectosActivos { get; set; }
    public int ProyectosFinalizados { get; set; }
}

/// <summary>
/// Datos del top 5 clientes por horas trabajadas del mes actual
/// </summary>
public class TopClientesViewModel
{
    public List<string> Clientes { get; set; } = new();
    public List<int> Horas { get; set; } = new();
}

/// <summary>
/// Datos del top 5 colaboradores por horas trabajadas del mes actual
/// </summary>
public class TopColaboradoresViewModel
{
    public List<string> Colaboradores { get; set; } = new();
    public List<int> Horas { get; set; } = new();
}

