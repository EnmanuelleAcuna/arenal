using Microsoft.AspNetCore.Mvc;
using plani.Models.ViewModels;

namespace plani.Models.ViewComponents;

/// <summary>
///     View Component para breadcrumbs.
///     Acepta breadcrumbs personalizados via ViewBag o genera breadcrumbs por defecto.
/// </summary>
public class BreadcrumbsViewComponent : ViewComponent {
    /// <summary>
    ///     Invoca el componente de breadcrumbs.
    ///     Lee breadcrumbs del ViewBag o genera uno por defecto.
    /// </summary>
    public IViewComponentResult Invoke() {
        BreadcrumbsViewModel viewModel = new() {
            Icon = ViewBag.BreadcrumbIcon ?? "fa-tachometer-alt",
            Breadcrumbs = GetBreadcrumbs()
        };

        return View(model: viewModel);
    }

    /// <summary>
    ///     Obtiene los breadcrumbs del ViewBag o genera uno por defecto basado en la acción actual
    /// </summary>
    private List<string> GetBreadcrumbs() {
        // Intentar obtener breadcrumbs desde ViewBag
        if (ViewBag.Breadcrumbs is string[] customBreadcrumbs && customBreadcrumbs.Length > 0) {
            return customBreadcrumbs.ToList();
        }

        // Si no hay breadcrumbs personalizados, generar uno basado en la acción
        string controller = ViewContext.RouteData.Values["controller"]?.ToString() ?? "";
        string action = ViewContext.RouteData.Values["action"]?.ToString() ?? "";

        return GenerateDefaultBreadcrumbs(controller: controller, action: action);
    }

    /// <summary>
    ///     Genera breadcrumbs por defecto basándose en el controlador y acción actuales
    /// </summary>
    private static List<string> GenerateDefaultBreadcrumbs(string controller, string action) {
        // Mapeo de acciones conocidas a breadcrumbs amigables
        Dictionary<string, string> breadcrumbMap = new() {
            ["Administracion"] = "Panel de administración",
            ["Areas"] = "Áreas de servicio",
            ["Modalidades"] = "Modalidades",
            ["Servicios"] = "Servicios",
            ["Clientes"] = "Clientes",
            ["TiposCliente"] = "Tipos de cliente",
            ["Proyectos"] = "Proyectos",
            ["Usuarios"] = "Usuarios",
            ["Roles"] = "Roles",
            ["Colaboradores"] = "Colaboradores",
            ["MisAsignaciones"] = "Mis asignaciones",
            ["Asignaciones"] = "Asignaciones",
            ["MisSesiones"] = "Mis sesiones",
            ["Sesiones"] = "Sesiones"
        };

        // Retornar breadcrumb mapeado o por defecto
        return new List<string> {
            breadcrumbMap.TryGetValue(key: action, out string breadcrumb)
                ? breadcrumb
                : "Panel de administración"
        };
    }
}