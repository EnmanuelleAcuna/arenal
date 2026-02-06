using System.ComponentModel.DataAnnotations;
using System.Globalization;
using plani.Identity;
using plani.Models.Domain;

namespace plani.Models.ViewModels;

public class UsuariosIndexViewModel
{
    public UsuariosIndexViewModel() { }

    public UsuariosIndexViewModel(ApplicationUser usuario, string roles)
    {
        IdUsuario = usuario.Id;
        Nombre = string.Format(new CultureInfo("es-CR"), "{0} {1} {2}", usuario.Name, usuario.FirstLastName, usuario.SecondLastName);
        Correo = usuario.Email;
        NumeroIdentificacion = usuario.IdentificationNumber;
        Roles = roles;
        Estado = (bool)usuario.Active ? "Activo" : "Inactivo";
    }

    public string IdUsuario { get; set; }

    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; }

    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; }

    [Display(Name = "Identificación")]
    public string NumeroIdentificacion { get; set; }

    public string Roles { get; set; }

    public string Estado { get; set; }
}

public class IniciarSesionViewModel
{
    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [Display(Name = "Correo electrónico")]
    [EmailAddress(ErrorMessage = "Debe digitar un correo válido.")]
    public string Correo { get; set; }

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; }
}

public class OlvidoContrasenaViewModel
{
    [Display(Name = "Correo electrónico")]
    [EmailAddress(ErrorMessage = "Debe digitar una dirección de correo electrónico válida.")]
    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    public string CorreoElectronico { get; set; }
}

public class RestablecerContrasenaViewModel
{
    [Display(Name = "Correo electrónico")]
    [Required(ErrorMessage = "El correo electrónico es requerido")]
    [EmailAddress(ErrorMessage = "Debe ser un correo válido")]
    public string CorreoElectronico { get; set; }

    [Display(Name = "Contraseña")]
    [Required(ErrorMessage = "La contraseña es requerida.")]
    [StringLength(100, ErrorMessage = "La {0} debe contener al menos {2} caracteres.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; }

    [Display(Name = "Confirmar contraseña")]
    [DataType(DataType.Password)]
    [Compare("Contrasena", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
    public string ConfirmarContrasena { get; set; }

    public string Code { get; set; }
}

public class AgregarUsuarioViewModel
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
    public string Nombre { get; set; }

    [Display(Name = "Primer apellido")]
    [Required(ErrorMessage = "El primer apellido es requerido.")]
    [StringLength(50, ErrorMessage = "El primer apellido no puede exceder los 50 caracteres.")]
    public string PrimerApellido { get; set; }

    [Display(Name = "Segundo apellido")]
    [Required(ErrorMessage = "El segundo apellido es requerido.")]
    [StringLength(50, ErrorMessage = "El segundo apellido no puede exceder los 50 caracteres.")]
    public string SegundoApellido { get; set; }

    [Display(Name = "Número de identificación")]
    [Required(ErrorMessage = "El número de identificación es requerido.")]
    [StringLength(50, ErrorMessage = "El número de identificacion no puede exceder los 50 caracteres.")]
    public string NumeroIdentificacion { get; set; }

    [Display(Name = "Correo electrónico")]
    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [StringLength(50, ErrorMessage = "El correo electrónico no puede exceder los 50 caracteres.")]
    public string CorreoElectronico { get; set; }

    [Display(Name = "Contraseña")]
    [Required(ErrorMessage = "La contraseña es requerida.")]
    [StringLength(100, ErrorMessage = "La {0} debe contener al menos {2} caracteres.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; }

    [Display(Name = "Confirmar contraseña")]
    [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    public string ConfirmarContrasena { get; set; }

    [Display(Name = "Activo")]
    public bool Estado { get; set; }

    public ApplicationUser Entidad()
    {
        return new ApplicationUser(Guid.NewGuid().ToString(), CorreoElectronico, Nombre,
            PrimerApellido, SegundoApellido, NumeroIdentificacion, true);
    }
}

public class EditarUsuarioViewModel
{
    public EditarUsuarioViewModel() { }

    public EditarUsuarioViewModel(ApplicationUser usuario, IList<ApplicationRole> roles)
    {
        IdUsuario = usuario.Id;
        Nombre = usuario.Name;
        PrimerApellido = usuario.FirstLastName;
        SegundoApellido = usuario.SecondLastName;
        NumeroIdentificacion = usuario.IdentificationNumber;
        CorreoElectronico = usuario.Email;
        Estado = (bool)usuario.Active;
        NombreCompleto = $"{Nombre} {PrimerApellido} {SegundoApellido}";
        Roles = roles;
    }

    [Required(ErrorMessage = "El id es requerido.")]
    public string IdUsuario { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
    public string Nombre { get; set; }

    [Display(Name = "Primer apellido")]
    [Required(ErrorMessage = "El primer apellido es requerido.")]
    [StringLength(50, ErrorMessage = "El primer apellido no puede exceder los 50 caracteres")]
    public string PrimerApellido { get; set; }

    [Display(Name = "Segundo apellido")]
    [Required(ErrorMessage = "El segundo apellido es requerido.")]
    [StringLength(50, ErrorMessage = "El segundo apellido no puede exceder los 50 caracteres.")]
    public string SegundoApellido { get; set; }

    [Display(Name = "Número de identificación")]
    [Required(ErrorMessage = "El número de identificación es requerido.")]
    [StringLength(50, ErrorMessage = "El número de identificación no puede exceder los 50 caracteres.")]
    public string NumeroIdentificacion { get; set; }

    [Display(Name = "Correo electrónico")]
    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [StringLength(50, ErrorMessage = "El correo electrónico no puede exceder los 50 caracteres.")]
    public string CorreoElectronico { get; set; }

    [Display(Name = "Activo")]
    public bool Estado { get; set; }

    public string NombreCompleto { get; set; }

    public IList<ApplicationRole> Roles { get; set; }

    public IList<string> RolesSeleccionados { get; set; }

    public ApplicationUser Entidad()
    {
        return new ApplicationUser(IdUsuario, CorreoElectronico, Nombre, PrimerApellido,
            SegundoApellido, NumeroIdentificacion, true);
    }
}

public class DetalleColaboradorViewModel
{
    public DetalleColaboradorViewModel(ApplicationUser usuario, IList<Asignacion> asignaciones, IList<Proyecto> proyectosACargo)
    {
        IdUsuario = usuario.Id;
        NombreCompleto = usuario.FullName;
        CorreoElectronico = usuario.Email;
        NumeroIdentificacion = usuario.IdentificationNumber;
        Asignaciones = asignaciones;
        ProyectosACargo = proyectosACargo;
    }

    public string IdUsuario { get; set; }
    public string NombreCompleto { get; set; }
    public string CorreoElectronico { get; set; }
    public string NumeroIdentificacion { get; set; }
    public IList<Asignacion> Asignaciones { get; set; }
    public IList<Proyecto> ProyectosACargo { get; set; }
}

public class DetalleUsuarioViewModel
{
    public DetalleUsuarioViewModel(ApplicationUser usuario, IList<ApplicationRole> roles)
    {
        IdUsuario = usuario.Id;
        NombreCompleto = usuario.FullName;
        CorreoElectronico = usuario.Email;
        NumeroIdentificacion = usuario.IdentificationNumber;
        Activo = usuario.Active ?? false;
        Roles = roles;
    }

    public string IdUsuario { get; set; }
    public string NombreCompleto { get; set; }
    public string CorreoElectronico { get; set; }
    public string NumeroIdentificacion { get; set; }
    public bool Activo { get; set; }
    public IList<ApplicationRole> Roles { get; set; }
}
