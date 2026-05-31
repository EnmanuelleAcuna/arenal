using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using plani.Identity;
using plani.Models;
using plani.Models.Data;
using plani.Models.Domain;
using plani.Models.Managers;
using plani.Models.ViewModels;

namespace plani.Controllers;

[Authorize]
public class ClientesController : BaseController {
    private readonly AreasManager _areasManager;
    private readonly ClientesManager _clientesManager;
    private readonly ColaboradoresManager _colaboradoresManager;
    private readonly ILogger<ClientesController> _logger;
    private readonly ProyectosManager _proyectosManager;
    private readonly ServiciosManager _serviciosManager;
    private readonly SesionesManager _sesionesManager;
    private readonly ApplicationUserManager _userManager;

    public ClientesController(
        ApplicationUserManager userManager,
        ApplicationRoleManager roleManager,
        IConfiguration configuration,
        ILogger<ClientesController> logger,
        IHttpContextAccessor contextAccesor,
        IWebHostEnvironment environment,
        SesionesManager sesionesManager,
        ClientesManager clientesManager,
        AreasManager areasManager,
        ProyectosManager proyectosManager,
        ColaboradoresManager colaboradoresManager,
        ServiciosManager serviciosManager)
        : base(userManager: userManager, roleManager: roleManager, configuration: configuration, contextAccessor: contextAccesor, environment: environment) {
        _logger = logger;
        _userManager = userManager;
        _sesionesManager = sesionesManager;
        _clientesManager = clientesManager;
        _areasManager = areasManager;
        _proyectosManager = proyectosManager;
        _colaboradoresManager = colaboradoresManager;
        _serviciosManager = serviciosManager;
    }

    [HttpGet]
    public IActionResult Construccion() {
        return View();
    }

    #region Tipos de cliente

