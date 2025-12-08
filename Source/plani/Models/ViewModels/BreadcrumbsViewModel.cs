namespace plani.Models.ViewModels;

/// <summary>
/// ViewModel para el componente de breadcrumbs.
/// </summary>
public class BreadcrumbsViewModel
{
    /// <summary>
    /// Ícono de Font Awesome que se muestra antes de los breadcrumbs
    /// </summary>
    public string Icon { get; set; } = "fa-tachometer-alt";

    /// <summary>
    /// Lista de breadcrumbs a mostrar
    /// </summary>
    public List<string> Breadcrumbs { get; set; } = new();
}
