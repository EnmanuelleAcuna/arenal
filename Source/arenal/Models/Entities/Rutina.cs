using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using arenal.Models.Identity;

namespace arenal.Models.Entities;

[Table("RUTINAS", Schema = "fitcare")]
public class Rutina : Base
{
	public Rutina() : base()
	{
		Medidas = new HashSet<MedidaRutina>();
		Ejercicios = new HashSet<EjercicioRutina>();
	}

	public Rutina(Guid id, DateTime fechaRealizacion, DateTime fechaInicio, DateTime fechaFin, string objetivo, ApplicationUser instructor, ApplicationUser cliente, IList<EjercicioRutina> ejercicios, IList<MedidaRutina> medidas)
	{
		Id = id;

		FechaRealizacion = fechaRealizacion;
		FechaInicio = fechaInicio;
		FechaFin = fechaFin;
		Objetivo = objetivo;

		IdInstructor = instructor.Id;
		Instructor = instructor;

		IdCliente = cliente.Id;
		Cliente = cliente;

		Medidas = medidas;
		Ejercicios = ejercicios;
	}

	public Rutina(Guid id, DateTime fechaRealizacion, DateTime fechaInicio, DateTime fechaFin, string objetivo, string idInstructor, string idCliente)
	{
		Id = id;
		FechaRealizacion = fechaRealizacion;
		FechaInicio = fechaInicio;
		FechaFin = fechaFin;
		Objetivo = objetivo;

		IdInstructor = idInstructor;
		IdCliente = idCliente;

		Medidas = new HashSet<MedidaRutina>();
		Ejercicios = new HashSet<EjercicioRutina>();
	}

	public Rutina(Guid id)
	{
		Id = id;

		Medidas = new HashSet<MedidaRutina>();
		Ejercicios = new HashSet<EjercicioRutina>();
	}

	[Key]
	public Guid Id { get; set; }

	[Column("Fecha_Realizacion")]
	public DateTime FechaRealizacion { get; set; }

	[Column("Fecha_Inicio")]
	public DateTime FechaInicio { get; set; }

	[Column("Fecha_Fin")]
	public DateTime FechaFin { get; set; }

	public string Objetivo { get; set; }

	public string IdInstructor { get; set; }

	[ForeignKey("IdInstructor")]
	[InverseProperty("RutinasInstructor")]
	public virtual ApplicationUser Instructor { get; set; }

	public string IdCliente { get; set; }

	[ForeignKey("IdCliente")]
	[InverseProperty("RutinasCliente")]
	public virtual ApplicationUser Cliente { get; set; }

	public ICollection<EjercicioRutina> Ejercicios { get; set; }
	public ICollection<MedidaRutina> Medidas { get; set; }

	public override string ToString() => JsonSerializer.Serialize(this);
}

[Table("DETALLE_RUTINA", Schema = "fitcare")]
public class EjercicioRutina : Base
{
	public EjercicioRutina() : base() { }

	public EjercicioRutina(Guid id, Guid idRutina, int series, int repeticiones, int minutosDescanso, Ejercicio ejercicio, Maquina maquina)
	{
		Id = id;
		IdRutina = idRutina;
		Series = series;
		Repeticiones = repeticiones;
		MinutosDescanso = minutosDescanso;
		IdEjercicio = ejercicio.Id;
		Ejercicio = ejercicio;
		IdMaquina = maquina.Id;
		Maquina = maquina;
	}

	public EjercicioRutina(Guid id, int series, int repeticiones, int minutosDescanso, Ejercicio ejercicio, Maquina maquina)
	{
		Id = id;
		Series = series;
		Repeticiones = repeticiones;
		MinutosDescanso = minutosDescanso;
		IdEjercicio = ejercicio.Id;
		Ejercicio = ejercicio;
		IdMaquina = maquina.Id;
		Maquina = maquina;
	}

	[Key]
	public Guid Id { get; set; }

	[Column("Num_Series")]
	public int Series { get; set; }

	[Column("Num_Repeticiones")]
	public int Repeticiones { get; set; }

	[Column("Minutos_Descanso")]
	public int MinutosDescanso { get; set; }

	[ForeignKey(nameof(Rutina))]
	[Column("IdRutina")]
	public Guid IdRutina { get; set; }
	public Rutina Rutina { get; set; }

	[ForeignKey(nameof(Ejercicio))]
	[Column("IdEjercicio")]
	public Guid IdEjercicio { get; set; }
	public Ejercicio Ejercicio { get; set; }

	[ForeignKey(nameof(Maquina))]
	[Column("IdMaquina")]
	public Guid? IdMaquina { get; set; }
	public Maquina Maquina { get; set; }

	public override string ToString() => JsonSerializer.Serialize(this);
}

[Table("DETALLE_MEDIDAS", Schema = "fitcare")]
public class MedidaRutina : Base
{
	public MedidaRutina() : base() { }

	public MedidaRutina(Guid id, Guid idRutina, string valor, string comentario, TipoMedida tipoMedida){
		Id = id;
		IdRutina = idRutina;
		Valor = valor;
		Comentario = comentario;
		IdTipoMedida = tipoMedida.Id;
		TipoMedida = tipoMedida;
	}

	public MedidaRutina(Guid id, string valor, string comentario, TipoMedida tipoMedida)
	{
		Id = id;
		Valor = valor;
		Comentario = comentario;
		IdTipoMedida = tipoMedida.Id;
		TipoMedida = tipoMedida;
	}

	[Key]
	public Guid Id { get; set; }

	[ForeignKey(nameof(Rutina))]
	[Column("IdRutina")]
	public Guid IdRutina { get; set; }
	public Rutina Rutina { get; set; }

	[Column("ValorMedida")]
	public string Valor { get; set; }

	public string Comentario { get; set; }

	[ForeignKey(nameof(TipoMedida))]
	[Column("IdTipoMedida")]
	public Guid IdTipoMedida { get; set; }
	public TipoMedida TipoMedida { get; set; }

	public override string ToString() => JsonSerializer.Serialize(this);
}
