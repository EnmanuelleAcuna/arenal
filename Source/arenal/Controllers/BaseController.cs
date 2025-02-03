using System.Collections;
using System.Globalization;
using arenal.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace arenal.Controllers;

public class BaseController : Controller
{
    private readonly ApplicationUserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IWebHostEnvironment _environment;

    public BaseController(ApplicationUserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IConfiguration configuration,
        IHttpContextAccessor contextAccessor,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _contextAccessor = contextAccessor;
        _environment = environment;
    }

    protected async Task CreateDefaultUser()
    {
        if (await _userManager.Users.AnyAsync()) return;

        var rolAdministrador =
            new ApplicationRole(Guid.NewGuid().ToString(), "Administrador", "Administrador del sistema.");
        var rolColaborador = new ApplicationRole(Guid.NewGuid().ToString(), "Colaborador", "Colaborador.");
        var rolCoordinador = new ApplicationRole(Guid.NewGuid().ToString(), "Coordinador", "Coordinador.");

        await _roleManager.CreateAsync(rolAdministrador);
        await _roleManager.CreateAsync(rolColaborador);
        await _roleManager.CreateAsync(rolCoordinador);

        var usuario = new ApplicationUser(Guid.NewGuid().ToString(), "emanuelacu@gmail.com", "emanuelacu@gmail.com",
            "Enmanuelle", "Acuña", "Arguedas", "206830685", true);

        var rolesSeleccionados = new List<string> { "Administrador", "Colaborador" };

        await _userManager.CreateAsync(usuario, "ContraseñaGenerica");
        await _userManager.AddToRolesAsync(usuario, rolesSeleccionados);

        await Task.CompletedTask;
    }

    private string CurrentUser => User.Identity?.Name;

    protected string GetCurrentUser() => CurrentUser;

    protected IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Administracion", "Home");
    }

    protected void AddErrors(IdentityResult result)
    {
        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
    }

    protected List<string> GetModelStateErrors() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

    protected IEnumerable<SelectListItem> CargarListaSeleccionRoles(bool cargarRolAdministrador = false)
    {
        IEnumerable<ApplicationRole> listaRoles;
        
        if (cargarRolAdministrador)
            listaRoles = _roleManager.Roles.Where(r => !r.Name.Equals("Administrador")).ToList();
        else
            listaRoles = _roleManager.Roles.ToList();

        IEnumerable<SelectListItem> listaSeleccionRoles =
            listaRoles.Select(p => new SelectListItem { Value = p.Id, Text = p.Name }).ToList();
        
        return listaSeleccionRoles;
    }

    protected static IList<string> ObtenerRolesSeleccionados(IFormCollection collection)
    {
        // En la colección vienen los rols seleccionados y la llave es el id del rol chequeado
        // Recorrer los roles chequeados y tomar el id que es el id del rol y crear un
        // objeto rol asignando la propiedad id obtenida del collection
        IList<string> rolesSeleccionados = new List<string>();
        foreach (string key in collection.Keys)
        {
            if (key[..1].Equals("R", StringComparison.OrdinalIgnoreCase))
            {
                string rolSeleccionado = key[2..];
                rolesSeleccionados.Add(rolSeleccionado);
            }
        }

        return rolesSeleccionados;
    }

    protected IEnumerable<SelectListItem> CargarListaSeleccionUsuarios(IEnumerable<ApplicationUser> listaUsuarios)
    {
        IEnumerable<SelectListItem> listaSeleccionUsuarios = listaUsuarios.Select(p => new SelectListItem
        {
            Value = Convert.ToString(p.Id.ToString(), new CultureInfo("es-CR")),
            Text = string.Format("{0} {1} {2}", p.Name, p.FirstLastName, p.SecondLastName)
        }).ToList();
        return listaSeleccionUsuarios;
    }
}
