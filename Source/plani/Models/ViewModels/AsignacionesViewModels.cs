using System.ComponentModel;
using plani.Models.Domain;

namespace plani.Models.ViewModels;

public class ProyectoAsignacionesViewModel
{
    public Guid IdProyecto { get; set; }
    public string NombreProyecto { get; set; }
    public string NombreCliente { get; set; }
    public List<Asignacion> Asignaciones { get; set; }

    public int TotalHorasEstimadas => Asignaciones?.Sum(a => a.HorasEstimadas) ?? 0;
    public int CantidadAsignaciones => Asignaciones?.Count ?? 0;
}

public class AsignacionesIndexViewModel
{
    public List<ProyectoAsignacionesViewModel> ProyectosAsignaciones { get; set; }
    public int TotalAsignaciones => ProyectosAsignaciones?.Sum(p => p.CantidadAsignaciones) ?? 0;
    public int TotalHorasEstimadas => ProyectosAsignaciones?.Sum(p => p.TotalHorasEstimadas) ?? 0;

    [DisplayName("Colaborador")]
    public string IdUsuario { get; set; }

    [DisplayName("Cliente & Proyecto")]
    public string IdProyecto { get; set; }
}

public class AgregarAsignacionModel
{
    public string NombreColaborador { get; set; }
    public Guid IdProyecto { get; set; }
    public Guid IdUsuario { get; set; }
    public int HorasEstimadas { get; set; }
    public string Descripcion { get; set; }
}

public class DetalleAsignacionViewModel
{
    public Guid Id { get; set; }
    public string NombreColaborador { get; set; }
    public string NombreProyecto { get; set; }
    public string NombreCliente { get; set; }
    public int HorasEstimadas { get; set; }
    public string Descripcion { get; set; }
}

public class EliminarAsignacionViewModel
{
    public EliminarAsignacionViewModel() { }

    public EliminarAsignacionViewModel(Asignacion asignacion)
    {
        Id = asignacion.Id;
        HorasEstimadas = asignacion.HorasEstimadas;
        ApplicationUser = asignacion.ApplicationUser;
        Proyecto = asignacion.Proyecto;
    }

    public Guid Id { get; set; }
    public int HorasEstimadas { get; set; }
    public plani.Identity.ApplicationUser ApplicationUser { get; set; }
    public Proyecto Proyecto { get; set; }
}
