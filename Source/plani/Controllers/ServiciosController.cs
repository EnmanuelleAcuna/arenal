using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using plani.Identity;
using plani.Models.Domain;
using plani.Models.Managers;
using plani.Models.ViewModels;

namespace plani.Controllers;

[Authorize]
public class ServiciosController : BaseController {
    private readonly AreasManager _areasManager;
    private readonly ILogger<ServiciosController> _logger;
    private readonly ModalidadesManager _modalidadesManager;
    private readonly ServiciosManager _serviciosManager;

    public ServiciosController(
        AreasManager areasManager,
        ModalidadesManager modalidadesManager,
        ServiciosManager serviciosManager,
        ApplicationUserManager userManager,
        ApplicationRoleManager roleManager,
        IConfiguration configuration,
        IHttpContextAccessor contextAccesor,
        ILogger<ServiciosController> logger,
        IWebHostEnvironment environment)
        : base(userManager: userManager, roleManager: roleManager, configuration: configuration, contextAccessor: contextAccesor, environment: environment) {
        _areasManager = areasManager;
        _modalidadesManager = modalidadesManager;
        _serviciosManager = serviciosManager;
        _logger = logger;
    }

    #region Areas

    [HttpGet]
    public IActionResult Areas() {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> DetalleArea(Guid id) {
        Area area = await _areasManager.ObtenerDetalleAsync(id: id);

        if (area == null) {
            return NotFound();
        }

        return View(new DetalleAreaViewModel(area: area));
    }

    // JSON endpoints for inline editing

    [HttpGet]
    public async Task<JsonResult> ObtenerAreas() {
        IEnumerable<AreaListViewModel> viewModels = await _areasManager.ObtenerTodasAsync();
        return Json(new { success = true, data = viewModels });
    }

    [HttpPost]
    public async Task<JsonResult> AgregarAreaJson([FromBody] AgregarAreaViewModel model) {
        if (!ModelState.IsValid) {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        (bool success, AreaListViewModel data, string error) = await _areasManager.CrearAsync(viewModel: model, GetCurrentUser());

        if (success) {
            return Json(new { success = true, message = "Área agregada exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    [HttpPost]
    public async Task<JsonResult> EditarAreaJson([FromBody] EditarAreaViewModel model) {
        if (!ModelState.IsValid) {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        (bool success, AreaListViewModel data, string error) = await _areasManager.ActualizarAsync(viewModel: model, GetCurrentUser());

        if (success) {
            return Json(new { success = true, message = "Área actualizada exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    [HttpPost]
    public async Task<JsonResult> EliminarAreaJson([FromBody] EliminarAreaRequest request) {
        (bool success, string error) = await _areasManager.EliminarAsync(Guid.Parse(input: request.Id), GetCurrentUser());

        if (success) {
            return Json(new { success = true, message = "Área eliminada exitosamente" });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    #endregion

    #region Modalidades

    [HttpGet]
    public IActionResult Modalidades() {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> DetalleModalidad(Guid id) {
        Modalidad modalidad = await _modalidadesManager.ObtenerDetalleAsync(id: id);

        if (modalidad == null) {
            return NotFound();
        }

        return View(new DetalleModalidadViewModel(modalidad: modalidad));
    }

    // JSON endpoints for inline editing

    [HttpGet]
    public async Task<JsonResult> ObtenerModalidades() {
        IEnumerable<ModalidadListViewModel> viewModels = await _modalidadesManager.ObtenerTodasAsync();
        return Json(new { success = true, data = viewModels });
    }

    [HttpPost]
    public async Task<JsonResult> AgregarModalidadJson([FromBody] AgregarModalidadViewModel model) {
        if (!ModelState.IsValid) {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        (bool success, ModalidadListViewModel data, string error) = await _modalidadesManager.CrearAsync(viewModel: model, GetCurrentUser());

        if (success) {
            return Json(new { success = true, message = "Modalidad agregada exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    [HttpPost]
    public async Task<JsonResult> EditarModalidadJson([FromBody] EditarModalidadViewModel model) {
        if (!ModelState.IsValid) {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        (bool success, ModalidadListViewModel data, string error) = await _modalidadesManager.ActualizarAsync(viewModel: model, GetCurrentUser());

        if (success) {
            return Json(new { success = true, message = "Modalidad actualizada exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    [HttpPost]
    public async Task<JsonResult> EliminarModalidadJson([FromBody] EliminarModalidadRequest request) {
        (bool success, string error) = await _modalidadesManager.EliminarAsync(Guid.Parse(input: request.Id), GetCurrentUser());

        if (success) {
            return Json(new { success = true, message = "Modalidad eliminada exitosamente" });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    #endregion

    #region Servicios

    [HttpGet]
    public IActionResult Servicios() {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> DetalleServicio(Guid id) {
        ServicioDetalleViewModel model = await _serviciosManager.ObtenerDetallePorIdAsync(id: id);

        if (model == null) {
            return NotFound();
        }

        return View(model: model);
    }

    // JSON endpoints for inline editing

    [HttpGet]
    public async Task<JsonResult> ObtenerServicios() {
        IEnumerable<ServicioListViewModel> viewModels = await _serviciosManager.ObtenerTodosAsync();
        return Json(new { success = true, data = viewModels });
    }

    [HttpPost]
    public async Task<JsonResult> AgregarServicioJson([FromBody] AgregarServicioViewModel model) {
        if (!ModelState.IsValid) {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        (bool success, ServicioListViewModel data, string error) = await _serviciosManager.CrearAsync(viewModel: model, GetCurrentUser());

        if (success) {
            return Json(new { success = true, message = "Servicio agregado exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    [HttpPost]
    public async Task<JsonResult> EditarServicioJson([FromBody] EditarServicioViewModel model) {
        if (!ModelState.IsValid) {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        (bool success, ServicioListViewModel data, string error) = await _serviciosManager.ActualizarAsync(viewModel: model, GetCurrentUser());

        if (success) {
            return Json(new { success = true, message = "Servicio actualizado exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    [HttpPost]
    public async Task<JsonResult> EliminarServicioJson([FromBody] EliminarServicioRequest request) {
        (bool success, string error) = await _serviciosManager.EliminarAsync(Guid.Parse(input: request.Id), GetCurrentUser());

        if (success) {
            return Json(new { success = true, message = "Servicio eliminado exitosamente" });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    #endregion
}