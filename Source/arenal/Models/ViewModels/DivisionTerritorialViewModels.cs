using System;
using System.ComponentModel.DataAnnotations;
using arenal.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace arenal.Models.ViewModels;

public class ProvinciaViewModel : BaseViewModel
{
	public ProvinciaViewModel(Provincia provincia) : base(provincia)
	{
		Id = provincia.Id.ToString();
		Nombre = provincia.Nombre;
		Estado = provincia.Estado ? "Activo" : "Inactivo";
	}

	public string Id { get; set; }
	public string Nombre { get; set; }
	public string Estado { get; set; }
}

public class AgregarProvinciaViewModel
{
	[Display(Name = "Nombre de la provincia")]
	[Required(ErrorMessage = "El nombre es requerido.")]
	[StringLength(50, ErrorMessage = "El nombre no puede exceder los {0} caracteres.")]
	public string Nombre { get; set; }

	public bool Activo { get; set; }

	public Provincia Entidad() => new(Guid.NewGuid(), Nombre, Activo);
}

public class EditarProvinciaViewModel : BaseViewModel
{
	public EditarProvinciaViewModel() : base() { }

	public EditarProvinciaViewModel(Provincia provincia) : base(provincia)
	{
		Id = provincia.Id.ToString();
		Nombre = provincia.Nombre;
		Activo = provincia.Estado;
	}

	[Required(ErrorMessage = "El id es requerido.")]
	public string Id { get; set; }

	[Display(Name = "Nombre de la provincia")]
	[Required(ErrorMessage = "El nombre es requerido.")]
	[StringLength(50, ErrorMessage = "El nombre no puede exceder los {0} caracteres.")]
	public string Nombre { get; set; }

	public bool Activo { get; set; }

	public Provincia Entidad() => new(new Guid(Id), Nombre, Activo);
}

public class EliminarProvinciaViewModel
{
	public EliminarProvinciaViewModel() { }

	public EliminarProvinciaViewModel(Provincia provincia)
	{
		Id = provincia.Id.ToString();
		Nombre = provincia.Nombre;
	}

	[Required(ErrorMessage = "El id es requerido.")]
	public string Id { get; set; }

	public string Nombre { get; set; }
}

public class CantonViewModel : BaseViewModel
{
	public CantonViewModel(Canton canton) : base(canton)
	{
		Id = canton.Id.ToString();
		Nombre = canton.Nombre;
		Estado = canton.Estado ? "Activo" : "Inactivo";
		IdINEC = canton.IdCantonInec;
		Provincia = new ProvinciaViewModel(canton.Provincia);
	}

	public string Id { get; set; }
	public string Nombre { get; set; }
	public string Estado { get; set; }
	[Display(Name = "Código INEC")]
	public int IdINEC { get; set; }
	public ProvinciaViewModel Provincia { get; set; }
}

public class AgregarCantonViewModel
{
	[Display(Name = "Nombre del cantón")]
	[Required(ErrorMessage = "El nombre es requerido.")]
	[StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
	public string Nombre { get; set; }

	public bool Activo { get; set; }

	[Display(Name = "Código INEC")]
	[Required(ErrorMessage = "El código INEC es requerido.")]
	public int IdINEC { get; set; }

	public string IdProvincia { get; set; }

	public Canton Entidad() =>
		new(Guid.NewGuid(), Nombre, Activo, IdINEC, new Guid(IdProvincia));
}

public class EditarCantonViewModel : BaseViewModel
{
	public EditarCantonViewModel() : base() { }

	public EditarCantonViewModel(Canton canton) : base(canton)
	{
		Id = canton.Id.ToString();
		Nombre = canton.Nombre;
		Activo = canton.Estado;
		IdINEC = canton.IdCantonInec;
		IdProvincia = canton.IdProvincia.ToString();
	}

	[Required(ErrorMessage = "El id es requerido.")]
	public string Id { get; set; }

