using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using plani.Identity;
using plani.Models;
using plani.Models.Data;

namespace plani.Controllers;

public class HomeController : BaseController
{
	private readonly ApplicationUserManager _userManager;
	private readonly ILogger<HomeController> _logger;
	private readonly DashboardManager _dashboardManager;

	public HomeController(ApplicationUserManager userManager,
		ApplicationRoleManager roleManager,
		IConfiguration configuration,
		IHttpContextAccessor contextAccesor,
		ILogger<HomeController> logger,
		IWebHostEnvironment environment,
		DashboardManager dashboardManager,
		ApplicationDbContext dbContext)
		: base(userManager, roleManager, configuration, contextAccesor, environment, dbContext)
	{
		_userManager = userManager;
		_logger = logger;
		_dashboardManager = dashboardManager;
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

	[HttpGet]
	[Authorize]
	public async Task<IActionResult> ObtenerDatosDashboard()
	{
		try
		{
			var datos = await _dashboardManager.ObtenerDatosDashboardAsync();
			return Json(datos);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al obtener datos del dashboard");
			return StatusCode(500, new { error = "Error al cargar los datos del dashboard" });
		}
	}
}
