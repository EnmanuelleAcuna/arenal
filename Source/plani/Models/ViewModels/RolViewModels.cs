using System.ComponentModel.DataAnnotations;

namespace plani.Models.ViewModels;

/// <summary>
/// ViewModel para mostrar roles en la lista (para JSON)
/// </summary>
public class RolListViewModel
{
    public string Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
}

/// <summary>
/// Request para eliminar rol
/// </summary>
public class EliminarRolRequest
{
    [Required]
    public string Id { get; set; }
}
