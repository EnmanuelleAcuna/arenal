using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using plani.Models.Domain;

namespace plani.Models.ViewModels;

public class DetalleContratoViewModel
{
    public DetalleContratoViewModel() { }

    public DetalleContratoViewModel(Contrato contrato)
    {
        Id = contrato.Id;
        Identificacion = contrato.Identificacion;
        Descripcion = contrato.Descripcion;
        FechaInicio = contrato.FechaInicio;
        Cliente = contrato.Cliente;
        Area = contrato.Area;
        Proyectos = contrato.Proyectos;
    }

    public Guid Id { get; set; }
    public string Identificacion { get; set; }
    public string Descripcion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string LongDateFechaInicio => FechaInicio.ToString("D", new CultureInfo("es-ES"));
    public string TruncatedDescripcion => Descripcion?.Length > 20 ? Descripcion.Substring(0, 20) + "..." : Descripcion;
    public Cliente Cliente { get; set; }
    public Area Area { get; set; }
    public IEnumerable<Proyecto> Proyectos { get; set; }
}

public class AgregarContratoViewModel
{
    public Guid IdCliente { get; set; }

    [Required(ErrorMessage = "La identificación es requerida.")]
    [StringLength(100, ErrorMessage = "La identificación debe tener máximo 100 caracteres.")]
    public string Identificacion { get; set; }

    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [DisplayName("Fecha de inicio")]
    [Required(ErrorMessage = "La fecha de inicio es requerida.")]
    public DateTime FechaInicio { get; set; }

    [DisplayName("Fecha de fin")]
    public DateTime? FechaFin { get; set; }

    public Guid IdArea { get; set; }
}

public class EditarContratoViewModel
{
    public EditarContratoViewModel() { }

    public EditarContratoViewModel(Contrato contrato)
    {
        Id = contrato.Id;
        IdCliente = contrato.IdCliente;
        Identificacion = contrato.Identificacion;
        Descripcion = contrato.Descripcion;
        FechaInicio = contrato.FechaInicio;
        IdArea = contrato.IdArea;
    }

    public Guid Id { get; set; }
    public Guid IdCliente { get; set; }

    [Required(ErrorMessage = "La identificación es requerida.")]
    [StringLength(100, ErrorMessage = "La identificación debe tener máximo 100 caracteres.")]
    public string Identificacion { get; set; }

    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [DisplayName("Fecha de inicio")]
    [Required(ErrorMessage = "La fecha de inicio es requerida.")]
    public DateTime FechaInicio { get; set; }

    [DisplayName("Fecha de fin")]
    public DateTime? FechaFin { get; set; }

    public Guid IdArea { get; set; }
}

public class EliminarContratoViewModel
{
    public EliminarContratoViewModel() { }

    public EliminarContratoViewModel(Contrato contrato)
    {
        Id = contrato.Id;
        Identificacion = contrato.Identificacion;
        Cliente = contrato.Cliente;
    }

    public Guid Id { get; set; }
    public string Identificacion { get; set; }
    public Cliente Cliente { get; set; }
}
