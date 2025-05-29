using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using plani.Identity;

namespace plani.Models;

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
    public int TotalSesiones => ProyectosSesiones?.Sum(p => p.CantidadSesiones) ?? 0;
    public double TotalHoras => ProyectosSesiones?.Sum(p => p.TotalHoras) ?? 0;

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
