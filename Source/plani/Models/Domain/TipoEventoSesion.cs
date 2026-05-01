namespace plani.Models.Domain;

/// <summary>
/// Tipos de eventos que pueden ocurrir en una sesión de trabajo
/// </summary>
public enum TipoEventoSesion
{
    /// <summary>
    /// Inicio de la sesión
    /// </summary>
    Inicio = 1,

    /// <summary>
    /// Pausa de la sesión (se calcula tiempo desde inicio o última reanudación)
    /// </summary>
    Pausa = 2,

    /// <summary>
    /// Reanudación de la sesión (continúa después de una pausa)
    /// </summary>
    Reanudacion = 3,

    /// <summary>
    /// Finalización de la sesión (se calcula tiempo si no estaba pausada)
    /// </summary>
    Finalizacion = 4
}
