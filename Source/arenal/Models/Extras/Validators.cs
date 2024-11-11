using System;
using System.Collections.Generic;
using arenal.Models.Entities;

namespace arenal.Models.Extras;

public static class Validators
{
	public static void ValidateGuid(Guid idObjeto)
	{
		if (string.IsNullOrEmpty(idObjeto.ToString())) throw new ArgumentNullException(paramName: nameof(idObjeto), message: Messages.MensajeParametroNuloVacio);

		if (idObjeto.Equals(Guid.Empty)) throw new ArgumentException(paramName: nameof(idObjeto), message: Messages.MensajeParametroNuloVacio);
	}

	public static void ValidateString(string texto, string nombrePropiedad)
	{
		if (String.IsNullOrEmpty(texto))
		{
			throw new ArgumentNullException(paramName: nombrePropiedad, message: Messages.MensajeParametroNulo);
		}
	}

	public static void ValidateList<T>(IEnumerable<T> lista, string nombreLista)
	{
		if (lista is null)
		{
			throw new ArgumentNullException(paramName: nombreLista, message: Messages.MensajeListaNula);
		}
	}

	public static void ValidateCollection<T>(ICollection<T> coleccion, string nombreLista)
	{
		if (coleccion is null)
		{
			throw new ArgumentNullException(paramName: nombreLista, message: Messages.MensajeListaNula);
		}
	}

	public static void ValidateProvincia(Provincia provincia)
	{
		if (provincia is null)
		{
			throw new ArgumentNullException(paramName: nameof(provincia), message: Messages.MensajeModeloNulo);
		}
	}

	public static void ValidateCanton(Canton canton)
	{
		if (canton is null)
		{
			throw new ArgumentNullException(paramName: nameof(canton), message: Messages.MensajeModeloNulo);
		}

		ValidateProvincia(canton.Provincia);
	}

	public static void ValidateDistrito(Distrito distrito)
	{
		if (distrito is null)
		{
			throw new ArgumentNullException(paramName: nameof(distrito), message: Messages.MensajeModeloNulo);
		}

		ValidateCanton(distrito.Canton);
	}

	public static void ValidateContacto(Contacto contacto)
	{
		if (contacto is null)
		{
			throw new ArgumentNullException(paramName: nameof(contacto), message: Messages.MensajeModeloNulo);
		}
	}

	public static void ValidateTipoMaquina(TipoMaquina tipoMaquina)
	{
		if (tipoMaquina is null)
		{
			throw new ArgumentNullException(paramName: nameof(tipoMaquina), message: Messages.MensajeModeloNulo);
		}
	}

	public static void ValidateMaquina(Maquina maquina)
	{
		if (maquina is null)
		{
			throw new ArgumentNullException(paramName: nameof(maquina), message: Messages.MensajeModeloNulo);
		}

		ValidateTipoMaquina(maquina.TipoMaquina);
	}

	public static void ValidateGrupoMuscular(GrupoMuscular grupoMuscular)
	{
		if (grupoMuscular is null)
		{
			throw new ArgumentNullException(paramName: nameof(grupoMuscular), message: Messages.MensajeModeloNulo);
		}
	}

	public static void ValidateTipoEjercicio(TipoEjercicio tipoEjercicio)
	{
		if (tipoEjercicio is null)
		{
			throw new ArgumentNullException(paramName: nameof(tipoEjercicio), message: Messages.MensajeModeloNulo);
		}

		if (string.IsNullOrEmpty(tipoEjercicio.Nombre))
		{
			throw new ArgumentNullException(paramName: nameof(tipoEjercicio.Nombre), message: Messages.MensajeParametroNulo);
		}
	}

	public static void ValidateEjercicio(Ejercicio ejercicio)
	{
		if (ejercicio is null)
		{
			throw new ArgumentNullException(paramName: nameof(ejercicio), message: Messages.MensajeModeloNulo);
		}

		ValidateTipoEjercicio(ejercicio.TipoEjercicio);
	}

	public static void ValidateTipoMedida(TipoMedida tipoMedida)
	{
		if (tipoMedida is null)
		{
			throw new ArgumentNullException(paramName: nameof(tipoMedida), message: Messages.MensajeModeloNulo);
		}
	}

	public static void ValidateRutina(Rutina rutina)
	{
		if (rutina is null)
		{
			throw new ArgumentNullException(paramName: nameof(rutina), message: Messages.MensajeModeloNulo);
		}

		ValidateList(rutina.Ejercicios, nameof(rutina.Ejercicios));
		ValidateList(rutina.Medidas, nameof(rutina.Medidas));
	}

	public static void ValidateEjercicioRutina(EjercicioRutina ejercicioRutina)
	{
		if (ejercicioRutina is null)
		{
			throw new ArgumentNullException(paramName: nameof(ejercicioRutina), message: Messages.MensajeModeloNulo);
		}

		ValidateEjercicio(ejercicioRutina.Ejercicio);
	}

	public static void ValidateMedidaRutina(MedidaRutina medidaRutina)
	{
		if (medidaRutina is null)
		{
			throw new ArgumentNullException(paramName: nameof(medidaRutina), message: Messages.MensajeModeloNulo);
		}

		ValidateTipoMedida(medidaRutina.TipoMedida);
	}
}
