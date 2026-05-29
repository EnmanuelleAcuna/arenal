using plani.Identity;

namespace plani.Models;

public static class Utils {
    public const string MensajeModeloNulo = "Modelo nulo.";
    public const string MensajeModeloInvalido = "Modelo inválido.";
    public const string MensajeListaNula = "Lista nula.";
    public const string MensajeParametroNulo = "Parámetro nulo.";
    public const string MensajeParametroNuloVacio = "Parámetro nulo o vacío.";
    public const string MensajeErrorObjetoYaEliminado = "El elemento ya está eliminado.";
    public const string MensajeArchivoNulo = "El archivo proporcionado es inválido o nulo.";
    public const string MensajeErrorGuardandoArchivo = "Error guardando archivo.";
    public const string MensajeErrorGuardandoEnBD = "Error guardando en base de datos.";

    public static string MensajeErrorAgregar(string nombreObjeto) {
        return MensajeErrorCrear(nombreObjeto: nombreObjeto);
    }

    public static string MensajeErrorCrear(string nombreObjeto) {
        return string.Concat("Error al crear ", nombreObjeto.ToLower());
    }

    public static string MensajeErrorEditar(string nombreObjeto) {
        return MensajeErrorEditar(nombreObjeto: nombreObjeto);
    }

    public static string MensajeErrorActualizar(string nombreObjeto) {
        return string.Concat("Error al actualizar ", nombreObjeto.ToLower());
    }

    public static string MensajeErrorBorrar(string nombreObjeto) {
        return MensajeErrorEliminar(nombreObjeto: nombreObjeto);
    }

    public static string MensajeErrorEliminar(string nombreObjeto) {
        return string.Concat("Error al eliminar ", nombreObjeto.ToLower());
    }

    public static void ValidateGuid(Guid idObjeto) {
        if (string.IsNullOrEmpty(idObjeto.ToString())) {
            throw new ArgumentNullException(nameof(idObjeto), message: MensajeParametroNuloVacio);
        }

        if (idObjeto.Equals(g: Guid.Empty)) {
            throw new ArgumentException(paramName: nameof(idObjeto), message: MensajeParametroNuloVacio);
        }
    }

    public static void ValidateString(string texto, string nombrePropiedad) {
        if (string.IsNullOrEmpty(value: texto)) {
            throw new ArgumentNullException(paramName: nombrePropiedad, message: MensajeParametroNulo);
        }
    }

    public static void ValidateList<T>(IEnumerable<T> lista, string nombreLista) {
        if (lista is null) {
            throw new ArgumentNullException(paramName: nombreLista, message: MensajeListaNula);
        }
    }

    public static void ValidateCollection<T>(ICollection<T> coleccion, string nombreLista) {
        if (coleccion is null) {
            throw new ArgumentNullException(paramName: nombreLista, message: MensajeListaNula);
        }
    }

    public static bool IdValido(this string id) {
        if (string.IsNullOrEmpty(value: id)) {
            return false;
        }

        if (id.Equals("0", comparisonType: StringComparison.OrdinalIgnoreCase)) {
            return false;
        }

        if (!int.TryParse(s: id, result: out _)) {
            return false;
        }

        return true;
    }

    public static List<ApplicationUser> GetUsuariosDisponiblesParainstructor(this IList<ApplicationUser> usuarios,
        IList<ApplicationUser> usuariosInstructor) {
        List<ApplicationUser> usuariosDisponiblesParaSerInstructores = new();

        foreach (ApplicationUser usuario in usuarios) {
            if (!usuariosInstructor.Any(x => x.Id == usuario.Id)) {
                usuariosDisponiblesParaSerInstructores.Add(item: usuario);
            }
        }

        return usuariosDisponiblesParaSerInstructores;
    }
}