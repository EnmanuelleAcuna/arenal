using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using plani.Models.Domain;

namespace plani.Models.ViewModels;

public class ProyectoSesionesViewModel
{
    public Guid IdProyecto { get; set; }
    public string NombreProyecto { get; set; }
    public string NombreCliente { get; set; }
    public List<Sesion> Sesiones { get; set; }

    public double TotalHoras => Sesiones?.Sum(a => a.Horas) ?? 0;
    public double TotalMinutos => Sesiones?.Sum(a => a.Minutes) ?? 0;
    public int CantidadSesiones => Sesiones?.Count ?? 0;
}

public class SesionesIndexViewModel
{
    public List<ProyectoSesionesViewModel> ProyectosSesiones { get; set; }
    public List<Sesion> SesionesActivas { get; set; }
    public List<Sesion> Sesiones { get; set; }

    public int TotalSesiones => Sesiones?.Count ?? ProyectosSesiones?.Sum(p => p.CantidadSesiones) ?? 0;
    public double TotalHoras => Sesiones?.Sum(s => s.Horas + s.Minutes / 60.0) ?? ProyectosSesiones?.Sum(p => p.TotalHoras) ?? 0;
    public double TotalMinutos => Sesiones?.Sum(s => s.Minutes) ?? ProyectosSesiones?.Sum(p => p.TotalMinutos) ?? 0;

    [DisplayName("Colaborador")]
    public string IdUsuario { get; set; }

    [DisplayName("Cliente & Proyecto")]
    public string IdProyecto { get; set; }

    [DisplayName("Inicio")]
    public DateTime? FechaInicio { get; set; }

    [DisplayName("Fin")]
    public DateTime? FechaFin { get; set; }
}

public class AgregarSesionModel
{
    public string NombreColaborador { get; set; }
    public Guid IdProyecto { get; set; }
    public Guid IdUsuario { get; set; }
    public Guid IdServicio { get; set; }
    public DateTime Fecha { get; set; }
    public int Horas { get; set; }

    [Range(0, 59, ErrorMessage = "Los minutos deben estar entre 0 y 59.")]
    public int Minutos { get; set; }

    public string Descripcion { get; set; }
}

public class PausarSesionModel
{
    public Guid IdSesion { get; set; }
    public string NombreColaborador { get; set; }
    public Guid IdProyecto { get; set; }
    public string NombreProyecto { get; set; }
    public Guid IdUsuario { get; set; }
    public Guid IdServicio { get; set; }
    public string NombreServicio { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; }
    public int Horas { get; set; }
    public int Minutos { get; set; }
}

public class FinalizarSesionModel
{
    public Guid IdSesion { get; set; }
    public string NombreColaborador { get; set; }
    public Guid IdProyecto { get; set; }
    public string NombreProyecto { get; set; }
    public Guid IdUsuario { get; set; }
    public Guid IdServicio { get; set; }
    public string NombreServicio { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; }
}

public class DetalleSesionViewModel
{
    public DetalleSesionViewModel() { }

    public DetalleSesionViewModel(Sesion sesion)
    {
        Id = sesion.Id;
        NombreColaborador = sesion.ApplicationUser?.FullName;
        NombreProyecto = sesion.Proyecto?.Nombre;
        NombreCliente = sesion.Proyecto?.Contrato?.Cliente?.Nombre;
        NombreServicio = sesion.Servicio?.Nombre;
        FechaInicio = sesion.FechaInicio;
        FechaFin = sesion.FechaFin;
        Horas = sesion.Horas;
        Minutes = sesion.Minutes;
        Descripcion = sesion.Descripcion;
        DateCreated = sesion.DateCreated;
        Servicio = sesion.Servicio;
        Proyecto = sesion.Proyecto;
    }

    public Guid Id { get; set; }
    public string NombreColaborador { get; set; }
    public string NombreProyecto { get; set; }
    public string NombreCliente { get; set; }
    public string NombreServicio { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int Horas { get; set; }
    public int Minutes { get; set; }
    public string Descripcion { get; set; }
    public string Estado { get; set; }
    public DateTime DateCreated { get; set; }
    public Servicio Servicio { get; set; }
    public Proyecto Proyecto { get; set; }
}
