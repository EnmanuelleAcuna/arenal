namespace arenal.Models.Extras;

public static class Messages
{
	public const string MensajeModeloNulo = "Modelo nulo.";
	public const string MensajeModeloInvalido = "Modelo inválido.";
	public const string MensajeListaNula = "Lista nula.";
	public const string MensajeParametroNulo = "Parámetro nulo.";
	public const string MensajeParametroNuloVacio = "Parámetro nulo o vacío.";
	public const string MensajeErrorEnviarInformacion = "Error al enviar la información.";
	public const string MensajeArchivoNulo = "El archivo proporcionado es inválido o nulo.";
	public const string MensajeErrorGuardandoArchivo = "Error guardando archivo.";
	public const string MensajeErrorGuardandoEnBD = "Error guardando en base de datos.";

	public const int CodigoEventoGet = 8001;

	public static string MensajeErrorCrear(string nombreObjeto)
	{
		return string.Concat("Error al crear ", nombreObjeto.ToLower());
	}

	public static string MensajeErrorActualizar(string nombreObjeto)
	{
		return string.Concat("Error al actualizar ", nombreObjeto.ToLower());
	}

	public static string MensajeErrorEliminar(string nombreObjeto)
	{
		return string.Concat("Error al eliminar ", nombreObjeto.ToLower());
	}
}
