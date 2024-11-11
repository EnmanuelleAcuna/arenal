using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace arenal.Models.Entities;

[Table("EJERCICIOS", Schema = "fitcare")]
public class Ejercicio : Base
{
	public Ejercicio() : base()
	{
		//DetalleRutina = new HashSet<DetalleRutina>();
	}

	public Ejercicio(Guid id, string codigo, string nombre, bool estado, TipoEjercicio tipoEjercicio)
	{
		Id = id;
		Codigo = codigo;
		Nombre = nombre;
		Estado = estado;

		TipoEjercicio = tipoEjercicio;
		IdTipoEjercicio = tipoEjercicio.Id;

		//DetalleRutina = new HashSet<DetalleRutina>();
	}

	public Ejercicio(Guid id, string codigo, string nombre, bool estado, Guid idTipoEjercicio)
	{
		Id = id;
		Codigo = codigo;
		Nombre = nombre;
		Estado = estado;

		IdTipoEjercicio = idTipoEjercicio;

		//DetalleRutina = new HashSet<DetalleRutina>();
	}

	public Ejercicio(Guid id) => Id = id;

	public Guid Id { get; set; }
	public string Codigo { get; set; }
	public string Nombre { get; set; }
	public bool Estado { get; set; }

	[ForeignKey(nameof(TipoEjercicio))]
	[Column("Id_Tipo_Ejercicio")]
	public Guid IdTipoEjercicio { get; set; }
	public TipoEjercicio TipoEjercicio { get; set; }

	// public virtual ICollection<DetalleRutina> DetalleRutina { get; set; }

	public override string ToString() => JsonSerializer.Serialize(this);
}

[Table("TIPOS_EJERCICIO", Schema = "fitcare")]
public class TipoEjercicio : Base
{
	public TipoEjercicio() : base()
	{
		Ejercicios = new HashSet<Ejercicio>();
	}

	public TipoEjercicio(Guid id, string codigo, string nombre, bool estado)
	{
		Id = id;
		Codigo = codigo;
		Nombre = nombre;
		Estado = estado;

		Ejercicios = new HashSet<Ejercicio>();
	}

	public TipoEjercicio(Guid id)
	{
		Id = id;

		Ejercicios = new HashSet<Ejercicio>();
	}

	public Guid Id { get; set; }
	public string Codigo { get; set; }
	public string Nombre { get; set; }
	public bool Estado { get; set; }

	public virtual ICollection<Ejercicio> Ejercicios { get; set; }

	public override string ToString() => JsonSerializer.Serialize(this);
}
