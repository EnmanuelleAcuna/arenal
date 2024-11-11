using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace arenal.Models.Entities;

[Table("GRUPOS_MUSCULARES", Schema = "fitcare")]
public class GrupoMuscular : Base
{
	public GrupoMuscular() : base() { }

	public GrupoMuscular(Guid id, string nombre, string descripcion)
	{
		Id = id;
		Nombre = nombre;
		Descripcion = descripcion;
	}

	public GrupoMuscular(Guid id)
	{
		Id = id;
	}

	public Guid Id { get; set; }
	public string Nombre { get; set; }
	public string Descripcion { get; set; }

	public override string ToString() => JsonSerializer.Serialize(this);
}
