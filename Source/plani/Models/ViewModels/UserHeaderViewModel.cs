using plani.Models.Domain;

namespace plani.Models.ViewModels;

/// <summary>
/// ViewModel para el componente de header de usuario.
/// Contiene información del usuario autenticado para mostrar en el topbar.
/// </summary>
public class UserHeaderViewModel
{
    /// <summary>
    /// Iniciales del usuario (ej: "JM" para Juan Martinez)
    /// </summary>
    public string UserInitials { get; set; } = "";

    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string UserName { get; set; } = "Usuario";

    /// <summary>
    /// Rol de mayor jerarquía del usuario (Administrador > Coordinador > Colaborador)
    /// </summary>
    public string DisplayRole { get; set; } = "Usuario";

    /// <summary>
    /// Indica si el usuario está autenticado
    /// </summary>
    public bool IsAuthenticated { get; set; }
}
