using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using plani.Identity;
using plani.Models.ViewModels;

namespace plani.ViewComponents;

/// <summary>
/// View Component para el sidebar de navegación.
/// Construye el menú dinámicamente basado en los roles del usuario.
/// Solo se muestra si el usuario está autenticado.
/// </summary>
public class NavigationSidebarViewComponent : ViewComponent
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public NavigationSidebarViewComponent(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public IViewComponentResult Invoke()
    {
        // Si el usuario no está autenticado, no mostrar nada
        if (!_signInManager.IsSignedIn(HttpContext.User))
        {
            return Content(string.Empty);
        }

        var viewModel = new NavigationViewModel
        {
            CurrentController = ViewContext.RouteData.Values["controller"]?.ToString() ?? "",
            CurrentAction = ViewContext.RouteData.Values["action"]?.ToString() ?? "",
            Sections = BuildNavigationSections()
        };

        return View(viewModel);
    }

    /// <summary>
    /// Construye todas las secciones de navegación según los permisos del usuario
    /// </summary>
    private List<NavigationSection> BuildNavigationSections()
    {
        var sections = new List<NavigationSection>();

        // Sección: Inicio (visible para todos)
        sections.Add(new NavigationSection
        {
            Title = "",  // Sin título de sección
            Icon = "fa-home",
            HasSubmenu = false,
            DirectLink = new NavigationLink
            {
                Text = "Inicio",
                Controller = "Home",
                Action = "Administracion"
            }
        });

        // Sección: Sesiones de Trabajo
        var sesionesSection = BuildSesionesSection();
        if (sesionesSection.SubmenuItems.Any())
        {
            sections.Add(sesionesSection);
        }

        // Sección: Gestión de Clientes (visible para todos los autenticados)
        sections.Add(new NavigationSection
        {
            Title = "Gestión de Clientes",
            Icon = "fa-building",
            HasSubmenu = true,
            SubmenuId = "submenu-clientes",
            SubmenuItems = new List<NavigationLink>
            {
                new() { Text = "Tipos de cliente", Controller = "Clientes", Action = "TiposCliente" },
                new() { Text = "Clientes", Controller = "Clientes", Action = "Clientes" },
                new() { Text = "Proyectos", Controller = "Clientes", Action = "Proyectos" }
            }
        });

        // Sección: Gestión de Servicios (visible para todos los autenticados)
        sections.Add(new NavigationSection
        {
            Title = "Gestión de Servicios",
            Icon = "fa-briefcase",
            HasSubmenu = true,
            SubmenuId = "submenu-servicios",
            SubmenuItems = new List<NavigationLink>
            {
                new() { Text = "Áreas de servicio", Controller = "Servicios", Action = "Areas" },
                new() { Text = "Modalidades", Controller = "Servicios", Action = "Modalidades" },
                new() { Text = "Servicios", Controller = "Servicios", Action = "Servicios" }
            }
        });

        // Sección: Administración (solo para Administradores)
        if (User.IsInRole("Administrador"))
        {
            sections.Add(new NavigationSection
            {
                Title = "Administración",
                Icon = "fa-users-cog",
                HasSubmenu = true,
                SubmenuId = "submenu-cuentas",
                SubmenuItems = new List<NavigationLink>
                {
                    new() { Text = "Usuarios", Controller = "Cuentas", Action = "Usuarios" },
                    new() { Text = "Roles", Controller = "Cuentas", Action = "Roles" }
                }
            });
        }

        return sections;
    }

    /// <summary>
    /// Construye la sección de "Sesiones de Trabajo" según los roles del usuario
    /// </summary>
    private NavigationSection BuildSesionesSection()
    {
        var submenuItems = new List<NavigationLink>();

        var isAdmin = User.IsInRole("Administrador");
        var isCoordinador = User.IsInRole("Coordinador");
        var isColaborador = User.IsInRole("Colaborador");

        // Items según roles
        if (isAdmin || isCoordinador)
        {
            submenuItems.Add(new NavigationLink
            {
                Text = "Colaboradores",
                Controller = "Cuentas",
                Action = "Colaboradores"
            });
        }

        if (isColaborador || isCoordinador)
        {
            submenuItems.Add(new NavigationLink
            {
                Text = "Mis Asignaciones",
                Controller = "Clientes",
                Action = "MisAsignaciones"
            });
        }

        if (isAdmin || isCoordinador)
        {
            submenuItems.Add(new NavigationLink
            {
                Text = "Asignaciones",
                Controller = "Clientes",
                Action = "Asignaciones"
            });
        }

        if (isColaborador || isCoordinador)
        {
            submenuItems.Add(new NavigationLink
            {
                Text = "Mis Sesiones",
                Controller = "Clientes",
                Action = "MisSesiones"
            });
        }

        if (isAdmin || isCoordinador)
        {
            submenuItems.Add(new NavigationLink
            {
                Text = "Todas las Sesiones",
                Controller = "Clientes",
                Action = "Sesiones"
            });
        }

        return new NavigationSection
        {
            Title = "Sesiones de Trabajo",
            Icon = "fa-clock",
            HasSubmenu = true,
            SubmenuId = "submenu-sesiones",
            SubmenuItems = submenuItems
        };
    }
}
