using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace plani.Models.ViewModels;

/// <summary>
/// ViewModel para mostrar áreas en la lista
/// </summary>
public class AreaListViewModel
{
    /// <summary>
    /// Constructor para crear desde entidad Area
    /// </summary>
    public AreaListViewModel(Area area)
    {
        Id = area.Id.ToString();
        Nombre = area.Nombre;
        Descripcion = area.Descripcion;
        TruncatedDescripcion = area.TruncatedDescripcion;
        CreadoPor = area.CreatedBy;
        CreadoEl = area.DateCreated.ToString("dd/MM/yyyy");
        EditadoPor = area.UpdatedBy ?? string.Empty;
        EditadoEl = area.DateUpdated?.ToString("dd/MM/yyyy") ?? string.Empty;
    }

    public string Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public string TruncatedDescripcion { get; set; }

    // Audit trail
    public string CreadoPor { get; set; }
    public string CreadoEl { get; set; }
    public string EditadoPor { get; set; }
    public string EditadoEl { get; set; }
}

/// <summary>
/// ViewModel para agregar nueva área
/// </summary>
public class AgregarAreaViewModel
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(255, ErrorMessage = "El nombre debe tener máximo 255 caracteres.")]
    public string Nombre { get; set; }

    [DisplayName("Descripción")]
    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    /// <summary>
    /// Convierte el ViewModel a una entidad Area
    /// </summary>
    public Area ToEntity()
    {
        return new Area(Guid.NewGuid(), Nombre, Descripcion);
    }
}

/// <summary>
/// ViewModel para editar área existente
/// </summary>
public class EditarAreaViewModel
{
    [Required(ErrorMessage = "El id es requerido")]
    public string Id { get; set; }

    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(255, ErrorMessage = "El nombre debe tener máximo 255 caracteres.")]
    public string Nombre { get; set; }

    [DisplayName("Descripción")]
    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    /// <summary>
    /// Convierte el ViewModel a una entidad Area para actualización
    /// </summary>
    public Area ToEntity()
    {
        return new Area(Guid.Parse(Id), Nombre, Descripcion);
    }
}

/// <summary>
/// Request para eliminar área
/// </summary>
public class EliminarAreaRequest
{
    [Required]
    public string Id { get; set; }
}
