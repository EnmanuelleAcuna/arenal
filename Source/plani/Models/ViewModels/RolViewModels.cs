using System.ComponentModel.DataAnnotations;
using plani.Identity;

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

public class DetalleRolViewModel
{
    public DetalleRolViewModel(ApplicationRole rol, IList<ApplicationUser> usuarios)
    {
        IdRol = rol.Id;
        Nombre = rol.Name;
        Descripcion = rol.Description;
        Usuarios = usuarios;
    }

    public string IdRol { get; set; }
    public string Nombre { get; set; }

    [Display(Name = "Descripción")]
    public string Descripcion { get; set; }

    public IList<ApplicationUser> Usuarios { get; set; }
}

public class AgregarRolViewModel
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
    public string Nombre { get; set; }

    [Display(Name = "Descripción")]
    [Required(ErrorMessage = "La descripción es requerida.")]
    [StringLength(250, ErrorMessage = "La descripción no debe exceder los 250 caracteres.")]
    public string Descripcion { get; set; }

    public ApplicationRole ToApplicationRole()
    {
        return new ApplicationRole(Guid.NewGuid().ToString(), Nombre, Descripcion);
    }
}

public class EditarRolViewModel
{
    public string IdRol { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
    public string Nombre { get; set; }

    [Display(Name = "Descripción")]
    [Required(ErrorMessage = "La descripción es requerida.")]
    [StringLength(250, ErrorMessage = "La descripción no puede exceder los 250 caracteres.")]
    public string Descripcion { get; set; }
}
