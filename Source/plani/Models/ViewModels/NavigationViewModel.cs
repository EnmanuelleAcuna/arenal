namespace plani.Models.ViewModels;

/// <summary>
/// ViewModel para el componente de navegación lateral (sidebar).
/// </summary>
public class NavigationViewModel
{
    /// <summary>
    /// Lista de secciones de navegación que se mostrarán en el sidebar
    /// </summary>
    public List<NavigationSection> Sections { get; set; } = new();

    /// <summary>
    /// Controlador actualmente activo
    /// </summary>
    public string CurrentController { get; set; } = "";

    /// <summary>
    /// Acción actualmente activa
    /// </summary>
    public string CurrentAction { get; set; } = "";
}

/// <summary>
/// Representa una sección del menú de navegación
/// </summary>
public class NavigationSection
{
    /// <summary>
    /// Título de la sección (ej: "Gestión de Clientes")
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Ícono de Font Awesome (ej: "fa-building")
    /// </summary>
    public string Icon { get; set; } = "";

    /// <summary>
    /// Indica si la sección tiene submenús
    /// </summary>
    public bool HasSubmenu { get; set; }

    /// <summary>
    /// ID único para el submenú (ej: "submenu-clientes")
    /// </summary>
    public string SubmenuId { get; set; } = "";

    /// <summary>
    /// Link directo (si no tiene submenú)
    /// </summary>
    public NavigationLink DirectLink { get; set; }

    /// <summary>
    /// Lista de items del submenú
    /// </summary>
    public List<NavigationLink> SubmenuItems { get; set; } = new();
}

/// <summary>
/// Representa un link de navegación
/// </summary>
public class NavigationLink
{
    /// <summary>
    /// Texto a mostrar
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// Controlador al que apunta
    /// </summary>
    public string Controller { get; set; } = "";

    /// <summary>
    /// Acción a la que apunta
    /// </summary>
    public string Action { get; set; } = "";

    /// <summary>
    /// Ícono (opcional, para submenús)
    /// </summary>
    public string Icon { get; set; }
}
