using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using plani.Models.Domain;

namespace plani.Models.ViewModels;

/// <summary>
/// ViewModel para mostrar modalidades en la lista
/// </summary>
public class ModalidadListViewModel
{
    /// <summary>
    /// Constructor para crear desde entidad Modalidad
    /// </summary>
    public ModalidadListViewModel(Modalidad modalidad)
    {
        Id = modalidad.Id.ToString();
        Nombre = modalidad.Nombre;
        Descripcion = modalidad.Descripcion;
        TruncatedDescripcion = modalidad.TruncatedDescripcion;
        CreadoPor = modalidad.CreatedBy;
        CreadoEl = modalidad.DateCreated.ToString("dd/MM/yyyy");
        EditadoPor = modalidad.UpdatedBy ?? string.Empty;
        EditadoEl = modalidad.DateUpdated?.ToString("dd/MM/yyyy") ?? string.Empty;
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
/// ViewModel para agregar nueva modalidad
/// </summary>
public class AgregarModalidadViewModel
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(255, ErrorMessage = "El nombre debe tener máximo 255 caracteres.")]
    public string Nombre { get; set; }

    [DisplayName("Descripción")]
    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    /// <summary>
    /// Convierte el ViewModel a una entidad Modalidad
    /// </summary>
    public Modalidad ToEntity()
    {
        return new Modalidad(Guid.NewGuid(), Nombre, Descripcion);
    }
}

/// <summary>
/// ViewModel para editar modalidad existente
/// </summary>
public class EditarModalidadViewModel
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
    /// Convierte el ViewModel a una entidad Modalidad para actualización
    /// </summary>
    public Modalidad ToEntity()
    {
        return new Modalidad(Guid.Parse(Id), Nombre, Descripcion);
    }
}

/// <summary>
/// Request para eliminar modalidad
/// </summary>
public class EliminarModalidadRequest
{
    [Required]
    public string Id { get; set; }
}
