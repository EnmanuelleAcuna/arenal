using System;
using System.ComponentModel.DataAnnotations;
using arenal.Models.Entities;

namespace arenal.Models.ViewModels;

public class MaquinaViewModel : BaseViewModel
{
	public MaquinaViewModel(Maquina maquina) : base(maquina)
	{
		Id = maquina.Id.ToString();
		Codigo = maquina.Codigo;
		Nombre = maquina.Nombre;
		CodigoActivo = maquina.CodigoActivo;
		Estado = maquina.Estado ? "Activo" : "Inactivo";
		FechaAdquisicion = maquina.FechaAdquisicion;
		TipoMaquina = new TipoMaquinaViewModel(maquina.TipoMaquina);
	}

	public string Id { get; set; }
	public string Nombre { get; set; }

	[Display(Name = "Código")]
	public string Codigo { get; set; }

	[Display(Name = "Código de activo")]
	public string CodigoActivo { get; set; }

	[Display(Name = "Fecha de adquisición")]
	public DateTime FechaAdquisicion { get; set; }

	public string Estado { get; set; }
	public TipoMaquinaViewModel TipoMaquina { get; set; }
}

public class AgregarMaquinaViewModel
{
	[Required(ErrorMessage = "El nombre es requerido.")]
	[StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
	public string Nombre { get; set; }

	[Display(Name = "Código")]
	[Required(ErrorMessage = "El codigo es requerido.")]
	[StringLength(50, ErrorMessage = "El codigo no puede exceder los 50 caracteres.")]
	public string Codigo { get; set; }

	[Display(Name = "Código de activo")]
	[Required(ErrorMessage = "El codigo de activo es requerido.")]
	[StringLength(50, ErrorMessage = "El codigo de activo no puede exceder los 50 caracteres.")]
	public string CodigoActivo { get; set; }

	[Display(Name = "Fecha de adquisición")]
	[Required(ErrorMessage = "La fecha de adquisición es requerida")]
	[DataType(DataType.Date, ErrorMessage = "La fecha no tiene formato correcto.")]
	public DateTime FechaAdquisicion { get; set; }

	[Display(Name = "Tipo de máquina")]
	[Required(ErrorMessage = "Debe indicar el tipo de máquina")]
	public string IdTipoMaquina { get; set; }

	public bool Activo { get; set; }

	public Maquina Entidad() => new(Guid.NewGuid(), Codigo, Nombre, CodigoActivo, Activo, Convert.ToDateTime(FechaAdquisicion), new Guid(IdTipoMaquina));
}

public class EditarMaquinaViewModel : BaseViewModel
{
	public EditarMaquinaViewModel() { }

	public EditarMaquinaViewModel(Maquina maquina) : base(maquina)
	{
		Id = maquina.Id.ToString();
		Nombre = maquina.Nombre;
		Codigo = maquina.Codigo;
		CodigoActivo = maquina.CodigoActivo;
		FechaAdquisicion = maquina.FechaAdquisicion;
		IdTipoMaquina = maquina.TipoMaquina.Id.ToString();
		Activo = maquina.Estado;
	}

	[Required(ErrorMessage = "El id es requerido.")]
	public string Id { get; set; }

	[Required(ErrorMessage = "El nombre es requerido.")]
	[StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
	public string Nombre { get; set; }

	[Display(Name = "Código")]
	[Required(ErrorMessage = "El codigo es requerido.")]
	[StringLength(50, ErrorMessage = "El codigo no puede exceder los 50 caracteres.")]
	public string Codigo { get; set; }

	[Display(Name = "Código de activo")]
	[Required(ErrorMessage = "El codigo de activo es requerido.")]
	[StringLength(50, ErrorMessage = "El codigo de activo no puede exceder los 50 caracteres.")]
	public string CodigoActivo { get; set; }

	[Display(Name = "Fecha de adquisición")]
	[Required(ErrorMessage = "La fecha de adquisición es requerida")]
	[DataType(DataType.Date, ErrorMessage = "La fecha no tiene formato correcto.")]
	public DateTime FechaAdquisicion { get; set; }

	[Display(Name = "Tipo de máquina")]
	[Required(ErrorMessage = "Debe indicar el tipo de máquina")]
	public string IdTipoMaquina { get; set; }

	public bool Activo { get; set; }

	public Maquina Entidad() => new(new Guid(Id), Codigo, Nombre, CodigoActivo, Activo, Convert.ToDateTime(FechaAdquisicion), new Guid(IdTipoMaquina));
}

public class EliminarMaquinaViewModel
{
	public EliminarMaquinaViewModel() { }

	public EliminarMaquinaViewModel(Maquina maquina)
	{
		Id = maquina.Id.ToString();
		Nombre = maquina.Nombre;
	}

	[Required(ErrorMessage = "El id es requerido.")]
	public string Id { get; set; }

	public string Nombre { get; set; }
}

public class TipoMaquinaViewModel : BaseViewModel
{
	public TipoMaquinaViewModel(TipoMaquina tipoMaquina) : base(tipoMaquina)
	{
		IdTipoMaquina = tipoMaquina.Id.ToString();
		Nombre = tipoMaquina.Nombre;
		Codigo = tipoMaquina.Codigo;
		Estado = tipoMaquina.Estado ? "Activo" : "Inactivo";
	}

	public string IdTipoMaquina { get; set; }

	[Display(Name = "Tipo de máquina")]
	public string Nombre { get; set; }

	public string Estado { get; set; }

	[Display(Name = "Código")]
	public string Codigo { get; set; }
}

public class AgregarTipoMaquinaViewModel
{
	[Display(Name = "Nombre del tipo de máquina")]
	[Required(ErrorMessage = "El nombre es requerido")]
	[StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
	public string Nombre { get; set; }

	[Display(Name = "Activo")]
	public bool Estado { get; set; }

	[Display(Name = "Código del tipo de máquina")]
	[Required(ErrorMessage = "El código es requerido")]
	[StringLength(20, ErrorMessage = "El código no puede exceder los 50 caracteres")]
	public string Codigo { get; set; }

	public TipoMaquina Entidad() => new(Guid.NewGuid(), Nombre, Estado, Codigo);
}

public class EditarTipoMaquinaViewModel : BaseViewModel
{
	public EditarTipoMaquinaViewModel() { }

	public EditarTipoMaquinaViewModel(TipoMaquina tipoMaquina) : base(tipoMaquina)
	{
		Id = tipoMaquina.Id.ToString();
		Nombre = tipoMaquina.Nombre;
		Estado = tipoMaquina.Estado;
		Codigo = tipoMaquina.Codigo;
	}

	public string Id { get; set; }

	[Display(Name = "Nombre del tipo de máquina")]
	[Required(ErrorMessage = "El nombre es requerido")]
	[StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
	public string Nombre { get; set; }

	[Display(Name = "Activo")]
	public bool Estado { get; set; }

	[Display(Name = "Código del tipo de máquina")]
	[Required(ErrorMessage = "El código es requerido")]
	[StringLength(20, ErrorMessage = "El código no puede exceder los 50 caracteres")]
	public string Codigo { get; set; }

	public TipoMaquina Entidad() => new(new Guid(Id), Nombre, Estado, Codigo);
}

public class EliminarTipoMaquinaViewModel
{
	public EliminarTipoMaquinaViewModel() { }

	public EliminarTipoMaquinaViewModel(TipoMaquina tipoMaquina)
	{
		Id = tipoMaquina.Id.ToString();
		Nombre = tipoMaquina.Nombre;
	}

	[Required(ErrorMessage = "El id es requerido.")]
	public string Id { get; set; }

	public string Nombre { get; set; }
}