    [HttpGet]
    public IActionResult TiposCliente() {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> DetalleTipoCliente(Guid id) {
        TipoCliente tipoCliente = await _clientesManager.ObtenerTipoClientePorIdAsync(id: id);

        if (tipoCliente == null) {
            return NotFound();
        }

        return View(new DetalleTipoClienteViewModel(tipoCliente: tipoCliente));
    }

    [HttpGet]
    public async Task<JsonResult> ObtenerTiposCliente() {
        IEnumerable<TipoClienteListViewModel> viewModels = await _clientesManager.ObtenerTodosTiposClienteAsync();
        return Json(new { success = true, data = viewModels });
    }

    [HttpPost]
    public async Task<JsonResult> AgregarTipoClienteJson([FromBody] AgregarTipoClienteViewModel model) {
        if (!ModelState.IsValid) {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        (bool success, TipoClienteListViewModel data, string error) = await _clientesManager.CrearTipoClienteAsync(viewModel: model, GetCurrentUser());

        if (success) {
            return Json(new { success = true, message = "Tipo de cliente agregado exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    [HttpPost]
    public async Task<JsonResult> EditarTipoClienteJson([FromBody] EditarTipoClienteViewModel model) {
        if (!ModelState.IsValid) {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        (bool success, TipoClienteListViewModel data, string error) = await _clientesManager.ActualizarTipoClienteAsync(viewModel: model, GetCurrentUser());

        if (success) {
            return Json(new { success = true, message = "Tipo de cliente actualizado exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    [HttpPost]
    public async Task<JsonResult> EliminarTipoClienteJson([FromBody] EliminarTipoClienteRequest request) {
        (bool success, string error) = await _clientesManager.EliminarTipoClienteAsync(Guid.Parse(input: request.Id), GetCurrentUser());

        if (success) {
            return Json(new { success = true, message = "Tipo de cliente eliminado exitosamente" });
        }

        return Json(new { success = false, errors = new[] { error } });
    }

    #endregion

    #region Clientes

    [HttpGet]
    public async Task<IActionResult> Clientes(string palabraClave) {
        IEnumerable<Cliente> clientes = await _clientesManager.ObtenerTodosClientesAsync(palabraClave: palabraClave);
        IndexClientesViewModel model = new() { PalabraClave = palabraClave, Clientes = clientes };
        return View(model: model);
    }

    [HttpGet]
    public async Task<IActionResult> DetalleCliente(Guid id) {
        Cliente cliente = await _clientesManager.ObtenerClienteDetalleAsync(id: id);

        if (cliente == null) {
            return NotFound();
        }

        return View(new DetalleClienteViewModel(cliente: cliente));
    }

    [HttpGet]
    public async Task<IActionResult> AgregarCliente() {
        await PrepararViewBagsCliente();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarCliente(AgregarClienteViewModel model) {
        if (!ModelState.IsValid) {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Cliente)), GetModelStateErrors()));
            await PrepararViewBagsCliente();
            return View(model: model);
        }

        (bool success, _, string error) = await _clientesManager.CrearClienteAsync(model.ToEntity(), GetCurrentUser());

        if (success) {
            return RedirectToAction(nameof(Clientes));
        }

        ModelState.AddModelError("", error ?? Utils.MensajeErrorAgregar(nameof(Cliente)));
        await PrepararViewBagsCliente();
        return View(model: model);
    }

    [HttpGet]
    public async Task<IActionResult> EditarCliente(Guid id) {
        Cliente cliente = await _clientesManager.ObtenerClientePorIdAsync(id: id);

        if (cliente == null) {
            return NotFound();
        }

        await PrepararViewBagsCliente();
        return View(new EditarClienteViewModel(cliente: cliente));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditarCliente(EditarClienteViewModel model) {
        if (!ModelState.IsValid) {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorActualizar(nameof(Cliente)), GetModelStateErrors()));
            await PrepararViewBagsCliente();
            return View(model: model);
        }

        (bool success, string error) = await _clientesManager.ActualizarClienteAsync(model.ToEntity(), GetCurrentUser());

        if (success) {
            return RedirectToAction(nameof(Clientes));
        }

        ModelState.AddModelError("", error ?? Utils.MensajeErrorActualizar(nameof(Cliente)));
        await PrepararViewBagsCliente();
        return View(model: model);
    }

    [HttpGet]
    public async Task<IActionResult> EliminarCliente(Guid id) {
        Cliente cliente = await _clientesManager.ObtenerClientePorIdAsync(id: id);

        if (cliente == null) {
            return NotFound();
        }

        return View(new EliminarClienteViewModel(cliente: cliente));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarCliente(EliminarClienteViewModel model) {
        (bool success, string error) = await _clientesManager.EliminarClienteAsync(id: model.Id, GetCurrentUser());

        if (success) {
            return RedirectToAction(nameof(Clientes));
        }

        ModelState.AddModelError("", error ?? Utils.MensajeErrorEliminar(nameof(Cliente)));
        return View(model: model);
    }

    #endregion

    #region Contratos

    [HttpGet]
    public async Task<IActionResult> Contratos() {
        IEnumerable<Contrato> contratos = await _clientesManager.ObtenerTodosContratosAsync();
        return View(contratos.Select(c => new DetalleContratoViewModel(contrato: c)));
    }

    [HttpGet]
    public async Task<IActionResult> DetalleContrato(Guid id) {
        Contrato contrato = await _clientesManager.ObtenerContratoDetalleAsync(id: id);

        if (contrato == null) {
            return NotFound();
        }

        return View(new DetalleContratoViewModel(contrato: contrato));
    }

    [HttpGet]
    public async Task<IActionResult> AgregarContrato() {
        await PrepararViewBagsContrato();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarContrato(AgregarContratoViewModel model) {
        if (!ModelState.IsValid) {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Contrato)), GetModelStateErrors()));
            await PrepararViewBagsContrato();
            return View(model: model);
        }

        (bool success, _, string error) = await _clientesManager.CrearContratoAsync(model.ToEntity(), GetCurrentUser());

        if (success) {
            return RedirectToAction(nameof(Contratos));
        }

        ModelState.AddModelError("", error ?? Utils.MensajeErrorAgregar(nameof(Contrato)));
        await PrepararViewBagsContrato();
        return View(model: model);
    }

    [HttpGet]
    public async Task<IActionResult> EditarContrato(Guid id) {
        Contrato contrato = await _clientesManager.ObtenerContratoPorIdAsync(id: id);

        if (contrato == null) {
            return NotFound();
        }

        await PrepararViewBagsContrato();
        return View(new EditarContratoViewModel(contrato: contrato));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditarContrato(EditarContratoViewModel model) {
        if (!ModelState.IsValid) {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorActualizar(nameof(Contrato)), GetModelStateErrors()));
            await PrepararViewBagsContrato();
            return View(model: model);
        }

        (bool success, string error) = await _clientesManager.ActualizarContratoAsync(model.ToEntity(), GetCurrentUser());

        if (success) {
            return RedirectToAction(nameof(Contratos));
        }

        ModelState.AddModelError("", error ?? Utils.MensajeErrorActualizar(nameof(Contrato)));
        await PrepararViewBagsContrato();
        return View(model: model);
    }

    [HttpGet]
    public async Task<IActionResult> EliminarContrato(Guid id) {
        Contrato contrato = await _clientesManager.ObtenerContratoConClienteAsync(id: id);

        if (contrato == null) {
            return NotFound();
        }

        return View(new EliminarContratoViewModel(contrato: contrato));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarContrato(EliminarContratoViewModel model) {
        (bool success, string error) = await _clientesManager.EliminarContratoAsync(id: model.Id, GetCurrentUser());

        if (success) {
            return RedirectToAction(nameof(Contratos));
        }

        ModelState.AddModelError("", error ?? Utils.MensajeErrorEliminar(nameof(Contrato)));
        return View(model: model);
    }

    #endregion

    #region Proyectos

    [HttpGet]
    public async Task<IActionResult> Proyectos(string palabraClave) {
        IEnumerable<Proyecto> proyectos = await _proyectosManager.ObtenerTodosProyectosAsync(palabraClave: palabraClave);
        IndexProyectosViewModel model = new() { PalabraClave = palabraClave, Proyectos = proyectos };
        return View(model: model);
    }

    [HttpGet]
    public async Task<IActionResult> DetalleProyecto(Guid id) {
        Proyecto proyecto = await _proyectosManager.ObtenerProyectoDetalleAsync(id: id);

        if (proyecto == null) {
            return NotFound();
        }

        return View(new DetalleProyectoViewModel(proyecto: proyecto));
    }

    [HttpGet]
    public async Task<IActionResult> AgregarProyecto() {
        await PrepararViewBagsProyecto();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarProyecto(AgregarProyectoViewModel model) {
        if (!ModelState.IsValid) {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Proyecto)), GetModelStateErrors()));
            await PrepararViewBagsProyecto();
            return View(model: model);
        }

        (bool success, _, string error) = await _proyectosManager.CrearProyectoAsync(model.ToEntity(), GetCurrentUser());

        if (success) {
            return RedirectToAction(nameof(Proyectos));
        }

        ModelState.AddModelError("", error ?? Utils.MensajeErrorAgregar(nameof(Proyecto)));
        await PrepararViewBagsProyecto();
        return View(model: model);
    }

    [HttpGet]
    public async Task<IActionResult> EditarProyecto(Guid id) {
        Proyecto proyecto = await _proyectosManager.ObtenerProyectoPorIdAsync(id: id);

        if (proyecto == null) {
            return NotFound();
        }

        await PrepararViewBagsProyecto();
        return View(new EditarProyectoViewModel(proyecto: proyecto));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditarProyecto(EditarProyectoViewModel model) {
        if (!ModelState.IsValid) {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorActualizar(nameof(Proyecto)), GetModelStateErrors()));
            await PrepararViewBagsProyecto();
            return View(model: model);
        }

        (bool success, string error) = await _proyectosManager.ActualizarProyectoAsync(model.ToEntity(), GetCurrentUser());

        if (success) {
            return RedirectToAction(nameof(Proyectos));
        }

        ModelState.AddModelError("", error ?? Utils.MensajeErrorActualizar(nameof(Proyecto)));
        await PrepararViewBagsProyecto();
        return View(model: model);
    }

    [HttpGet]
    public async Task<IActionResult> EliminarProyecto(Guid id) {
        Proyecto proyecto = await _proyectosManager.ObtenerProyectoConContratoAsync(id: id);

        if (proyecto == null) {
            return NotFound();
        }

        return View(new EliminarProyectoViewModel(proyecto: proyecto));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarProyecto(EliminarProyectoViewModel model) {
        (bool success, string error) = await _proyectosManager.EliminarProyectoAsync(id: model.Id, GetCurrentUser());

        if (success) {
            return RedirectToAction(nameof(Proyectos));
        }

        ModelState.AddModelError("", error ?? Utils.MensajeErrorEliminar(nameof(Proyecto)));
        return View(model: model);
    }

    #endregion

    #region Asignaciones

    [HttpGet]
    public async Task<IActionResult> Asignaciones() {
        await PrepararViewBagsAsignacionesIndex();

        List<Asignacion> asignaciones = await _proyectosManager.ObtenerTodasAsignacionesAsync();

        AsignacionesIndexViewModel viewModel = new() {
            ProyectosAsignaciones = _proyectosManager.AgruparAsignacionesPorProyecto(asignaciones: asignaciones)
        };

        return View(model: viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Asignaciones(AsignacionesIndexViewModel model) {
        await PrepararViewBagsAsignacionesIndex();

        List<Asignacion> asignaciones = await _proyectosManager.ObtenerAsignacionesFiltradasAsync(idUsuario: model.IdUsuario, idProyecto: model.IdProyecto);

        AsignacionesIndexViewModel viewModel = new() {
            IdUsuario = model.IdUsuario,
            IdProyecto = model.IdProyecto,
            ProyectosAsignaciones = _proyectosManager.AgruparAsignacionesPorProyecto(asignaciones: asignaciones)
        };

        return View(model: viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarAsignaciones(
        string idUsuario = null,
        string idProyecto = null) {
        List<Asignacion> asignaciones = await _proyectosManager.ObtenerAsignacionesParaExportarAsync(idUsuario: idUsuario, idProyecto: idProyecto);
        byte[] content = _proyectosManager.ExportarAsignacionesExcel(asignaciones: asignaciones);
        string fileName = $"Asignaciones_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        return File(fileContents: content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileDownloadName: fileName);
    }

    [HttpGet]
    public async Task<IActionResult> MisAsignaciones() {
        ApplicationUser usuario = await _userManager.FindByEmailAsync(GetCurrentUser());

        List<Asignacion> asignaciones = await _proyectosManager.ObtenerAsignacionesUsuarioAsync(idUsuario: usuario.Id);

        AsignacionesIndexViewModel viewModel = new() {
            ProyectosAsignaciones = _proyectosManager.AgruparAsignacionesPorProyecto(asignaciones: asignaciones)
        };

        return View(model: viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> AsignarProyecto(Guid id) {
        ApplicationUser colaborador = await _userManager.FindByIdAsync(id.ToString());
        AgregarAsignacionModel model = new() {
            NombreColaborador = colaborador.FullName,
            IdUsuario = id
        };

        ViewBag.Proyectos = await _proyectosManager.ObtenerParaDropdownAsync();

        return View(model: model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarProyecto(AgregarAsignacionModel model) {
        if (!ModelState.IsValid) {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Asignacion)), GetModelStateErrors()));

            ViewBag.Proyectos = await _proyectosManager.ObtenerParaDropdownAsync();
            return View(model: model);
        }

        (bool success, _, string error) = await _proyectosManager.CrearAsignacionAsync(model: model, GetCurrentUser());

        if (success) {
            return RedirectToAction(nameof(Asignaciones));
        }

        ModelState.AddModelError("", error ?? Utils.MensajeErrorAgregar(nameof(Asignacion)));
        ViewBag.Proyectos = await _proyectosManager.ObtenerParaDropdownAsync();
        return View(model: model);
    }

    [HttpGet]
    public async Task<IActionResult> EliminarAsignacion(Guid id) {
        Asignacion asignacion = await _proyectosManager.ObtenerAsignacionDetalleAsync(id: id);

        if (asignacion == null) {
            return NotFound();
        }

        return View(new EliminarAsignacionViewModel(asignacion: asignacion));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarAsignacion(EliminarAsignacionViewModel model) {
        (bool success, string idColaborador, string error) = await _proyectosManager.EliminarAsignacionAsync(id: model.Id, GetCurrentUser());

        if (success) {
            return RedirectToAction("DetalleColaborador", "Cuentas", new { id = idColaborador });
        }

        ModelState.AddModelError("", error ?? Utils.MensajeErrorEliminar(nameof(Asignacion)));
        return View(model: model);
    }

    #endregion

    #region Sesiones

    [HttpGet]
    public async Task<IActionResult> Sesiones(
        string idUsuario = null,
        string idProyecto = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null) {
        // Si no hay fechas, usar mes actual por defecto
        if (fechaInicio == null && fechaFin == null) {
            (fechaInicio, fechaFin) = _sesionesManager.ObtenerRangoMesActual();
        }

        ViewBag.Colaboradores = await _colaboradoresManager.ObtenerParaDropdownAsync();
        ViewBag.Proyectos = await _proyectosManager.ObtenerParaDropdownAsync();

        List<Sesion> sesiones = await _sesionesManager.ObtenerSesionesFiltradas(idUsuario: idUsuario, idProyecto: idProyecto, fechaInicio: fechaInicio, fechaFin: fechaFin);

        SesionesIndexViewModel viewModel = new() {
            IdUsuario = idUsuario,
            IdProyecto = idProyecto,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Sesiones = sesiones
        };

        return View(model: viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarSesiones(
        string idUsuario = null,
        string idProyecto = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null) {
        List<Sesion> sesiones = await _sesionesManager.ObtenerSesionesFiltradas(idUsuario: idUsuario, idProyecto: idProyecto, fechaInicio: fechaInicio, fechaFin: fechaFin);
        byte[] content = _sesionesManager.ExportarSesionesExcel(sesiones: sesiones);
        string fileName = $"Sesiones_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        return File(fileContents: content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileDownloadName: fileName);
    }

    [HttpGet]
    public async Task<IActionResult> MisSesiones(
        string idProyecto = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null) {
        ApplicationUser usuario = await _userManager.FindByEmailAsync(GetCurrentUser());

        ViewBag.Proyectos = await _proyectosManager.ObtenerAsignadosParaDropdownAsync(idUsuario: usuario.Id);

        List<Sesion> sesiones = await _sesionesManager.ObtenerSesionesFiltradas(
            idUsuario: usuario.Id, idProyecto: idProyecto, fechaInicio: fechaInicio, fechaFin: fechaFin);

        // Si no hay filtros, limitar a 25 sesiones
        if (fechaInicio == null && fechaFin == null && string.IsNullOrEmpty(value: idProyecto)) {
            sesiones = sesiones.Take(count: 25).ToList();
        }

        SesionesIndexViewModel viewModel = new() {
            IdProyecto = idProyecto,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Sesiones = sesiones,
            SesionesActivas = await _sesionesManager.ObtenerSesionesActivas(idUsuario: usuario.Id)
        };

        return View(model: viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> AgregarSesion() {
        ApplicationUser colaborador = await _userManager.FindByEmailAsync(GetCurrentUser());

        ViewBag.Servicios = await _serviciosManager.ObtenerParaDropdownAsync();
        ViewBag.Proyectos = await _proyectosManager.ObtenerAsignadosParaDropdownAsync(idUsuario: colaborador.Id);

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarSesion(AgregarSesionModel model) {
        ApplicationUser colaborador = await _userManager.FindByEmailAsync(GetCurrentUser());

        if (!ModelState.IsValid) {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Sesion)), GetModelStateErrors()));

            ViewBag.Servicios = await _serviciosManager.ObtenerParaDropdownAsync();
            ViewBag.Proyectos = await _proyectosManager.ObtenerAsignadosParaDropdownAsync(idUsuario: colaborador.Id);

            return View(model: model);
        }

        bool exito = await _sesionesManager.CrearSesionManual(model: model, idColaborador: colaborador.Id, GetCurrentUser());

        if (exito) {
            return RedirectToAction(nameof(MisSesiones));
        }

        ModelState.AddModelError("", Utils.MensajeErrorAgregar(nameof(Sesion)));
        return View(model: model);
    }

    [HttpGet]
    public ActionResult ErrorIniciarSesion() {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> IniciarSesion() {
        ApplicationUser colaborador = await _userManager.FindByEmailAsync(GetCurrentUser());

        // Validar que no tenga más de 1 sesión activa
        int sesionesActivas = await _sesionesManager.ContarSesionesActivas(idUsuario: colaborador.Id);
        if (sesionesActivas > 1) {
            return RedirectToAction(nameof(ErrorIniciarSesion));
        }

        ViewBag.Servicios = await _serviciosManager.ObtenerParaDropdownAsync();
        ViewBag.Proyectos = await _proyectosManager.ObtenerAsignadosParaDropdownAsync(idUsuario: colaborador.Id);

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarSesion(AgregarSesionModel model) {
        ApplicationUser colaborador = await _userManager.FindByEmailAsync(GetCurrentUser());

        if (!ModelState.IsValid) {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Sesion)), GetModelStateErrors()));

            ViewBag.Servicios = await _serviciosManager.ObtenerParaDropdownAsync();
            ViewBag.Proyectos = await _proyectosManager.ObtenerAsignadosParaDropdownAsync(idUsuario: colaborador.Id);

            return View(model: model);
        }

        (bool exito, string error) = await _sesionesManager.IniciarSesion(model: model, idColaborador: colaborador.Id, GetCurrentUser());

        if (exito) {
            return RedirectToAction(nameof(MisSesiones));
        }

        ModelState.AddModelError("", error ?? Utils.MensajeErrorAgregar(nameof(Sesion)));
        return View(model: model);
    }

    [HttpGet]
    public async Task<IActionResult> PausarSesion(Guid id) {
        Sesion sesion = await _sesionesManager.ObtenerSesionPorId(id: id);

        if (sesion == null) {
            return NotFound();
        }

        PausarSesionModel model = new() {
            IdSesion = sesion.Id,
            IdProyecto = sesion.IdProyecto,
            NombreProyecto = sesion.Proyecto.Nombre,
            IdServicio = sesion.IdServicio,
            NombreServicio = sesion.Servicio.Nombre,
            Descripcion = sesion.Descripcion,
            Horas = sesion.Horas
        };

        return View(model: model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PausarSesion(PausarSesionModel model) {
        if (!ModelState.IsValid) {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Sesion)), GetModelStateErrors()));
            return View(model: model);
        }

        (bool exito, string error) = await _sesionesManager.PausarSesion(idSesion: model.IdSesion, descripcion: model.Descripcion, GetCurrentUser());

        if (exito) {
            return RedirectToAction(nameof(MisSesiones));
        }

        ModelState.AddModelError("", error ?? Utils.MensajeErrorAgregar(nameof(Sesion)));
        return View(model: model);
    }

    [HttpGet]
    public async Task<IActionResult> ReanudarSesion(Guid id) {
        Sesion sesion = await _sesionesManager.ObtenerSesionPorId(id: id);

        if (sesion == null) {
            return NotFound();
        }

        PausarSesionModel model = new() {
            IdSesion = sesion.Id,
            IdProyecto = sesion.IdProyecto,
            NombreProyecto = sesion.Proyecto.Nombre,
            IdServicio = sesion.IdServicio,
            NombreServicio = sesion.Servicio.Nombre,
            Descripcion = sesion.Descripcion,
            Horas = sesion.Horas
        };

        return View(model: model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReanudarSesion(PausarSesionModel model) {
        if (!ModelState.IsValid) {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Sesion)), GetModelStateErrors()));
            return View(model: model);
        }

        (bool exito, string error) = await _sesionesManager.ReanudarSesion(idSesion: model.IdSesion, descripcion: model.Descripcion, GetCurrentUser());

        if (exito) {
            return RedirectToAction(nameof(MisSesiones));
        }

        ModelState.AddModelError("", error ?? Utils.MensajeErrorAgregar(nameof(Sesion)));
        return View(model: model);
    }

    [HttpGet]
    public async Task<IActionResult> FinalizarSesion(Guid id) {
        Sesion sesion = await _sesionesManager.ObtenerSesionPorId(id: id);

        if (sesion == null) {
            return NotFound();
        }

        FinalizarSesionModel model = new() {
            IdSesion = sesion.Id,
            IdProyecto = sesion.IdProyecto,
            NombreProyecto = sesion.Proyecto.Nombre,
            IdServicio = sesion.IdServicio,
            NombreServicio = sesion.Servicio.Nombre,
            Descripcion = sesion.Descripcion
        };

        return View(model: model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizarSesion(FinalizarSesionModel model) {
        if (!ModelState.IsValid) {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Sesion)), GetModelStateErrors()));
            return View(model: model);
        }

        (bool exito, string error) = await _sesionesManager.FinalizarSesion(idSesion: model.IdSesion, descripcion: model.Descripcion, GetCurrentUser());

        if (exito) {
            return RedirectToAction(nameof(MisSesiones));
        }

        ModelState.AddModelError("", error ?? Utils.MensajeErrorAgregar(nameof(Sesion)));
        return View(model: model);
    }

