using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace plani.Models.ViewModels;

/// <summary>
/// ViewModel para mostrar servicios en la lista
/// </summary>
public class ServicioListViewModel
{
    /// <summary>
    /// Constructor para crear desde entidad Servicio
    /// </summary>
    public ServicioListViewModel(Servicio servicio)
    {
        Id = servicio.Id.ToString();
        Nombre = servicio.Nombre;
        Descripcion = servicio.Descripcion;
        TruncatedDescripcion = servicio.TruncatedDescripcion;
        IdArea = servicio.IdArea.ToString();
        IdModalidad = servicio.IdModalidad.ToString();
        AreaNombre = servicio.Area?.Nombre ?? string.Empty;
        ModalidadNombre = servicio.Modalidad?.Nombre ?? string.Empty;
        CreadoPor = servicio.CreatedBy;
        CreadoEl = servicio.DateCreated.ToString("dd/MM/yyyy");
        EditadoPor = servicio.UpdatedBy ?? string.Empty;
        EditadoEl = servicio.DateUpdated?.ToString("dd/MM/yyyy") ?? string.Empty;
    }

    public string Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public string TruncatedDescripcion { get; set; }
    public string IdArea { get; set; }
    public string IdModalidad { get; set; }
    public string AreaNombre { get; set; }
    public string ModalidadNombre { get; set; }

    // Audit trail
    public string CreadoPor { get; set; }
    public string CreadoEl { get; set; }
    public string EditadoPor { get; set; }
    public string EditadoEl { get; set; }
}

/// <summary>
/// ViewModel para agregar nuevo servicio
/// </summary>
public class AgregarServicioViewModel
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(250, ErrorMessage = "El nombre debe tener máximo 250 caracteres.")]
    public string Nombre { get; set; }

    [DisplayName("Descripción")]
    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [Required(ErrorMessage = "El área es requerida")]
    public string IdArea { get; set; }

    [Required(ErrorMessage = "La modalidad es requerida")]
    public string IdModalidad { get; set; }

    /// <summary>
    /// Convierte el ViewModel a una entidad Servicio
    /// </summary>
    public Servicio ToEntity()
    {
        return new Servicio(
            Guid.NewGuid(),
            Nombre,
            Descripcion,
            Guid.Parse(IdArea),
            Guid.Parse(IdModalidad)
        );
    }
}

/// <summary>
/// ViewModel para editar servicio existente
/// </summary>
public class EditarServicioViewModel
{
    [Required(ErrorMessage = "El id es requerido")]
    public string Id { get; set; }

    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(250, ErrorMessage = "El nombre debe tener máximo 250 caracteres.")]
    public string Nombre { get; set; }

    [DisplayName("Descripción")]
    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    [Required(ErrorMessage = "El área es requerida")]
    public string IdArea { get; set; }

    [Required(ErrorMessage = "La modalidad es requerida")]
    public string IdModalidad { get; set; }

    /// <summary>
    /// Convierte el ViewModel a una entidad Servicio para actualización
    /// </summary>
    public Servicio ToEntity()
    {
        return new Servicio(
            Guid.Parse(Id),
            Nombre,
            Descripcion,
            Guid.Parse(IdArea),
            Guid.Parse(IdModalidad)
        );
    }
}

/// <summary>
/// Request para eliminar servicio
/// </summary>
public class EliminarServicioRequest
{
    [Required]
    public string Id { get; set; }
}

/// <summary>
/// ViewModel para mostrar el detalle completo de un servicio
/// </summary>
public class ServicioDetalleViewModel
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public string AreaNombre { get; set; }
    public string ModalidadNombre { get; set; }

    public List<ProyectoUsandoServicioViewModel> ProyectosUsandoServicio { get; set; }

    public ServicioDetalleViewModel()
    {
        ProyectosUsandoServicio = new List<ProyectoUsandoServicioViewModel>();
    }
}

/// <summary>
/// ViewModel para un proyecto que usa un servicio
/// </summary>
public class ProyectoUsandoServicioViewModel
{
    public Guid IdProyecto { get; set; }
    public string NombreProyecto { get; set; }
    public string NombreCliente { get; set; }
    public string NombreArea { get; set; }
}
