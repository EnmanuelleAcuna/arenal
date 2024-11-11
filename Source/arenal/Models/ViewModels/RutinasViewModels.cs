using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using arenal.Models.Entities;
using arenal.Models.Identity;
using Microsoft.Extensions.Configuration;

namespace arenal.Models.ViewModels;

public class RutinaViewModel
{
	public RutinaViewModel(Rutina rutina)
	{
		Id = rutina.Id.ToString();

		Instructor = rutina.Instructor.FullName;
		Cliente = rutina.Cliente.FullName;

		FechaRealizacion = rutina.FechaRealizacion;
		FechaInicio = rutina.FechaInicio;
		FechaFin = rutina.FechaFin;
		Objetivos = rutina.Objetivo;

		CantidadEjercicios = rutina.Ejercicios?.Count;
	}

	public string Id { get; set; }

	[Display(Name = "Instructor")]
	public string Instructor { get; set; }

	[Display(Name = "Cliente")]
	public string Cliente { get; set; }

	[Display(Name = "Fecha de realización")]
	public DateTime FechaRealizacion { get; set; }

	[Display(Name = "Fecha de inicio")]
	public DateTime FechaInicio { get; set; }

	[Display(Name = "Fecha de finalización")]
	public DateTime FechaFin { get; set; }

	public string Objetivos { get; set; }

	public int? CantidadEjercicios { get; set; } = 0;
}

public class DetalleRutinaViewModel
{
	public DetalleRutinaViewModel(Rutina rutina)
	{
		Id = rutina.Id.ToString();

		Instructor = rutina.Instructor.FullName;
		Cliente = rutina.Cliente.FullName;

		FechaRealizacion = rutina.FechaRealizacion;
		FechaInicio = rutina.FechaInicio;
		FechaFin = rutina.FechaFin;
		Objetivos = rutina.Objetivo;

		CantidadEjercicios = rutina.Ejercicios?.Count;

		Ejercicios = rutina.Ejercicios.Select(e => new EjercicioRutinaViewModel(e)).ToList();

		Medidas = rutina.Medidas.Select(m => new MedidaRutinaViewModel(m)).ToList();
	}

	public string Id { get; set; }

	[Display(Name = "Instructor")]
	public string Instructor { get; set; }

	[Display(Name = "Cliente")]
	public string Cliente { get; set; }

	[Display(Name = "Fecha de realización")]
	public DateTime FechaRealizacion { get; set; }

	[Display(Name = "Fecha de inicio")]
	public DateTime FechaInicio { get; set; }

	[Display(Name = "Fecha de finalización")]
	public DateTime FechaFin { get; set; }

	public string Objetivos { get; set; }

	public int? CantidadEjercicios { get; set; } = 0;

	public IEnumerable<EjercicioRutinaViewModel> Ejercicios { get; set; }

	public IEnumerable<MedidaRutinaViewModel> Medidas { get; set; }
}

public class AgregarRutinaViewModel
{
	[Required(ErrorMessage = "El instructor es requerido.")]
	public string IdInstructor { get; set; }

	[Required(ErrorMessage = "El cliente es requerido.")]
	public string IdCliente { get; set; }

	[Display(Name = "Realización")]
	[Required(ErrorMessage = "La fecha de realización es requerida.")]
	[DataType(DataType.Date)]
	public DateTime FechaRealizacion { get; set; } = DateTime.Now;

	[Display(Name = "Inicio")]
	[Required(ErrorMessage = "La fecha de inicio es requerida")]
	[DataType(DataType.Date)]
	public DateTime FechaInicio { get; set; } = DateTime.Now;

	[Display(Name = "Finalización")]
	[Required(ErrorMessage = "La fecha de finalización es requerida")]
	[DataType(DataType.Date)]
	public DateTime FechaFin { get; set; } = DateTime.Now.AddMonths(1);

	[Display(Name = "Objetivo")]
	public string Objetivo { get; set; }

	public List<EjercicioRutinaViewModel> DetalleEjercicios { get; set; }
	public List<MedidaRutinaViewModel> DetalleMedidas { get; set; }

	public Rutina Entidad(ApplicationUser instructor, ApplicationUser cliente)
	{
		IList<EjercicioRutina> ejercicios = DetalleEjercicios.Select(x => x.Entidad()).ToList();
		IList<MedidaRutina> medidas = DetalleMedidas.Select(x => x.Entidad()).ToList();

		Rutina rutina = new(Guid.NewGuid(), FechaRealizacion, FechaInicio, FechaFin, Objetivo, instructor, cliente, ejercicios, medidas);
		return rutina;
	}
}

public class EjercicioRutinaViewModel
{
	public EjercicioRutinaViewModel() { }

