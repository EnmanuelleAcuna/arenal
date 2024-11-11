using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace arenal.Models.Entities;

[Table("MAQUINAS", Schema = "fitcare")]
public class Maquina : Base
{
	public Maquina() : base()
	{
		// DetalleRutina = new HashSet<DetalleRutina>();
	}

	public Maquina(Guid id, string codigo, string nombre, string codigoActivo, bool activo, DateTime fechaAdquisicion, TipoMaquina tipoMaquina)
	{
		Id = id;
		Nombre = nombre;
		Codigo = codigo;
		CodigoActivo = codigoActivo;
		Estado = activo;
		FechaAdquisicion = fechaAdquisicion;

		IdTipoMaquina = tipoMaquina.Id;
		TipoMaquina = tipoMaquina;

		// DetalleRutina = new HashSet<DetalleRutina>();
	}

	public Maquina(Guid id, string codigo, string nombre, string codigoActivo, bool activo, DateTime fechaAdquisicion, Guid idTipoMaquina)
	{
		Id = id;
		Nombre = nombre;
		Codigo = codigo;
		CodigoActivo = codigoActivo;
		Estado = activo;
		FechaAdquisicion = fechaAdquisicion;

		IdTipoMaquina = idTipoMaquina;

		// DetalleRutina = new HashSet<DetalleRutina>();
	}

	public Maquina(Guid id)
	{
		Id = id;

		// DetalleRutina = new HashSet<DetalleRutina>();
	}

	public Guid Id { get; private set; }
	public string Codigo { get; set; }
	public string Nombre { get; set; }

	[Column("NumeroActivo")]
	public string CodigoActivo { get; set; }

	public bool Estado { get; set; }
	public DateTime FechaAdquisicion { get; set; }

	[ForeignKey(nameof(TipoMaquina))]
	[Column("Id_Tipo_Maquina")]
	public Guid IdTipoMaquina { get; set; }
	public TipoMaquina TipoMaquina { get; set; }

	// public virtual ICollection<DetalleRutina> DetalleRutina { get; set; }

	public override string ToString() => JsonSerializer.Serialize(this);
}

[Table("TIPOS_MAQUINA", Schema = "fitcare")]
public class TipoMaquina : Base
{
	public TipoMaquina() : base()
	{
		Maquinas = new HashSet<Maquina>();
	}

	public TipoMaquina(Guid id, string nombre, bool estado, string codigo)
	{
		Id = id;
		Nombre = nombre;
		Estado = estado;
		Codigo = codigo;

		Maquinas = new HashSet<Maquina>();
	}

	public TipoMaquina(Guid id)
	{
		Id = id;

		Maquinas = new HashSet<Maquina>();
	}

	public Guid Id { get; set; }
	public string Nombre { get; set; }
	public string Codigo { get; set; }
	public bool Estado { get; set; }

	public ICollection<Maquina> Maquinas { get; set; }

	public override string ToString() => JsonSerializer.Serialize(this);
}
