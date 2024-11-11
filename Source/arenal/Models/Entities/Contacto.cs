using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace arenal.Models.Entities;

[Table("CONTACTOS", Schema = "fitcare")]
public class Contacto
{
	private Contacto() { }

	public Contacto(Guid id, string nombre, string correo, string telefono, string mensaje)
	{
		Id = id;
		NombreCompleto = nombre;
		CorreoElectronico = correo;
		Telefono = telefono;
		Mensaje = mensaje;
	}

	[Key]
	public Guid Id { get; private set; } = Guid.NewGuid();
	public string NombreCompleto { get; private set; }
	public string CorreoElectronico { get; private set; }
	public string Telefono { get; private set; }
	public string Mensaje { get; private set; }
	public DateTime FechaRegistro { get; private set; } = DateTime.Now;
}
