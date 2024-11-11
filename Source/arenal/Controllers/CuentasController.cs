using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using arenal.Models;
using arenal.Models.Extras;
using arenal.Models.Identity;
using arenal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace arenal.Controllers;

[Authorize]
public class CuentasController : BaseController
{
	private readonly ApplicationUserManager<ApplicationUser> _userManager;
	private readonly RoleManager<ApplicationRole> _roleManager;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly IConfiguration _configuration;
	private readonly IEmailSender _emailSender;
	private readonly ILogger<CuentasController> _logger;

	public CuentasController(ApplicationUserManager<ApplicationUser> userManager,
							 RoleManager<ApplicationRole> roleManager,
							 SignInManager<ApplicationUser> signInManager,
							 IConfiguration configuration,
							 IHttpContextAccessor contextAccesor,
							 IEmailSender emailSender,
							 ILogger<CuentasController> logger,
							 IWebHostEnvironment environment)
	: base(userManager, roleManager, configuration, contextAccesor, environment)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_signInManager = signInManager;
		_configuration = configuration;
		_emailSender = emailSender;
		_logger = logger;
	}

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
		returnUrl ??= Url.Content("~/Home/Administracion");

		if (!ModelState.IsValid)
		{
			ModelState.AddModelError("", "Datos incorrectos.");
			return View(modelo);
		}

		// This doesn't count login failures towards account lockout
		// To enable password failures to trigger account lockout, change to shouldLockout: true
		SignInResult result = await _signInManager.PasswordSignInAsync(modelo.Correo, modelo.Contrasena, isPersistent: false, lockoutOnFailure: false);

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

		if (result.RequiresTwoFactor) return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = false });
		if (result.IsLockedOut) return RedirectToPage("./Lockout");

		// Si llega a este punto, quiere decir que hubo un error
		ModelState.AddModelError(string.Empty, "Ocurrió un error al iniciar sesión.");
		return View(modelo);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> CerrarSesion()
	{
		await _signInManager.SignOutAsync();
		return RedirectToAction("Index", "Home");
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

		if (usuario is null) return View(nameof(SolicitarContrasenaConfirmada)); // No revelar que el usuario no existe, redirigir a la confirmación

		if ((bool)usuario.Active)
		{
			string token = await _userManager.GeneratePasswordResetTokenAsync(usuario); // Generar un token de restablecimiento de contraseña

			string urlRestablecimientoContrasena = Url.Action(nameof(RestablecerContrasena), "Cuentas", new { userId = usuario.Id, code = token }, protocol: Request.Scheme); // Crear enlace

			// Configurar correo y enviarlo
			string mensajeCorreo = string.Format(new CultureInfo("es-CR"), "Para restablecer su contraseña haga click <a href=\"{0}\">aquí</a>", urlRestablecimientoContrasena);
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

		if (usuario is null) return View(nameof(RestablecerContrasenaConfirmada)); // No revelar que el usuario no existe, redirigir a la confirmación

		IdentityResult result = await _userManager.ResetPasswordAsync(usuario, modelo.Code, modelo.Contrasena);

		if (result.Succeeded) return View(nameof(RestablecerContrasenaConfirmada));

		AddErrors(result);
		return View();
	}

	[HttpGet]
	[AllowAnonymous]
	public ActionResult RestablecerContrasenaConfirmada() => View();

	[HttpGet]
	public ActionResult ListarUsuarios()
	{
		var listaUsuarios = _userManager.Users.ToList();
		var modelo = listaUsuarios.Select(u => new UsuarioViewModel(u)).ToList();
		return View(modelo);
	}

	[HttpGet]
	public ActionResult AgregarUsuario()
	{
		ViewBag.ListaRoles = CargarListaSeleccionRoles();
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<ActionResult> AgregarUsuario(AgregarUsuarioViewModel modelo, IFormCollection collection)
	{
		if (ModelState.IsValid)
		{
			ApplicationUser usuario = modelo.Entidad();

			IList<string> rolesSeleccionados = ObtenerRolesSeleccionados(collection);

			IdentityResult usuarioCreado = await _userManager.CreateAsync(usuario, modelo.Contrasena);
			IdentityResult rolesAsignados = usuarioCreado.Succeeded ? await _userManager.AddToRolesAsync(usuario, rolesSeleccionados) : IdentityResult.Failed();

			if (usuarioCreado.Succeeded && rolesAsignados.Succeeded) return RedirectToAction(nameof(ListarUsuarios));

			AddErrors(usuarioCreado);
			AddErrors(rolesAsignados);
		}

		ModelState.AddModelError("", Messages.MensajeErrorCrear(nameof(ApplicationUser)));
		ViewBag.ListaRoles = CargarListaSeleccionRoles();
		return View(modelo);
	}

	[HttpGet]
	public async Task<ActionResult> EditarUsuario(string id)
	{
		ApplicationUser usuario = await _userManager.FindByIdAsync(id);

		if (usuario == null) return NotFound();

		IList<ApplicationRole> rolesUsuario = await ObtenerRolesUsuario(usuario);

		EditarUsuarioViewModel modelo = new(usuario, rolesUsuario);

		ViewBag.ListaRoles = CargarListaSeleccionRoles();

		return View(modelo);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<ActionResult> EditarUsuario(EditarUsuarioViewModel modelo, IFormCollection collection)
	{
		if (ModelState.IsValid)
		{
			ApplicationUser usuario = modelo.Entidad();

			IList<string> rolesSeleccionados = ObtenerRolesSeleccionados(collection);

			IdentityResult usuarioActualizado = await _userManager.UpdatePersonalInformation(usuario);
			IdentityResult rolesActualizados = usuarioActualizado.Succeeded ? await _userManager.ActualizarRolesUsuario(usuario, rolesSeleccionados) : IdentityResult.Failed();

			if (usuarioActualizado.Succeeded && rolesActualizados.Succeeded)
			{
				return RedirectToAction(nameof(ListarUsuarios));
			}
			else
			{
				AddErrors(usuarioActualizado);
				AddErrors(rolesActualizados);
			}
		}

		ModelState.AddModelError("", Messages.MensajeErrorActualizar(nameof(ApplicationUser)));
		ViewBag.ListaRoles = CargarListaSeleccionRoles();

		return View(modelo);
	}

	[HttpGet]
	public async Task<ActionResult> EliminarUsuario(string id)
	{
		ApplicationUser usuario = await _userManager.FindByIdAsync(id);

		if (usuario == null) return NotFound();

		IList<ApplicationRole> rolesUsuario = await ObtenerRolesUsuario(usuario);

		EditarUsuarioViewModel modelo = new(usuario, rolesUsuario);

		return View(modelo);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<ActionResult> EliminarUsuario(EditarUsuarioViewModel modelo)
	{
		ApplicationUser usuario = await _userManager.FindByIdAsync(modelo.IdUsuario);

		if (usuario == null) return NotFound();

		IdentityResult usuarioEliminado = await _userManager.DeleteAsync(usuario);

		if (usuarioEliminado.Succeeded) return RedirectToAction(nameof(ListarUsuarios));

		// Si se llega a este punto, hubo un error
		AddErrors(usuarioEliminado);

		ModelState.AddModelError("", Messages.MensajeErrorEliminar(nameof(usuario)));

		return View(modelo);
	}

	[HttpGet]
	public IActionResult ListarRoles()
	{
		IList<ApplicationRole> listaRoles = _roleManager.Roles.ToList();
		IList<InicioRolesViewModel> modelo = listaRoles.Select(x => new InicioRolesViewModel(x)).ToList();
		return View(modelo);
	}

	[HttpGet]
	public IActionResult AgregarRol() => View();

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> AgregarRol(NuevoRolViewModel modelo)
	{
		if (ModelState.IsValid)
		{
			ApplicationRole rol = modelo.Entidad();
			IdentityResult rolCreado = await _roleManager.CreateAsync(rol);

			if (rolCreado.Succeeded) return RedirectToAction(nameof(ListarRoles));

			AddErrors(rolCreado);
		}

		// Si se llega a este punto, hubo un error
		ModelState.AddModelError("", Messages.MensajeErrorCrear(nameof(ApplicationRole)));

		return View(modelo);
	}

	[HttpGet]
	public async Task<IActionResult> EditarRol(string id)
	{
		ApplicationRole rol = await _roleManager.FindByIdAsync(id);

		if (rol == null) return NotFound();

		EditarRolViewModel modelo = new(rol);

		return View(modelo);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<ActionResult> EditarRol(EditarRolViewModel modelo)
	{
		if (ModelState.IsValid)
		{
			ApplicationRole rol = await _roleManager.FindByIdAsync(modelo.IdRol);

			rol.ActualizarDatos(modelo.Nombre, modelo.Descripcion);

			IdentityResult RolActualizado = await _roleManager.UpdateAsync(rol);

			if (RolActualizado.Succeeded) return RedirectToAction(nameof(ListarRoles));

			AddErrors(RolActualizado);
		}

		// Si se llega a este punto, hubo un error
		ModelState.AddModelError("", Messages.MensajeErrorActualizar(nameof(ApplicationRole)));

		return View(modelo);
	}

	[HttpGet]
	public async Task<ActionResult> EliminarRol(string id)
	{
		ApplicationRole rol = await _roleManager.FindByIdAsync(id);

		if (rol == null) return NotFound();

		EditarRolViewModel modelo = new(rol);

		return View(modelo);
	}

	[HttpPost]
	public async Task<ActionResult> EliminarRol(EditarRolViewModel modelo)
	{
		ApplicationRole rol = await _roleManager.FindByIdAsync(modelo.IdRol);

		if (rol == null) return NotFound();

		IdentityResult rolEliminado = await _roleManager.DeleteAsync(rol);

		if (rolEliminado.Succeeded) return RedirectToAction(nameof(ListarRoles));

		// Si se llega a este punto, hubo un error
		AddErrors(rolEliminado);

		ModelState.AddModelError("", Messages.MensajeErrorEliminar(nameof(ApplicationRole)));

		return View(modelo);
	}

	public async Task<ActionResult> ListarInstructores()
	{
		var usuariosInstructor = await _userManager.GetUsersInRoleAsync("Instructor");
		var modelo = usuariosInstructor.Select(x => new UsuarioViewModel(x));
		return View(modelo);
	}

	public async Task<ActionResult> RegistrarNuevoInstructor()
	{
		var usuariosNoInstructor = await _userManager.GetUsersNotInRoleAsync("Instructor");
		var modelo = usuariosNoInstructor.Select(x => new UsuarioViewModel(x));
		return View(modelo);
	}

	[HttpGet]
	public async Task<IActionResult> RegistrarInstructor(string id)
	{
		var usuario = await _userManager.FindByIdAsync(id);
		var modelo = new AgregarInstructorViewModel(usuario);

		ViewBag.Provincias = await CargarListaSeleccionProvincias();
		ViewBag.Cantones = await CargarListaSeleccionCantones();
		ViewBag.Distritos = await CargarListaSeleccionDistritos();

		return View(modelo);
	}

	[HttpPost]
	public async Task<IActionResult> RegistrarInstructor(AgregarInstructorViewModel modelo)
	{
		if (ModelState.IsValid)
		{
			var rutaFotografia = GuardarImagenDisco(modelo.ProfilePicture);

			var usuarioRegistradoComoInstructor = await _userManager.RegistrarUsuarioComoInstructor(modelo.Entidad(), rutaFotografia);

			if (usuarioRegistradoComoInstructor.Succeeded) return RedirectToAction(nameof(ListarInstructores));

			AddErrors(usuarioRegistradoComoInstructor);
		}

		ModelState.AddModelError("", Messages.MensajeErrorCrear(nameof(ApplicationUser)));

		ViewBag.Provincias = await CargarListaSeleccionProvincias();
		ViewBag.Cantones = await CargarListaSeleccionCantones();
		ViewBag.Distritos = await CargarListaSeleccionDistritos();

		return View(modelo);
	}

	public async Task<ActionResult> ListarClientes()
	{
		var usuariosCliente = await _userManager.GetUsersInRoleAsync("Cliente");
		var modelo = usuariosCliente.Select(x => new UsuarioViewModel(x));
		return View(modelo);
	}

	public async Task<ActionResult> RegistrarNuevoCliente()
	{
		var usuariosNoCliente = await _userManager.GetUsersNotInRoleAsync("Cliente");
		var modelo = usuariosNoCliente.Select(x => new UsuarioViewModel(x));
		return View(modelo);
	}

	[HttpGet]
	public async Task<IActionResult> RegistrarCliente(string id)
	{
		var usuario = await _userManager.FindByIdAsync(id);
		var modelo = new AgregarClienteViewModel(usuario);

		ViewBag.Provincias = await CargarListaSeleccionProvincias();
		ViewBag.Cantones = await CargarListaSeleccionCantones();
		ViewBag.Distritos = await CargarListaSeleccionDistritos();

		return View(modelo);
	}

	[HttpPost]
	public async Task<IActionResult> RegistrarCliente(AgregarClienteViewModel modelo)
	{
		if (ModelState.IsValid)
		{
			var rutaFotografia = GuardarImagenDisco(modelo.ProfilePicture);

			var usuarioRegistradoComoCliente = await _userManager.RegistrarUsuarioComoCliente(modelo.Entidad(), rutaFotografia);

			if (usuarioRegistradoComoCliente.Succeeded) return RedirectToAction(nameof(ListarClientes));

			AddErrors(usuarioRegistradoComoCliente);
		}

		ModelState.AddModelError("", Messages.MensajeErrorCrear(nameof(ApplicationUser)));

		ViewBag.Provincias = await CargarListaSeleccionProvincias();
		ViewBag.Cantones = await CargarListaSeleccionCantones();
		ViewBag.Distritos = await CargarListaSeleccionDistritos();

		return View(modelo);
	}

	public async Task<ActionResult> ReporteInstructores()
	{
		var usuariosInstructor = await _userManager.GetUsersInRoleWithDivisionTerritorialInfoAsync("Instructor");
		var modelo = usuariosInstructor.Select(x => new ReporteInstructorViewModel(x));
		return View(modelo);
	}

	public async Task<ActionResult> ReporteClientes()
	{
		var usuariosCliente = await _userManager.GetUsersInRoleWithDivisionTerritorialInfoAsync("Cliente");
		var modelo = usuariosCliente.Select(x => new ReporteClienteViewModel(x));
		return View(modelo);
	}
}
