using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using plani.Models.Domain;

namespace plani.Models.ViewModels;

public class IndexProyectosViewModel
{
    [DisplayName("Filtrar por")]
    public string PalabraClave { get; set; }

    public IEnumerable<Proyecto> Proyectos { get; set; }
}

public class DetalleProyectoViewModel
{
    public DetalleProyectoViewModel(Proyecto proyecto)
    {
        Id = proyecto.Id;
        Nombre = proyecto.Nombre;
        Descripcion = proyecto.Descripcion;
        HorasEstimadas = proyecto.HorasEstimadas;
        FechaInicio = proyecto.FechaInicio;
        FechaFin = proyecto.FechaFin;
        FechaInicioFormateada = proyecto.LongDateFechaInicio;
        FechaFinFormateada = proyecto.LongDateFechaFin;
        Area = proyecto.Area;
        Contrato = proyecto.Contrato;
        Responsable = proyecto.Responsable;
    }

    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int? HorasEstimadas { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string FechaInicioFormateada { get; set; }
    public string FechaFinFormateada { get; set; }
    public Area Area { get; set; }
    public Contrato Contrato { get; set; }
    public plani.Identity.ApplicationUser Responsable { get; set; }
}

public class AgregarProyectoViewModel
{
    public Guid IdContrato { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(255, ErrorMessage = "El nombre debe tener máximo 255 caracteres.")]
    public string Nombre { get; set; }

    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [DisplayName("Horas estimadas")]
    public int HorasEstimadas { get; set; }

    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FechaFin { get; set; }

    public Guid IdArea { get; set; }

    [DisplayName("Responsable")]
    public string IdResponsable { get; set; }
}

public class EditarProyectoViewModel
{
    public EditarProyectoViewModel() { }

    public EditarProyectoViewModel(Proyecto proyecto)
    {
        Id = proyecto.Id;
        IdContrato = proyecto.IdContrato;
        Nombre = proyecto.Nombre;
        Descripcion = proyecto.Descripcion;
        HorasEstimadas = proyecto.HorasEstimadas ?? 0;
        FechaInicio = proyecto.FechaInicio;
        FechaFin = proyecto.FechaFin;
        IdArea = proyecto.IdArea;
        IdResponsable = proyecto.IdResponsable;
    }

    public Guid Id { get; set; }
    public Guid IdContrato { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(255, ErrorMessage = "El nombre debe tener máximo 255 caracteres.")]
    public string Nombre { get; set; }

    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [DisplayName("Horas estimadas")]
    public int HorasEstimadas { get; set; }

    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FechaFin { get; set; }

    public Guid IdArea { get; set; }

    [DisplayName("Responsable")]
    public string IdResponsable { get; set; }
}

public class EliminarProyectoViewModel
{
    public EliminarProyectoViewModel() { }

    public EliminarProyectoViewModel(Proyecto proyecto)
    {
        Id = proyecto.Id;
        Nombre = proyecto.Nombre;
        Contrato = proyecto.Contrato;
    }

    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public Contrato Contrato { get; set; }
}
