using System;
using System.ComponentModel.DataAnnotations;
using arenal.Models.Entities;

namespace arenal.Models.ViewModels;

public class ContactoViewModel
{
	public ContactoViewModel(Contacto contacto)
	{
		Id = contacto.Id.ToString();
		Nombre = contacto.NombreCompleto;
		Correo = contacto.CorreoElectronico;
		Telefono = contacto.Telefono;
		Mensaje = contacto.Mensaje;
		Fecha = contacto.FechaRegistro.ToString("dd/MM/yyyy");
	}

	[Required(ErrorMessage = "El id es requerido.")]
	public string Id { get; set; }

	[Display(Name = "Nombre completo")]
	public string Nombre { get; set; }

	[Display(Name = "Correo electrónico")]
	public string Correo { get; set; }

	[Display(Name = "Teléfono")]
	public string Telefono { get; set; }

	[Required(ErrorMessage = "El mensaje es requerido.")]
	public string Mensaje { get; set; }

	[Display(Name = "Fecha de registro")]
	public string Fecha { get; set; }
}

public class AgregarContactoViewModel
{
	[Display(Name = "Nombre completo")]
	[Required(ErrorMessage = "El nombre completo es requerido.")]
	[StringLength(255, ErrorMessage = "El nombre completo no puede exceder los {0} caracteres.")]
	public string Nombre { get; set; }

	[Display(Name = "Correo electrónico")]
	[Required(ErrorMessage = "El correo es requerido.")]
	[StringLength(50, ErrorMessage = "El correo no puede exceder los {0} caracteres.")]
	[EmailAddress(ErrorMessage = "Por favor digite una dirección de correo válida.")]
	public string Correo { get; set; }

	[Display(Name = "Teléfono")]
	[Required(ErrorMessage = "El teléfono es requerido.")]
	[RegularExpression("^[0-9]+$", ErrorMessage = "Por favor digite un teléfono correcto.")]
	[StringLength(16, ErrorMessage = "El teléfono no puede exceder los {1} caracteres.")]
	public string Telefono { get; set; }

	[Required(ErrorMessage = "El mensaje es requerido.")]
	[StringLength(4000, ErrorMessage = "El mensaje no puede exceder los {0} caracteres.")]
	public string Mensaje { get; set; }

	public Contacto Entidad() => new(Guid.NewGuid(), Nombre, Correo, Telefono, Mensaje);
}

public class EliminarContactoViewModel
{
	public EliminarContactoViewModel() { }

	public EliminarContactoViewModel(Contacto contacto)
	{
		Id = contacto.Id.ToString();
		Nombre = contacto.NombreCompleto;
		Mensaje = contacto.Mensaje;
	}

	[Required(ErrorMessage = "El id es requerido.")]
	public string Id { get; set; }

	public string Nombre { get; set; }
	public string Mensaje { get; set; }
}
