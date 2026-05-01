namespace plani.Models.Domain;

/// <summary>
/// Estados posibles de una sesión de trabajo
/// </summary>
public enum EstadoSesion
{
    /// <summary>
    /// Sesión en curso (el contador está corriendo)
    /// </summary>
    Activa = 1,

    /// <summary>
    /// Sesión pausada (el contador está detenido temporalmente)
    /// </summary>
    Pausada = 2,

    /// <summary>
    /// Sesión finalizada (no se puede modificar)
    /// </summary>
    Finalizada = 3
}