    [HttpGet]
    public async Task<IActionResult> DetalleSesion(Guid id) {
        Sesion sesion = await _sesionesManager.ObtenerSesionPorId(id: id);

        if (sesion == null) {
            return NotFound();
        }

        return View(new DetalleSesionViewModel(sesion: sesion));
    }

    #endregion

    #region Helpers

    private async Task PrepararViewBagsCliente() {
        ViewBag.TiposCliente = await _clientesManager.ObtenerTiposClienteParaDropdownAsync();
    }

    private async Task PrepararViewBagsContrato() {
        ViewBag.Clientes = await _clientesManager.ObtenerClientesParaDropdownAsync();
        ViewBag.Areas = await _areasManager.ObtenerParaDropdownAsync();
    }

    private async Task PrepararViewBagsProyecto() {
        ViewBag.Contratos = await _clientesManager.ObtenerContratosParaDropdownAsync();
        ViewBag.Areas = await _areasManager.ObtenerParaDropdownAsync();
        ViewBag.Responsables = await _colaboradoresManager.ObtenerParaDropdownAsync();
    }

    private async Task PrepararViewBagsAsignacionesIndex() {
        ViewBag.Colaboradores = await _colaboradoresManager.ObtenerParaDropdownAsync();
        ViewBag.Proyectos = await _proyectosManager.ObtenerParaDropdownAsync();
    }

    #endregion
}