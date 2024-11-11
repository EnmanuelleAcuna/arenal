using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using arenal.Models.Identity;
using Microsoft.AspNetCore.Http;

namespace arenal.Models.ViewModels;

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

public class UsuarioViewModel
{
	public UsuarioViewModel() { }

	public UsuarioViewModel(ApplicationUser usuario)
	{
		IdUsuario = usuario.Id;
		Nombre = string.Format(new CultureInfo("es-CR"), "{0} {1} {2}", usuario.Name, usuario.FirstLastName, usuario.SecondLastName);
		Correo = usuario.Email;
		NumeroIdentificacion = usuario.IdentificationNumber;
		Estado = (bool)usuario.Active ? "Activo" : "Inactivo";
	}

	public string IdUsuario { get; set; }

	public string Nombre { get; set; }

	[Display(Name = "Correo electrónico")]
	public string Correo { get; set; }

	public string NumeroIdentificacion { get; set; }

	public string Estado { get; set; }
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
		ApplicationUser usuario = new(Guid.NewGuid().ToString(), CorreoElectronico, CorreoElectronico, Nombre, PrimerApellido, SegundoApellido, NumeroIdentificacion, DateTime.Now, true);
		return usuario;
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

		NombreCompleto = String.Format("{0} {1} {2}", this.Nombre, this.PrimerApellido, this.SegundoApellido);
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

	public string NombreCompleto { get; }

	public IList<ApplicationRole> Roles { get; set; }

	public IList<string> RolesSeleccionados { get; set; }

	public ApplicationUser Entidad()
	{
		ApplicationUser usuario = new(IdUsuario, CorreoElectronico, CorreoElectronico, Nombre, PrimerApellido, SegundoApellido, NumeroIdentificacion, DateTime.Now, true);
		return usuario;
	}
}

public class AgregarInstructorViewModel : UsuarioViewModel
{
	public AgregarInstructorViewModel() : base() { }

	public AgregarInstructorViewModel(ApplicationUser user) : base(user) { }

	[Display(Name = "Provincia")]
	[Required(ErrorMessage = "La Provincia es requerida.")]
	public string IdProvincia { get; set; }

	[Display(Name = "Cantón")]
	[Required(ErrorMessage = "El Cantón es requerido.")]
	public string IdCanton { get; set; }

	[Display(Name = "Distrito")]
	[Required(ErrorMessage = "El Distrito es requerido.")]
	public string IdDistrito { get; set; }

	[Display(Name = "Fotografía")]
	[Required(ErrorMessage = "La fotografía es requerida.")]
	public IFormFile ProfilePicture { get; set; }

	[Display(Name = "Fecha de ingreso")]
	[Required(ErrorMessage = "La fecha de ingreso es requerida")]
	[DataType(DataType.Date, ErrorMessage = "La fecha no tiene formato correcto.")]
	public DateTime FechaIngreso { get; set; } = DateTime.Now;

	public ApplicationUser Entidad() => new(IdUsuario, new Guid(IdProvincia), new Guid(IdCanton), new Guid(IdDistrito), FechaIngreso);
}

public class AgregarClienteViewModel : UsuarioViewModel
{
	public AgregarClienteViewModel() : base() { }

	public AgregarClienteViewModel(ApplicationUser user) : base(user) { }

	[Display(Name = "Provincia")]
	[Required(ErrorMessage = "La Provincia es requerida.")]
	public string IdProvincia { get; set; }

	[Display(Name = "Cantón")]
	[Required(ErrorMessage = "El Cantón es requerido.")]
	public string IdCanton { get; set; }

	[Display(Name = "Distrito")]
	[Required(ErrorMessage = "El Distrito es requerido.")]
	public string IdDistrito { get; set; }

	[Display(Name = "Fotografía")]
	[Required(ErrorMessage = "La fotografía es requerida.")]
	public IFormFile ProfilePicture { get; set; }

