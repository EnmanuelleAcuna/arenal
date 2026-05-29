using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using plani.Identity;
using plani.Models;
using plani.Models.Data;
using plani.Models.ViewModels;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace plani.Controllers;

[Authorize]
public class CuentasController : BaseController {
    private readonly ApplicationDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<CuentasController> _logger;
    private readonly ApplicationRoleManager _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationUserManager _userManager;

    public CuentasController(ApplicationUserManager userManager,
        ApplicationRoleManager roleManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IHttpContextAccessor contextAccesor,
        IEmailSender emailSender,
        ILogger<CuentasController> logger,
        IWebHostEnvironment environment,
        ApplicationDbContext dbContext)
        : base(userManager: userManager, roleManager: roleManager, configuration: configuration, contextAccessor: contextAccesor, environment: environment, dbContext: dbContext) {
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
    public ActionResult IniciarSesion(string returnUrl = null) {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarSesion(IniciarSesionViewModel modelo, string returnUrl) {
        returnUrl ??= Url.Content("Home/Administracion");

        if (!ModelState.IsValid) {
            ModelState.AddModelError("", "Datos incorrectos.");
            return View(model: modelo);
        }

        // This doesn't count login failures towards account lockout
        // To enable password failures to trigger account lockout, change to shouldLockout: true
        SignInResult result = await _signInManager.PasswordSignInAsync(userName: modelo.Correo, password: modelo.Contrasena,
            isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded) {
            ApplicationUser usuario = await _userManager.FindByEmailAsync(email: modelo.Correo);
            IdentityResult ultimaConexionActualizada = await _userManager.UpdateLastSession(user: usuario);
            if (ultimaConexionActualizada.Succeeded) {
                return RedirectToLocal(returnUrl: returnUrl);
            }
        }
        else {
            ModelState.AddModelError(key: string.Empty, "Correo electrónico y/o contraseña incorrectos.");
            return View(model: modelo);
        }

        if (result.RequiresTwoFactor) {
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = false });
        }

        if (result.IsLockedOut) {
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(key: string.Empty, "Ocurrió un error al iniciar sesión.");
        return View(model: modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarSesion() {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Administracion", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult SolicitarContrasena() {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> SolicitarContrasena(OlvidoContrasenaViewModel modelo) {
        if (!ModelState.IsValid) {
            ModelState.AddModelError("", "Error, el modelo no es válido.");
            return View(model: modelo);
        }

        ApplicationUser usuario = await _userManager.FindByEmailAsync(email: modelo.CorreoElectronico);

        if (usuario is null) {
            return
                View(nameof(SolicitarContrasenaConfirmada)); // No revelar que el usuario no existe, redirigir a la confirmación
        }

        if (usuario.Active.HasValue && usuario.Active.Value) {
            string token =
                await _userManager
                    .GeneratePasswordResetTokenAsync(user: usuario); // Generar un token de restablecimiento de contraseña

            string urlRestablecimientoContrasena = Url.Action(nameof(RestablecerContrasena), "Cuentas",
                new { userId = usuario.Id, code = token }, protocol: Request.Scheme); // Crear enlace

            // Configurar correo y enviarlo
            string mensajeCorreo = string.Format(new CultureInfo("es-CR"),
                "Para restablecer su contraseña haga click <a href=\"{0}\">aquí</a>", arg0: urlRestablecimientoContrasena);
            await _emailSender.SendEmailAsync(email: modelo.CorreoElectronico, "Restablecer contraseña", htmlMessage: mensajeCorreo);

            return View(nameof(SolicitarContrasenaConfirmada));
        }

        ModelState.AddModelError("", "El usuario está inactivo");
        return View(model: modelo);
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult SolicitarContrasenaConfirmada() {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult RestablecerContrasena(string code) {
        return code == null ? View("Error") : View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> RestablecerContrasena(RestablecerContrasenaViewModel modelo) {
        if (!ModelState.IsValid) {
            ModelState.AddModelError("", "Error, el modelo no es válido.");
            return View(model: modelo);
        }

        ApplicationUser usuario = await _userManager.FindByEmailAsync(email: modelo.CorreoElectronico);

        if (usuario is null) {
            return
                View(nameof(
                    RestablecerContrasenaConfirmada)); // No revelar que el usuario no existe, redirigir a la confirmación
        }

        IdentityResult result = await _userManager.ResetPasswordAsync(user: usuario, token: modelo.Code, newPassword: modelo.Contrasena);

        if (result.Succeeded) {
            return View(nameof(RestablecerContrasenaConfirmada));
        }

        AddErrors(result: result);
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult RestablecerContrasenaConfirmada() {
        return View();
    }

    #endregion

    #region Usuarios

    [HttpGet]
    public async Task<ActionResult> Usuarios() {
        List<ApplicationUser> users = _userManager.Users.OrderBy(u => u.Name).ToList();

        List<UsuariosIndexViewModel> usuarios = new();
        foreach (ApplicationUser user in users) {
            IList<string> userRoles = await _userManager.GetRolesAsync(user: user);
            usuarios.Add(new UsuariosIndexViewModel(usuario: user, string.Join(", ", values: userRoles)));
        }

        return View(model: usuarios);
    }

    [HttpGet]
    public ActionResult AgregarUsuario() {
        ViewBag.ListaRoles = User.IsInRole("Administrador")
            ? CargarListaSeleccionRoles(cargarRolAdministrador: true)
            : CargarListaSeleccionRoles(cargarRolAdministrador: false);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> AgregarUsuario(AgregarUsuarioViewModel modelo, IFormCollection collection) {
        if (ModelState.IsValid) {
            ApplicationUser usuario = modelo.Entidad();
            usuario.RegristrarCreacion(GetCurrentUser(), creadoEl: DateTime.UtcNow);
            IList<string> rolesSeleccionados = ObtenerRolesSeleccionados(collection: collection);
            IdentityResult usuarioCreado = await _userManager.CreateAsync(user: usuario, password: modelo.Contrasena);
            IdentityResult rolesAsignados = usuarioCreado.Succeeded
                ? await _userManager.AddToRolesAsync(user: usuario, roles: rolesSeleccionados)
                : IdentityResult.Failed();

            if (usuarioCreado.Succeeded && rolesAsignados.Succeeded) {
                return RedirectToAction(nameof(Usuarios));
            }

            AddErrors(result: usuarioCreado);
            AddErrors(result: rolesAsignados);
        }

        ModelState.AddModelError("", Utils.MensajeErrorCrear("usuario"));
        ViewBag.ListaRoles = User.IsInRole("Administrador")
            ? CargarListaSeleccionRoles(cargarRolAdministrador: true)
            : CargarListaSeleccionRoles(cargarRolAdministrador: false);
        return View(model: modelo);
    }

    [HttpGet]
    public async Task<ActionResult> EditarUsuario(string id) {
        ApplicationUser usuario = await _userManager.FindByIdAsync(userId: id);
        if (usuario == null) {
            return NotFound();
        }

        IList<ApplicationRole> rolesUsuario = await _userManager.ObtenerRolesUsuario(usuario: usuario);
        EditarUsuarioViewModel modelo = new(usuario: usuario, roles: rolesUsuario);
        ViewBag.ListaRoles = User.IsInRole("Administrador")
            ? CargarListaSeleccionRoles(cargarRolAdministrador: true)
            : CargarListaSeleccionRoles(cargarRolAdministrador: false);

        return View(model: modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditarUsuario(EditarUsuarioViewModel modelo, IFormCollection collection) {
        if (ModelState.IsValid) {
            ApplicationUser usuario = modelo.Entidad();
            usuario.RegistrarActualizacion(GetCurrentUser(), actualizadoEl: DateTime.UtcNow);
            IList<string> rolesSeleccionados = ObtenerRolesSeleccionados(collection: collection);
            IdentityResult usuarioActualizado = await _userManager.UpdatePersonalInformation(user: usuario);
            IdentityResult rolesActualizados = usuarioActualizado.Succeeded
                ? await _userManager.ActualizarRolesUsuario(user: usuario, roles: rolesSeleccionados)
                : IdentityResult.Failed();

            if (usuarioActualizado.Succeeded && rolesActualizados.Succeeded) {
                return RedirectToAction(nameof(Usuarios));
            }

            AddErrors(result: usuarioActualizado);
            AddErrors(result: rolesActualizados);
        }

        ModelState.AddModelError("", Utils.MensajeErrorActualizar(nameof(ApplicationUser)));
        ViewBag.ListaRoles = User.IsInRole("Administrador")
            ? CargarListaSeleccionRoles(cargarRolAdministrador: true)
            : CargarListaSeleccionRoles(cargarRolAdministrador: false);
        return View(model: modelo);
    }

    [HttpGet]
    public async Task<ActionResult> EliminarUsuario(string id) {
        ApplicationUser usuario = await _userManager.FindByIdAsync(userId: id);
        if (usuario == null) {
            return NotFound();
        }

        IList<ApplicationRole> rolesUsuario = await _userManager.ObtenerRolesUsuario(usuario: usuario);
        EditarUsuarioViewModel modelo = new(usuario: usuario, roles: rolesUsuario);
        return View(model: modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EliminarUsuario(EditarUsuarioViewModel modelo) {
        ApplicationUser usuario = await _userManager.FindByIdAsync(userId: modelo.IdUsuario);
        if (usuario == null) {
            return NotFound();
        }

        usuario.Eliminar(GetCurrentUser());
        IdentityResult usuarioEliminado = await _userManager.UpdateAsync(user: usuario);
        if (usuarioEliminado.Succeeded) {
            return RedirectToAction(nameof(Usuarios));
        }

        AddErrors(result: usuarioEliminado);
        ModelState.AddModelError("", Utils.MensajeErrorEliminar(nameof(usuario)));

        return View(model: modelo);
    }

    #endregion

    #region Roles

    [HttpGet]
    public IActionResult Roles() {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> DetalleRol(string id) {
        ApplicationRole rol = await _roleManager.FindByIdAsync(roleId: id);
        if (rol == null) {
            return NotFound();
        }

        IList<ApplicationUser> usersInRole = await _userManager.GetUsersInRoleAsync(roleName: rol.Name);
        DetalleRolViewModel viewModel = new(rol: rol, usuarios: usersInRole);
        return View(model: viewModel);
    }

    // JSON endpoints for inline editing

    [HttpGet]
    public async Task<JsonResult> ObtenerRoles() {
        List<ApplicationRole> roles = await _roleManager.Roles
            .OrderBy(r => r.Name)
            .ToListAsync();

        IEnumerable<RolListViewModel> viewModels = roles.Select(r => new RolListViewModel {
            Id = r.Id,
            Nombre = r.Name,
            Descripcion = r.Description
        });

        return Json(new { success = true, data = viewModels });
    }

    [HttpPost]
    public async Task<JsonResult> AgregarRolJson([FromBody] AgregarRolViewModel model) {
        if (!ModelState.IsValid) {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        ApplicationRole rol = model.ToApplicationRole();
        rol.RegristrarCreacion(GetCurrentUser(), creadoEl: DateTime.UtcNow);

        IdentityResult result = await _roleManager.CreateAsync(role: rol);

        if (result.Succeeded) {
            RolListViewModel data = new() {
                Id = rol.Id,
                Nombre = rol.Name,
                Descripcion = rol.Description
            };
            return Json(new { success = true, message = "Rol agregado exitosamente", data });
        }

        List<string> resultErrors = result.Errors.Select(e => e.Description).ToList();
        return Json(new { success = false, errors = resultErrors });
    }

    [HttpPost]
    public async Task<JsonResult> EditarRolJson([FromBody] EditarRolViewModel model) {
        if (!ModelState.IsValid) {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        ApplicationRole rol = await _roleManager.FindByIdAsync(roleId: model.IdRol);

        if (rol == null) {
            return Json(new { success = false, errors = new[] { "Rol no encontrado" } });
        }

        rol.Name = model.Nombre;
        rol.Description = model.Descripcion;
        rol.RegistrarActualizacion(GetCurrentUser(), actualizadoEl: DateTime.UtcNow);

        IdentityResult result = await _roleManager.UpdateAsync(role: rol);

        if (result.Succeeded) {
            RolListViewModel data = new() {
                Id = rol.Id,
                Nombre = rol.Name,
                Descripcion = rol.Description
            };
            return Json(new { success = true, message = "Rol actualizado exitosamente", data });
        }

        List<string> resultErrors = result.Errors.Select(e => e.Description).ToList();
        return Json(new { success = false, errors = resultErrors });
    }

    [HttpPost]
    public async Task<JsonResult> EliminarRolJson([FromBody] EliminarRolRequest request) {
        ApplicationRole rol = await _roleManager.FindByIdAsync(roleId: request.Id);

        if (rol == null) {
            return Json(new { success = false, errors = new[] { "Rol no encontrado" } });
        }

        // Verificar si el rol tiene usuarios asignados
        IList<ApplicationUser> usuarios = await _userManager.GetUsersInRoleAsync(roleName: rol.Name);
        if (usuarios.Any()) {
            return Json(new { success = false, errors = new[] { "No se puede eliminar el rol porque tiene usuarios asignados" } });
        }

        rol.Eliminar(GetCurrentUser());
        IdentityResult result = await _roleManager.UpdateAsync(role: rol);

        if (result.Succeeded) {
            return Json(new { success = true, message = "Rol eliminado exitosamente" });
        }

        List<string> resultErrors = result.Errors.Select(e => e.Description).ToList();
        return Json(new { success = false, errors = resultErrors });
    }

    #endregion

    #region Colaboradores

    [HttpGet]
    public async Task<IActionResult> Colaboradores() {
        IList<ApplicationUser> usuariosColaboradores = await _userManager.GetUsersInRoleAsync("Colaborador");
        IList<ApplicationUser> usuariosCoordinadores = await _userManager.GetUsersInRoleAsync("Coordinador");

        List<ApplicationUser> usuarios = usuariosColaboradores.Union(second: usuariosCoordinadores).ToList();
        List<UsuariosIndexViewModel> modelo = usuarios.OrderBy(u => u.Name).Select(u => new UsuariosIndexViewModel(usuario: u, roles: string.Empty)).ToList();

        return View(model: modelo);
    }

    [HttpGet]
    public async Task<IActionResult> DetalleColaborador(Guid id) {
        ApplicationUser model = await _dbContext.Usuarios
            .Include(u => u.Asignaciones)
            .ThenInclude(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .FirstOrDefaultAsync(a => a.Id == id.ToString());

        if (model == null) {
            return NotFound();
        }

        return View(model: model);
    }

    #endregion
}