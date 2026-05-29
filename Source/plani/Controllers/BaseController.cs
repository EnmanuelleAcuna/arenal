using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plani.Identity;
using plani.Models.Data;
using plani.Models.Domain;

namespace plani.Controllers;

public class BaseController : Controller {
    protected readonly ApplicationDbContext _baseDbContext;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IWebHostEnvironment _environment;
    private readonly ApplicationRoleManager _roleManager;
    private readonly ApplicationUserManager _userManager;

    public BaseController(ApplicationUserManager userManager,
        ApplicationRoleManager roleManager,
        IConfiguration configuration,
        IHttpContextAccessor contextAccessor,
        IWebHostEnvironment environment,
        ApplicationDbContext dbContext) {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _contextAccessor = contextAccessor;
        _environment = environment;
        _baseDbContext = dbContext;
    }

    private string CurrentUser => User.Identity?.Name;

    protected string GetCurrentUser() {
        return CurrentUser;
    }

    protected IActionResult RedirectToLocal(string returnUrl) {
        if (Url.IsLocalUrl(url: returnUrl)) {
            return Redirect(url: returnUrl);
        }

        return RedirectToAction("Administracion", "Home");
    }

    protected void AddErrors(IdentityResult result) {
        foreach (IdentityError error in result.Errors) {
            ModelState.AddModelError("", errorMessage: error.Description);
        }
    }

    protected List<string> GetModelStateErrors() {
        return ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
    }

    protected IEnumerable<SelectListItem> CargarListaSeleccionRoles(bool cargarRolAdministrador) {
        IEnumerable<ApplicationRole> listaRoles;

        if (cargarRolAdministrador) {
            listaRoles = _roleManager.Roles.ToList();
        }
        else {
            listaRoles = _roleManager.Roles.Where(r => !r.Name.Equals("Administrador")).ToList();
        }

        IEnumerable<SelectListItem> listaSeleccionRoles =
            listaRoles.Select(p => new SelectListItem { Value = p.Id, Text = p.Name }).ToList();

        return listaSeleccionRoles;
    }

    protected static IList<string> ObtenerRolesSeleccionados(IFormCollection collection) {
        // En la colección vienen los rols seleccionados y la llave es el id del rol chequeado
        // Recorrer los roles chequeados y tomar el id que es el id del rol y crear un
        // objeto rol asignando la propiedad id obtenida del collection
        IList<string> rolesSeleccionados = new List<string>();
        foreach (string key in collection.Keys) {
            if (key[..1].Equals("R", comparisonType: StringComparison.OrdinalIgnoreCase)) {
                string rolSeleccionado = key[2..];
                rolesSeleccionados.Add(item: rolSeleccionado);
            }
        }

        return rolesSeleccionados;
    }

    protected IEnumerable<SelectListItem> CargarListaSeleccionUsuarios(IEnumerable<ApplicationUser> listaUsuarios) {
        IEnumerable<SelectListItem> listaSeleccionUsuarios = listaUsuarios.Select(p => new SelectListItem {
            Value = Convert.ToString(p.Id.ToString(), new CultureInfo("es-CR")),
            Text = string.Format("{0} {1} {2}", arg0: p.Name, arg1: p.FirstLastName, arg2: p.SecondLastName)
        }).ToList();
        return listaSeleccionUsuarios;
    }

    #region Dropdowns

    /// <summary>
    ///     Obtiene lista de colaboradores para dropdown
    /// </summary>
    protected async Task<IEnumerable<SelectListItem>> ObtenerColaboradoresDropdown() {
        List<ApplicationUser> colaboradores = await _baseDbContext.Usuarios
            .OrderBy(u => u.Name)
            .ToListAsync();

        return colaboradores.Select(c => new SelectListItem(text: c.FullName, value: c.Id));
    }

    /// <summary>
    ///     Obtiene lista de proyectos para dropdown (todos)
    /// </summary>
    protected async Task<IEnumerable<SelectListItem>> ObtenerProyectosDropdown() {
        List<Proyecto> proyectos = await _baseDbContext.Proyectos
            .Include(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .OrderBy(p => p.Contrato.Cliente.Nombre)
            .ToListAsync();

        return proyectos.Select(p =>
            new SelectListItem(
                $"{p.Contrato.Cliente.Nombre} - {p.Nombre}",
                p.Id.ToString()));
    }

    /// <summary>
    ///     Obtiene lista de proyectos asignados a un usuario para dropdown
    /// </summary>
    protected async Task<IEnumerable<SelectListItem>> ObtenerProyectosAsignadosDropdown(string idUsuario) {
        List<Asignacion> asignaciones = await _baseDbContext.Asignaciones
            .Where(a => a.IdColaborador == idUsuario)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .OrderBy(a => a.Proyecto.Contrato.Cliente.Nombre)
            .ToListAsync();

        return asignaciones.Select(a =>
            new SelectListItem(
                $"{a.Proyecto.Contrato.Cliente.Nombre} - {a.Proyecto.Nombre}",
                a.Proyecto.Id.ToString()));
    }

    /// <summary>
    ///     Obtiene lista de servicios para dropdown
    /// </summary>
    protected async Task<IEnumerable<SelectListItem>> ObtenerServiciosDropdown() {
        List<Servicio> servicios = await _baseDbContext.Servicios.ToListAsync();

        return servicios.Select(s => new SelectListItem(text: s.Nombre, s.Id.ToString()));
    }

    #endregion
}