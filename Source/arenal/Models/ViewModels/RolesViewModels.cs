using System;
using System.ComponentModel.DataAnnotations;
using arenal.Models.Identity;

namespace arenal.Models.ViewModels;

public class InicioRolesViewModel
{
	public InicioRolesViewModel(ApplicationRole rol)
	{
		IdRol = rol.Id;
		Nombre = rol.Name;
		Descripcion = rol.Description;
	}

	public string IdRol { get; set; }

	public string Nombre { get; set; }

	[Display(Name = "Descripción")]
	public string Descripcion { get; set; }
}

public class NuevoRolViewModel
{
	[Required(ErrorMessage = "El nombre es requerido.")]
	[StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
	public string Nombre { get; set; }

	[Display(Name = "Descripción")]
	[Required(ErrorMessage = "La descripción es requerida.")]
	[StringLength(250, ErrorMessage = "La descripción no debe exceder los 250 caracteres.")]
	public string Descripcion { get; set; }

	public ApplicationRole Entidad()
	{
		ApplicationRole rol = new(Guid.NewGuid().ToString(), Nombre, Descripcion);
		return rol;
	}
}

public class EditarRolViewModel
{
	public EditarRolViewModel() { }

	public EditarRolViewModel(ApplicationRole rol)
	{
		IdRol = rol.Id;
		Nombre = rol.Name;
		Descripcion = rol.Description;
	}

	public string IdRol { get; set; }

	[Required(ErrorMessage = "El nombre es requerido.")]
	[StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
	public string Nombre { get; set; }

	[Display(Name = "Descripción")]
	[Required(ErrorMessage = "La descripción es requerida.")]
	[StringLength(250, ErrorMessage = "La descripción no puede exceder los 250 caracteres.")]
	public string Descripcion { get; set; }

	public ApplicationRole Entidad()
	{
		ApplicationRole rol = new(IdRol, Nombre, Descripcion);
		return rol;
	}
}
