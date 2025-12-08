using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plani.Identity;
using plani.Models;
using plani.Models.Data;
using plani.Models.ViewModels;

namespace plani.Controllers;

[Authorize]
public class ServiciosController : BaseController
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AreasManager _areasManager;
    private readonly ModalidadesManager _modalidadesManager;
    private readonly ServiciosManager _serviciosManager;
    private readonly ILogger<ServiciosController> _logger;

    public ServiciosController(
        ApplicationDbContext dbContext,
        AreasManager areasManager,
        ModalidadesManager modalidadesManager,
        ServiciosManager serviciosManager,
        ApplicationUserManager userManager,
        ApplicationRoleManager roleManager,
        IConfiguration configuration,
        IHttpContextAccessor contextAccesor,
        ILogger<ServiciosController> logger,
        IWebHostEnvironment environment)
        : base(userManager, roleManager, configuration, contextAccesor, environment)
    {
        _dbContext = dbContext;
        _areasManager = areasManager;
        _modalidadesManager = modalidadesManager;
        _serviciosManager = serviciosManager;
        _logger = logger;
    }

    #region Areas

    [HttpGet]
    public IActionResult Areas()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> DetalleArea(Guid id)
    {
        Area model = await _dbContext.Areas
            .AsNoTracking()
            .Include(a => a.Servicios)
            .Include(a => a.Contratos)
            .ThenInclude(c => c.Cliente)
            .Include(a => a.Proyectos)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(p => p.Cliente)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (model == null) return NotFound();

        return View(model);
    }

    // JSON endpoints for inline editing

    [HttpGet]
    public async Task<JsonResult> ObtenerAreas()
    {
        var viewModels = await _areasManager.ObtenerTodasAsync();
        return Json(new { success = true, data = viewModels });
    }

    [HttpPost]
    public async Task<JsonResult> AgregarAreaJson([FromBody] AgregarAreaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        var (success, data, error) = await _areasManager.CrearAsync(model, GetCurrentUser());

        if (success)
        {
            return Json(new { success = true, message = "Área agregada exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    [HttpPost]
    public async Task<JsonResult> EditarAreaJson([FromBody] EditarAreaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        var (success, data, error) = await _areasManager.ActualizarAsync(model, GetCurrentUser());

        if (success)
        {
            return Json(new { success = true, message = "Área actualizada exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    [HttpPost]
    public async Task<JsonResult> EliminarAreaJson([FromBody] EliminarAreaRequest request)
    {
        var (success, error) = await _areasManager.EliminarAsync(Guid.Parse(request.Id), GetCurrentUser());

        if (success)
        {
            return Json(new { success = true, message = "Área eliminada exitosamente" });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    #endregion

    #region Modalidades

    [HttpGet]
    public IActionResult Modalidades()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> DetalleModalidad(Guid id)
    {
        Modalidad model = await _dbContext.Modalidades
            .Include(a => a.Servicios)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (model == null) return NotFound();

        return View(model);
    }

    // JSON endpoints for inline editing

    [HttpGet]
    public async Task<JsonResult> ObtenerModalidades()
    {
        var viewModels = await _modalidadesManager.ObtenerTodasAsync();
        return Json(new { success = true, data = viewModels });
    }

    [HttpPost]
    public async Task<JsonResult> AgregarModalidadJson([FromBody] AgregarModalidadViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        var (success, data, error) = await _modalidadesManager.CrearAsync(model, GetCurrentUser());

        if (success)
        {
            return Json(new { success = true, message = "Modalidad agregada exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    [HttpPost]
    public async Task<JsonResult> EditarModalidadJson([FromBody] EditarModalidadViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        var (success, data, error) = await _modalidadesManager.ActualizarAsync(model, GetCurrentUser());

        if (success)
        {
            return Json(new { success = true, message = "Modalidad actualizada exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    [HttpPost]
    public async Task<JsonResult> EliminarModalidadJson([FromBody] EliminarModalidadRequest request)
    {
        var (success, error) = await _modalidadesManager.EliminarAsync(Guid.Parse(request.Id), GetCurrentUser());

        if (success)
        {
            return Json(new { success = true, message = "Modalidad eliminada exitosamente" });
        }

        return Json(new { success = false, errors = new[] { error } });
    }


    #endregion

    #region Servicios

    [HttpGet]
    public IActionResult Servicios()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> DetalleServicio(Guid id)
    {
        var model = await _serviciosManager.ObtenerDetallePorIdAsync(id);

        if (model == null) return NotFound();

        return View(model);
    }

    // JSON endpoints for inline editing

    [HttpGet]
    public async Task<JsonResult> ObtenerServicios()
    {
        var viewModels = await _serviciosManager.ObtenerTodosAsync();
        return Json(new { success = true, data = viewModels });
    }

    [HttpPost]
    public async Task<JsonResult> AgregarServicioJson([FromBody] AgregarServicioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        var (success, data, error) = await _serviciosManager.CrearAsync(model, GetCurrentUser());

        if (success)
        {
            return Json(new { success = true, message = "Servicio agregado exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    [HttpPost]
    public async Task<JsonResult> EditarServicioJson([FromBody] EditarServicioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        var (success, data, error) = await _serviciosManager.ActualizarAsync(model, GetCurrentUser());

        if (success)
        {
            return Json(new { success = true, message = "Servicio actualizado exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    [HttpPost]
    public async Task<JsonResult> EliminarServicioJson([FromBody] EliminarServicioRequest request)
    {
        var (success, error) = await _serviciosManager.EliminarAsync(Guid.Parse(request.Id), GetCurrentUser());

        if (success)
        {
            return Json(new { success = true, message = "Servicio eliminado exitosamente" });
        }

        return Json(new { success = false, errors = new[] { error } });
    }


    #endregion
}