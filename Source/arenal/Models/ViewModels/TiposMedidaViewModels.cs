using System;
using System.ComponentModel.DataAnnotations;
using arenal.Models.Entities;

namespace arenal.Models.ViewModels;

public class TipoMedidaViewModel : BaseViewModel
{
	public TipoMedidaViewModel(TipoMedida modelo) : base(modelo)
	{
		Id = modelo.Id.ToString();
		Codigo = modelo.Codigo;
		Nombre = modelo.Nombre;
		Estado = modelo.Estado ? "Activo" : "Inactivo";
	}

	public string Id { get; set; }

	[Display(Name = "Código")]
	public string Codigo { get; set; }

	[Display(Name = "Tipo de medida")]
	public string Nombre { get; set; }

	public string Estado { get; set; }
}

public class AgregarTipoMedidaViewModel
{
	[Display(Name = "Código")]
	[Required(ErrorMessage = "El código es requerido")]
	[StringLength(50, ErrorMessage = "El código no puede exceder los 50 caracteres")]
	public string Codigo { get; set; }

	[Display(Name = "Nombre")]
	[Required(ErrorMessage = "El nombre es requerido")]
	[StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
	public string Nombre { get; set; }

	[Display(Name = "Activo")]
	public bool Estado { get; set; }

	public TipoMedida Entidad() => new(Guid.NewGuid(), Codigo, Nombre, Estado);
}

public class EditarTipoMedidaViewModel : BaseViewModel
{
	public EditarTipoMedidaViewModel() { }

	public EditarTipoMedidaViewModel(TipoMedida tipoMedida) : base(tipoMedida)
	{
		Id = tipoMedida.Id.ToString();
		Codigo = tipoMedida.Codigo;
		Nombre = tipoMedida.Nombre;
		Estado = tipoMedida.Estado;
	}

	public string Id { get; set; }

	[Display(Name = "Código")]
	[Required(ErrorMessage = "El código es requerido")]
	[StringLength(50, ErrorMessage = "El código no puede exceder los 50 caracteres")]
	public string Codigo { get; set; }

	[Display(Name = "Nombre")]
	[Required(ErrorMessage = "El nombre es requerido")]
	[StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
	public string Nombre { get; set; }

	[Display(Name = "Activo")]
	public bool Estado { get; set; }

	public TipoMedida Entidad() => new(new Guid(Id), Codigo, Nombre, Estado);
}

public class EliminarTipoMedidaViewModel
{
	public EliminarTipoMedidaViewModel() { }

	public EliminarTipoMedidaViewModel(TipoMedida tipoMedida)
	{
		IdTipoMedida = tipoMedida.Id.ToString();
		Nombre = tipoMedida.Nombre;
	}

	public string IdTipoMedida { get; set; }
	public string Nombre { get; set; }
}
