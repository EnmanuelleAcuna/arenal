using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using plani.Identity;
using plani.Models;
using plani.Models.Data;
using plani.Models.ViewModels;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace plani.Controllers;

[Authorize]
public class CuentasController : BaseController
{
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationRoleManager _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<CuentasController> _logger;

    private readonly ApplicationDbContext _dbContext;

    public CuentasController(ApplicationUserManager userManager,
        ApplicationRoleManager roleManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IHttpContextAccessor contextAccesor,
        IEmailSender emailSender,
        ILogger<CuentasController> logger,
        IWebHostEnvironment environment,
        ApplicationDbContext dbContext)
        : base(userManager, roleManager, configuration, contextAccesor, environment)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _logger = logger;

        _dbContext = dbContext;
    }

    #region Autenticación

    [HttpGet]
    [AllowAnonymous]
    public ActionResult IniciarSesion(string returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        // await CreateDefaultUser();
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarSesion(IniciarSesionViewModel modelo, string returnUrl)
    {
        returnUrl ??= Url.Content("Home/Administracion");

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Datos incorrectos.");
            return View(modelo);
        }

        // This doesn't count login failures towards account lockout
        // To enable password failures to trigger account lockout, change to shouldLockout: true
        SignInResult result = await _signInManager.PasswordSignInAsync(modelo.Correo, modelo.Contrasena,
            isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            ApplicationUser usuario = await _userManager.FindByEmailAsync(modelo.Correo);
            IdentityResult ultimaConexionActualizada = await _userManager.UpdateLastSession(usuario);
            if (ultimaConexionActualizada.Succeeded) return RedirectToLocal(returnUrl);
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Correo electrónico y/o contraseña incorrectos.");
            return View(modelo);
        }

        if (result.RequiresTwoFactor)
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = false });
        if (result.IsLockedOut) return RedirectToPage("./Lockout");

        ModelState.AddModelError(string.Empty, "Ocurrió un error al iniciar sesión.");
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarSesion()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Administracion", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult SolicitarContrasena() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> SolicitarContrasena(OlvidoContrasenaViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Error, el modelo no es válido.");
            return View(modelo);
        }

        ApplicationUser usuario = await _userManager.FindByEmailAsync(modelo.CorreoElectronico);

        if (usuario is null)
            return
                View(nameof(SolicitarContrasenaConfirmada)); // No revelar que el usuario no existe, redirigir a la confirmación

        if (usuario.Active.HasValue && usuario.Active.Value)
        {
            string token =
                await _userManager
                    .GeneratePasswordResetTokenAsync(usuario); // Generar un token de restablecimiento de contraseña

            string urlRestablecimientoContrasena = Url.Action(nameof(RestablecerContrasena), "Cuentas",
                new { userId = usuario.Id, code = token }, protocol: Request.Scheme); // Crear enlace

            // Configurar correo y enviarlo
            string mensajeCorreo = string.Format(new CultureInfo("es-CR"),
                "Para restablecer su contraseña haga click <a href=\"{0}\">aquí</a>", urlRestablecimientoContrasena);
            await _emailSender.SendEmailAsync(modelo.CorreoElectronico, "Restablecer contraseña", mensajeCorreo);

            return View(nameof(SolicitarContrasenaConfirmada));
        }
        else
        {
            ModelState.AddModelError("", "El usuario está inactivo");
            return View(modelo);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult SolicitarContrasenaConfirmada() => View();

    [HttpGet]
    [AllowAnonymous]
    public ActionResult RestablecerContrasena(string code) => code == null ? View("Error") : View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> RestablecerContrasena(RestablecerContrasenaViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Error, el modelo no es válido.");
            return View(modelo);
        }

        ApplicationUser usuario = await _userManager.FindByEmailAsync(modelo.CorreoElectronico);

        if (usuario is null)
            return
                View(nameof(
                    RestablecerContrasenaConfirmada)); // No revelar que el usuario no existe, redirigir a la confirmación

        IdentityResult result = await _userManager.ResetPasswordAsync(usuario, modelo.Code, modelo.Contrasena);

        if (result.Succeeded) return View(nameof(RestablecerContrasenaConfirmada));

        AddErrors(result);
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult RestablecerContrasenaConfirmada() => View();

    #endregion

    #region Usuarios

    [HttpGet]
    public async Task<ActionResult> Usuarios()
    {
        List<ApplicationUser> users = _userManager.Users.OrderBy(u => u.Name).ToList();

        List<UsuariosIndexViewModel> usuarios = new();
        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            usuarios.Add(new UsuariosIndexViewModel(user, string.Join(", ", userRoles)));
        }

        return View(usuarios);
    }

    [HttpGet]
    public ActionResult AgregarUsuario()
    {
        ViewBag.ListaRoles = User.IsInRole("Administrador")
            ? CargarListaSeleccionRoles(cargarRolAdministrador: true)
            : CargarListaSeleccionRoles(cargarRolAdministrador: false);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> AgregarUsuario(AgregarUsuarioViewModel modelo, IFormCollection collection)
    {
        if (ModelState.IsValid)
        {
            ApplicationUser usuario = modelo.Entidad();
            usuario.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
            IList<string> rolesSeleccionados = ObtenerRolesSeleccionados(collection);
            IdentityResult usuarioCreado = await _userManager.CreateAsync(usuario, modelo.Contrasena);
            IdentityResult rolesAsignados = usuarioCreado.Succeeded
                ? await _userManager.AddToRolesAsync(usuario, rolesSeleccionados)
                : IdentityResult.Failed();

            if (usuarioCreado.Succeeded && rolesAsignados.Succeeded) return RedirectToAction(nameof(Usuarios));

            AddErrors(usuarioCreado);
            AddErrors(rolesAsignados);
        }

        ModelState.AddModelError("", Utils.MensajeErrorCrear("usuario"));
        ViewBag.ListaRoles = User.IsInRole("Administrador")
            ? CargarListaSeleccionRoles(cargarRolAdministrador: true)
            : CargarListaSeleccionRoles(cargarRolAdministrador: false);
        return View(modelo);
    }

    [HttpGet]
    public async Task<ActionResult> EditarUsuario(string id)
    {
        ApplicationUser usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();
        IList<ApplicationRole> rolesUsuario = await _userManager.ObtenerRolesUsuario(usuario);
        EditarUsuarioViewModel modelo = new(usuario, rolesUsuario);
        ViewBag.ListaRoles = User.IsInRole("Administrador")
            ? CargarListaSeleccionRoles(cargarRolAdministrador: true)
            : CargarListaSeleccionRoles(cargarRolAdministrador: false);

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditarUsuario(EditarUsuarioViewModel modelo, IFormCollection collection)
    {
        if (ModelState.IsValid)
        {
            ApplicationUser usuario = modelo.Entidad();
            usuario.RegistrarActualizacion(GetCurrentUser(), DateTime.UtcNow);
            IList<string> rolesSeleccionados = ObtenerRolesSeleccionados(collection);
            IdentityResult usuarioActualizado = await _userManager.UpdatePersonalInformation(usuario);
            IdentityResult rolesActualizados = usuarioActualizado.Succeeded
                ? await _userManager.ActualizarRolesUsuario(usuario, rolesSeleccionados)
                : IdentityResult.Failed();

            if (usuarioActualizado.Succeeded && rolesActualizados.Succeeded) return RedirectToAction(nameof(Usuarios));

            AddErrors(usuarioActualizado);
            AddErrors(rolesActualizados);
        }

        ModelState.AddModelError("", Utils.MensajeErrorActualizar(nameof(ApplicationUser)));
        ViewBag.ListaRoles = User.IsInRole("Administrador")
            ? CargarListaSeleccionRoles(cargarRolAdministrador: true)
            : CargarListaSeleccionRoles(cargarRolAdministrador: false);
        return View(modelo);
    }

    [HttpGet]
    public async Task<ActionResult> EliminarUsuario(string id)
    {
        ApplicationUser usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();
        IList<ApplicationRole> rolesUsuario = await _userManager.ObtenerRolesUsuario(usuario);
        EditarUsuarioViewModel modelo = new(usuario, rolesUsuario);
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EliminarUsuario(EditarUsuarioViewModel modelo)
    {
        ApplicationUser usuario = await _userManager.FindByIdAsync(modelo.IdUsuario);
        if (usuario == null) return NotFound();
        usuario.Eliminar(GetCurrentUser());
        IdentityResult usuarioEliminado = await _userManager.UpdateAsync(usuario);
        if (usuarioEliminado.Succeeded) return RedirectToAction(nameof(Usuarios));

        AddErrors(usuarioEliminado);
        ModelState.AddModelError("", Utils.MensajeErrorEliminar(nameof(usuario)));

        return View(modelo);
    }

    #endregion

    #region Roles

    [HttpGet]
    public IActionResult Roles()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> DetalleRol(string id)
    {
        ApplicationRole rol = await _roleManager.FindByIdAsync(id);
        if (rol == null) return NotFound();
        IList<ApplicationUser> usersInRole = await _userManager.GetUsersInRoleAsync(rol.Name);
        DetalleRolViewModel viewModel = new(rol, usersInRole);
        return View(viewModel);
    }

    // JSON endpoints for inline editing

    [HttpGet]
    public async Task<JsonResult> ObtenerRoles()
    {
        var roles = await _roleManager.Roles
            .OrderBy(r => r.Name)
            .ToListAsync();

        var viewModels = roles.Select(r => new RolListViewModel
        {
            Id = r.Id,
            Nombre = r.Name,
            Descripcion = r.Description
        });

        return Json(new { success = true, data = viewModels });
    }

    [HttpPost]
    public async Task<JsonResult> AgregarRolJson([FromBody] AgregarRolViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        var rol = model.ToApplicationRole();
        rol.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);

        var result = await _roleManager.CreateAsync(rol);

        if (result.Succeeded)
        {
            var data = new RolListViewModel
            {
                Id = rol.Id,
                Nombre = rol.Name,
                Descripcion = rol.Description
            };
            return Json(new { success = true, message = "Rol agregado exitosamente", data });
        }

        var resultErrors = result.Errors.Select(e => e.Description).ToList();
        return Json(new { success = false, errors = resultErrors });
    }

    [HttpPost]
    public async Task<JsonResult> EditarRolJson([FromBody] EditarRolViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        var rol = await _roleManager.FindByIdAsync(model.IdRol);

        if (rol == null)
        {
            return Json(new { success = false, errors = new[] { "Rol no encontrado" } });
        }

        rol.Name = model.Nombre;
        rol.Description = model.Descripcion;
        rol.RegistrarActualizacion(GetCurrentUser(), DateTime.UtcNow);

        var result = await _roleManager.UpdateAsync(rol);

        if (result.Succeeded)
        {
            var data = new RolListViewModel
            {
                Id = rol.Id,
                Nombre = rol.Name,
                Descripcion = rol.Description
            };
            return Json(new { success = true, message = "Rol actualizado exitosamente", data });
        }

        var resultErrors = result.Errors.Select(e => e.Description).ToList();
        return Json(new { success = false, errors = resultErrors });
    }

    [HttpPost]
    public async Task<JsonResult> EliminarRolJson([FromBody] EliminarRolRequest request)
    {
        var rol = await _roleManager.FindByIdAsync(request.Id);

        if (rol == null)
        {
            return Json(new { success = false, errors = new[] { "Rol no encontrado" } });
        }

        // Verificar si el rol tiene usuarios asignados
        var usuarios = await _userManager.GetUsersInRoleAsync(rol.Name);
        if (usuarios.Any())
        {
            return Json(new { success = false, errors = new[] { "No se puede eliminar el rol porque tiene usuarios asignados" } });
        }

        rol.Eliminar(GetCurrentUser());
        var result = await _roleManager.UpdateAsync(rol);

        if (result.Succeeded)
        {
            return Json(new { success = true, message = "Rol eliminado exitosamente" });
        }

        var resultErrors = result.Errors.Select(e => e.Description).ToList();
        return Json(new { success = false, errors = resultErrors });
    }


    #endregion

    #region Colaboradores

    [HttpGet]
    public async Task<IActionResult> Colaboradores()
    {
        var usuariosColaboradores = await _userManager.GetUsersInRoleAsync("Colaborador");
        var usuariosCoordinadores = await _userManager.GetUsersInRoleAsync("Coordinador");

        var usuarios = usuariosColaboradores.Union(usuariosCoordinadores).ToList();
        var modelo = usuarios.OrderBy(u => u.Name).Select(u => new UsuariosIndexViewModel(u, string.Empty)).ToList();

        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> DetalleColaborador(Guid id)
    {
        ApplicationUser model = await _dbContext.Usuarios
            .Include(u => u.Asignaciones)
            .ThenInclude(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .FirstOrDefaultAsync(a => a.Id == id.ToString());

        if (model == null) return NotFound();

        return View(model);
    }

    #endregion
}