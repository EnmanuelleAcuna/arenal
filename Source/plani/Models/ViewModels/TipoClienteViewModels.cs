using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace plani.Models.ViewModels;

/// <summary>
/// ViewModel para mostrar tipos de cliente en la lista
/// </summary>
public class TipoClienteListViewModel
{
    /// <summary>
    /// Constructor para crear desde entidad TipoCliente
    /// </summary>
    public TipoClienteListViewModel(TipoCliente tipoCliente)
    {
        Id = tipoCliente.Id.ToString();
        Nombre = tipoCliente.Nombre;
        Descripcion = tipoCliente.Descripcion;
        TruncatedDescripcion = tipoCliente.TruncatedDescripcion;
        CreadoPor = tipoCliente.CreatedBy;
        CreadoEl = tipoCliente.DateCreated.ToString("dd/MM/yyyy");
        EditadoPor = tipoCliente.UpdatedBy ?? string.Empty;
        EditadoEl = tipoCliente.DateUpdated?.ToString("dd/MM/yyyy") ?? string.Empty;
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
/// ViewModel para agregar nuevo tipo de cliente
/// </summary>
public class AgregarTipoClienteViewModel
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(255, ErrorMessage = "El nombre debe tener máximo 255 caracteres.")]
    public string Nombre { get; set; }

    [DisplayName("Descripción")]
    [Required(ErrorMessage = "La descripción es requerida")]
    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    /// <summary>
    /// Convierte el ViewModel a una entidad TipoCliente
    /// </summary>
    public TipoCliente ToEntity()
    {
        return new TipoCliente(
            Guid.NewGuid(),
            Nombre,
            Descripcion
        );
    }
}

/// <summary>
/// ViewModel para editar tipo de cliente existente
/// </summary>
public class EditarTipoClienteViewModel
{
    [Required(ErrorMessage = "El id es requerido")]
    public string Id { get; set; }

    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(255, ErrorMessage = "El nombre debe tener máximo 255 caracteres.")]
    public string Nombre { get; set; }

    [DisplayName("Descripción")]
    [Required(ErrorMessage = "La descripción es requerida")]
    [StringLength(2000, ErrorMessage = "La descripción debe tener máximo 2000 caracteres.")]
    public string Descripcion { get; set; }

    /// <summary>
    /// Convierte el ViewModel a una entidad TipoCliente para actualización
    /// </summary>
    public TipoCliente ToEntity()
    {
        return new TipoCliente(
            Guid.Parse(Id),
            Nombre,
            Descripcion
        );
    }
}

/// <summary>
/// Request para eliminar tipo de cliente
/// </summary>
public class EliminarTipoClienteRequest
{
    [Required]
    public string Id { get; set; }
}
