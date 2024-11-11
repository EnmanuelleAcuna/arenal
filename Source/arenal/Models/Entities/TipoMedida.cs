using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace arenal.Models.Entities;

[Table("TIPOS_MEDIDA", Schema = "fitcare")]
public class TipoMedida : Base
{
	public TipoMedida() : base()
	{
		// DetalleMedidas = new HashSet<DetalleMedidas>();
	}

	public TipoMedida(Guid id, string codigo, string nombre, bool estado)
	{
		Id = id;
		Codigo = codigo;
		Nombre = nombre;
		Estado = estado;

		// DetalleMedidas = new HashSet<DetalleMedidas>();
	}

	public TipoMedida(Guid id)
	{
		Id = id;

		// DetalleMedidas = new HashSet<DetalleMedidas>();
	}

	public Guid Id { get; set; }
	public string Codigo { get; set; }
	public string Nombre { get; set; }
	public bool Estado { get; set; }

	// public virtual ICollection<DetalleMedidas> DetalleMedidas { get; set; }

	public override string ToString() => JsonSerializer.Serialize(this);
}
