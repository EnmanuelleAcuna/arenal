using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using plani.Identity;
using plani.Models.Data;
using plani.Models.Managers;
using plani.Models.ViewModels;

namespace plani.Controllers;

public class HomeController : BaseController {
    private readonly DashboardManager _dashboardManager;
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationUserManager _userManager;

    public HomeController(ApplicationUserManager userManager,
        ApplicationRoleManager roleManager,
        IConfiguration configuration,
        IHttpContextAccessor contextAccesor,
        ILogger<HomeController> logger,
        IWebHostEnvironment environment,
        DashboardManager dashboardManager,
        ApplicationDbContext dbContext)
        : base(userManager: userManager, roleManager: roleManager, configuration: configuration, contextAccessor: contextAccesor, environment: environment, dbContext: dbContext) {
        _userManager = userManager;
        _logger = logger;
        _dashboardManager = dashboardManager;
    }

    public IActionResult Privacidad() {
        return View();
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult> Administracion() {
        string nombreUsuarioConectado = User.Identity.Name;
        ApplicationUser usuarioConectado = await _userManager.FindByNameAsync(userName: nombreUsuarioConectado);
        ViewBag.NombreUsuario = string.Format("{0} {1} {2}", arg0: usuarioConectado.Name, arg1: usuarioConectado.FirstLastName,
            arg2: usuarioConectado.SecondLastName);
        return View();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ObtenerDatosDashboard() {
        try {
            DashboardViewModel datos = await _dashboardManager.ObtenerDatosDashboardAsync();
            return Json(data: datos);
        }
        catch (Exception ex) {
            _logger.LogError(exception: ex, "Error al obtener datos del dashboard");
            return StatusCode(statusCode: 500, new { error = "Error al cargar los datos del dashboard" });
        }
    }
}