	public EjercicioRutinaViewModel(EjercicioRutina ejercicioRutina)
	{
		Id = ejercicioRutina.Id.ToString();
		IdRutina = ejercicioRutina.IdRutina.ToString();
		IdEjercicio = ejercicioRutina.Ejercicio.Id.ToString();
		NombreEjercicio = ejercicioRutina.Ejercicio?.Nombre;
		IdMaquina = ejercicioRutina.IdMaquina.ToString();
		NombreMaquina = ejercicioRutina.Maquina?.Nombre;
		Series = ejercicioRutina.Series;
		Repeticiones = ejercicioRutina.Repeticiones;
		MinutosDescanso = ejercicioRutina.MinutosDescanso;
	}

	public string Id { get; set; }
	public string IdRutina { get; set; }
	public string IdEjercicio { get; set; }
	public string NombreEjercicio { get; set; }
	public string IdMaquina { get; set; }
	public string NombreMaquina { get; set; }
	public int Series { get; set; }
	public int Repeticiones { get; set; }
	public int MinutosDescanso { get; set; }
	public string IdTipoMaquina { get; set; }

	public EjercicioRutina Entidad()
	{
		Ejercicio ejercicio = new Ejercicio(new Guid(IdEjercicio));
		Maquina maquina = new Maquina(new Guid(IdMaquina));
		EjercicioRutina ejercicioRutina = new(Guid.NewGuid(), Series, Repeticiones, MinutosDescanso, ejercicio, maquina);
		return ejercicioRutina;
	}
}

public class MedidaRutinaViewModel
{
	public MedidaRutinaViewModel() { }

	public MedidaRutinaViewModel(MedidaRutina medidaRutina)
	{
		Id = medidaRutina.Id.ToString();
		IdRutina = medidaRutina.IdRutina.ToString();
		IdTipoMedida = medidaRutina.TipoMedida.Id.ToString();
		NombreTipoMedida = medidaRutina.TipoMedida.Nombre;
		Valor = medidaRutina.Valor;
		Comentario = medidaRutina.Comentario;
	}

	public string Id { get; set; }
	public string IdRutina { get; set; }
	public string IdTipoMedida { get; set; }
	public string NombreTipoMedida { get; set; }
	public string Valor { get; set; }
	public string Comentario { get; set; }

	public MedidaRutina Entidad()
	{
		TipoMedida tipoMedida = new TipoMedida(new Guid(IdTipoMedida));
		MedidaRutina medidaRutina = new(Guid.NewGuid(), Valor, Comentario, tipoMedida);
		return medidaRutina;
	}
}

public class ReporteRutinaResumidoViewModel
{
	public ReporteRutinaResumidoViewModel() { }
	
	public ReporteRutinaResumidoViewModel(ReporteRutinaResumido rutina, IConfiguration configuration)
	{
		if (rutina is null)
			throw new ArgumentNullException(paramName: nameof(rutina), message: configuration["AppSettings:ModeloNulo"]);

		NombreInstructor = rutina.NombreInstructor;
		NombreCliente = rutina.NombreCliente;
		FechaRegistro = rutina.FechaRegistro.ToString("dd/MM/yyyy");
		FechaInicio = rutina.FechaInicio.ToString("dd/MM/yyyy");
		FechaFin = rutina.FechaFin.ToString("dd/MM/yyyy");
		Objetivo = rutina.Objetivo;
		DiasRutina = rutina.DiasRutina;
		CantidadEjerciciosRegistrados = rutina.CantidadEjerciciosRegistrados;
	}

	[Display(Name = "Instructor")]
	public string NombreInstructor { get; set; }

	[Display(Name = "Cliente")]
	public string NombreCliente { get; set; }

	[Display(Name = "Registro de rutina")]
	public string FechaRegistro { get; set; }

	[Display(Name = "Inicio")]
	public string FechaInicio { get; set; }

	[Display(Name = "Finalización")]
	public string FechaFin { get; set; }

	public string Objetivo { get; set; }

	[Display(Name = "Cantidad de días")]
	public int DiasRutina { get; set; }

	[Display(Name = "Ejercicios registrados")]
	public int CantidadEjerciciosRegistrados { get; set; }
}

public class ReporteRutinaResumido
{
	public string Id { get; set; }
	public string NombreInstructor { get; set; }
	public string NombreCliente { get; set; }
	public DateTime FechaRegistro { get; set; }
	public DateTime FechaInicio { get; set; }
	public DateTime FechaFin { get; set; }
	public string Objetivo { get; set; }
	public int DiasRutina { get; set; }
	public int CantidadEjerciciosRegistrados { get; set; }

	// Sobreescribir metodo ToString() de la clase para devolver el objeto en una cadena string en formato JSON
	public override string ToString() => JsonSerializer.Serialize(this);
}
