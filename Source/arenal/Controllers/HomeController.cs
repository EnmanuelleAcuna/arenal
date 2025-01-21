using arenal.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace arenal.Controllers;

public class HomeController : BaseController
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ILogger<HomeController> _logger;

	public HomeController(ApplicationUserManager<ApplicationUser> userManager,
		RoleManager<ApplicationRole> roleManager,
		IConfiguration configuration,
		IHttpContextAccessor contextAccesor,
		ILogger<HomeController> logger,
		IWebHostEnvironment environment)
		: base(userManager, roleManager, configuration, contextAccesor, environment)
	{
		_userManager = userManager;
		_logger = logger;
	}

	public IActionResult Privacidad()
	{
		return View();
	}

	[HttpGet]
	[Authorize]
	public async Task<ActionResult> Administracion()
	{
		string nombreUsuarioConectado = User.Identity.Name;
		ApplicationUser usuarioConectado = await _userManager.FindByNameAsync(nombreUsuarioConectado);
		ViewBag.NombreUsuario = string.Format("{0} {1} {2}", usuarioConectado.Name, usuarioConectado.FirstLastName,
			usuarioConectado.SecondLastName);
		return View();
	}
}