	[Display(Name = "Fecha de inscripción")]
	[Required(ErrorMessage = "La fecha de inscripción es requerida")]
	[DataType(DataType.Date, ErrorMessage = "La fecha no tiene formato correcto.")]
	public DateTime FechaInscripcion { get; set; } = DateTime.Now;

	[Display(Name = "Fecha de renovación")]
	[Required(ErrorMessage = "La fecha de renovación es requerida")]
	[DataType(DataType.Date, ErrorMessage = "La fecha no tiene formato correcto.")]
	public DateTime FechaRenovacion { get; set; } = DateTime.Now.AddDays(30);

	public ApplicationUser Entidad() => new(IdUsuario, new Guid(IdProvincia), new Guid(IdCanton), new Guid(IdDistrito), FechaInscripcion, FechaRenovacion);
}

public class ReporteInstructorViewModel
{
	public ReporteInstructorViewModel(ApplicationUser usuarioInstructor)
	{
		NumeroIdentificacion = usuarioInstructor.IdentificationNumber;
		Nombre = string.Format(new CultureInfo("es-CR"), "{0} {1} {2}", usuarioInstructor.Name, usuarioInstructor.FirstLastName, usuarioInstructor.SecondLastName);
		Correo = usuarioInstructor.Email;
		Domicilio = string.Format(new CultureInfo("es-CR"), "{0}, cantón {1}, distrito {2}", usuarioInstructor.Provincia?.Nombre, usuarioInstructor.Canton?.Nombre, usuarioInstructor.Distrito?.Nombre);
		FechaIngreso = usuarioInstructor.FechaIngresoInscripcion?.ToString("dd/MM/yyyy");
		Estado = Convert.ToBoolean(usuarioInstructor.Active) ? "Activo" : "Inactivo";
	}

	[Display(Name = "Número de identificación")]
	public string NumeroIdentificacion { get; set; }

	[Display(Name = "Nombre")]
	public string Nombre { get; set; }

	[Display(Name = "Correo electrónico")]
	public string Correo { get; set; }

	[Display(Name = "Domicilio")]
	public string Domicilio { get; set; }

	[Display(Name = "Fecha de ingreso")]
	public string FechaIngreso { get; set; }

	[Display(Name = "Estado")]
	public string Estado { get; set; }
}

public class ReporteClienteViewModel
{
	public ReporteClienteViewModel(ApplicationUser usuarioCliente)
	{
		NumeroIdentificacion = usuarioCliente.IdentificationNumber;
		Nombre = string.Format(new CultureInfo("es-CR"), "{0} {1} {2}", usuarioCliente.Name, usuarioCliente.FirstLastName, usuarioCliente.SecondLastName);
		Correo = usuarioCliente.Email;
		Domicilio = string.Format(new CultureInfo("es-CR"), "{0}, cantón {1}, distrito {2}", usuarioCliente.Provincia?.Nombre, usuarioCliente.Canton?.Nombre, usuarioCliente.Distrito?.Nombre);
		FechaInscripcion = usuarioCliente.FechaIngresoInscripcion?.ToString("dd/MM/yyyy");
		FechaRenovacion = usuarioCliente.FechaRenovacion?.ToString("dd/MM/yyyy");
		Estado = Convert.ToBoolean(usuarioCliente.Active) ? "Activo" : "Inactivo";
	}

	[Display(Name = "Número de identificación")]
	public string NumeroIdentificacion { get; set; }

	[Display(Name = "Nombre")]
	public string Nombre { get; set; }

	[Display(Name = "Correo electrónico")]
	public string Correo { get; set; }

	[Display(Name = "Domicilio")]
	public string Domicilio { get; set; }

	[Display(Name = "Fecha de inscripción")]
	public string FechaInscripcion { get; set; }

	[Display(Name = "Fecha de renovación")]
	public string FechaRenovacion { get; set; }

	[Display(Name = "Estado")]
	public string Estado { get; set; }
}