	[Display(Name = "Nombre del cantón")]
	[Required(ErrorMessage = "El nombre es requerido.")]
	[StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
	public string Nombre { get; set; }

	public bool Activo { get; set; }

	[Display(Name = "Código INEC")]
	[Required(ErrorMessage = "El código INEC es requerido.")]
	public int IdINEC { get; set; }

	[Display(Name = "Provincia")]
	[Required(ErrorMessage = "La provincia es requerida.")]
	public string IdProvincia { get; set; }

	public Canton Entidad() =>
		new(new Guid(Id), Nombre, Activo, IdINEC, new Guid(IdProvincia));
}

public class EliminarCantonViewModel
{
	public EliminarCantonViewModel() { }

	public EliminarCantonViewModel(Canton canton)
	{
		Id = canton.Id.ToString();
		Nombre = canton.Nombre;
	}

	[Required(ErrorMessage = "El id es requerido.")]
	public string Id { get; set; }

	public string Nombre { get; set; }
}

public class DistritoViewModel : BaseViewModel
{
	public DistritoViewModel(Distrito distrito) : base(distrito)
	{
		Id = distrito.Id.ToString();
		Nombre = distrito.Nombre;
		Estado = distrito.Estado ? "Activo" : "Inactivo";
		IdINEC = distrito.IdDistritoInec;
		Canton = new CantonViewModel(distrito.Canton);

	}

	public string Id { get; set; }
	public string Nombre { get; set; }
	public string Estado { get; set; }
	[Display(Name = "Código INEC")]
	public int IdINEC { get; set; }
	public CantonViewModel Canton { get; set; }
}

public class AgregarDistritoViewModel
{
	[Display(Name = "Nombre del distrito")]
	[Required(ErrorMessage = "El nombre es requerido")]
	[StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
	public string Nombre { get; set; }

	[Display(Name = "Activo")]
	public bool Estado { get; set; }

	[Display(Name = "Código INEC")]
	[Required(ErrorMessage = "El código INEC es requerido")]
	public int IdINEC { get; set; }

	public string IdCanton { get; set; }

	public Distrito Entidad() =>
		new(Guid.NewGuid(), Nombre, Estado, IdINEC, new Guid(IdCanton));
}

public class EditarDistritoViewModel : BaseViewModel
{
	public EditarDistritoViewModel() : base() { }

	public EditarDistritoViewModel(Distrito distrito) : base(distrito)
	{
		Id = distrito.Id.ToString();
		Nombre = distrito.Nombre;
		Activo = distrito.Estado;
		IdINEC = distrito.IdDistritoInec;
		IdCanton = distrito.IdCanton.ToString();
	}

	[Required(ErrorMessage = "El id es requerido.")]
	public string Id { get; set; }

	[Display(Name = "Nombre del distrito")]
	[Required(ErrorMessage = "El nombre es requerido.")]
	[StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
	public string Nombre { get; set; }

	public bool Activo { get; set; }

	[Display(Name = "Código INEC")]
	[Required(ErrorMessage = "El código INEC es requerido.")]
	public int IdINEC { get; set; }

	[Display(Name = "Canton")]
	[Required(ErrorMessage = "El Canton es requerido.")]
	public string IdCanton { get; set; }

	public Distrito Entidad() =>
		new(new Guid(Id), Nombre, Activo, IdINEC, new Guid(IdCanton));
}

public class EliminarDistritoViewModel
{
	public EliminarDistritoViewModel() { }

	public EliminarDistritoViewModel(Distrito distrito)
	{
		Id = distrito.Id.ToString();
		Nombre = distrito.Nombre;
	}

	[Required(ErrorMessage = "El id es requerido.")]
	public string Id { get; set; }

	public string Nombre { get; set; }
}

public class CantonesDistritosSelectListItem : SelectListItem
{
	public string IdPadre { get; set; }
}
