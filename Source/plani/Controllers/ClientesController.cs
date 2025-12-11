using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plani.Identity;
using plani.Models;
using plani.Models.Data;
using plani.Models.ViewModels;

namespace plani.Controllers;

[Authorize]
public class ClientesController : BaseController
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ClientesController> _logger;
    private readonly ApplicationUserManager _userManager;
    private readonly IEmailSender _emailSender;
    private readonly SesionesManager _sesionesManager;

    public ClientesController(
        ApplicationDbContext dbContext,
        ApplicationUserManager userManager,
        ApplicationRoleManager roleManager,
        IConfiguration configuration,
        ILogger<ClientesController> logger,
        IHttpContextAccessor contextAccesor,
        IWebHostEnvironment environment,
        IEmailSender emailSender,
        SesionesManager sesionesManager)
        : base(userManager, roleManager, configuration, contextAccesor, environment, dbContext)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userManager = userManager;
        _emailSender = emailSender;
        _sesionesManager = sesionesManager;
    }

    [HttpGet]
    public IActionResult Construccion()
    {
        return View();
    }

    #region Tipos de cliente

    [HttpGet]
    public IActionResult TiposCliente()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> DetalleTipoCliente(Guid id)
    {
        TipoCliente model = await _dbContext.TiposCliente
            .Include(tc => tc.Clientes)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (model == null) return NotFound();

        return View(model);
    }

    // JSON endpoints for inline editing

    [HttpGet]
    public async Task<JsonResult> ObtenerTiposCliente()
    {
        var tiposCliente = await _dbContext.TiposCliente
            .OrderBy(tc => tc.Nombre)
            .ToListAsync();

        var viewModels = tiposCliente.Select(tc => new TipoClienteListViewModel(tc));

        return Json(new { success = true, data = viewModels });
    }

    [HttpPost]
    public async Task<JsonResult> AgregarTipoClienteJson([FromBody] AgregarTipoClienteViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        var tipoCliente = model.ToEntity();
        tipoCliente.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);

        await _dbContext.TiposCliente.AddAsync(tipoCliente);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0)
        {
            var data = new TipoClienteListViewModel(tipoCliente);
            return Json(new { success = true, message = "Tipo de cliente agregado exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { "Error al agregar el tipo de cliente" } });
    }

    [HttpPost]
    public async Task<JsonResult> EditarTipoClienteJson([FromBody] EditarTipoClienteViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors });
        }

        var tipoCliente = await _dbContext.TiposCliente.FindAsync(Guid.Parse(model.Id));

        if (tipoCliente == null)
        {
            return Json(new { success = false, errors = new[] { "Tipo de cliente no encontrado" } });
        }

        var updatedEntity = model.ToEntity();
        tipoCliente.Actualizar(updatedEntity, GetCurrentUser());

        _dbContext.TiposCliente.Update(tipoCliente);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0)
        {
            var data = new TipoClienteListViewModel(tipoCliente);
            return Json(new { success = true, message = "Tipo de cliente actualizado exitosamente", data });
        }

        return Json(new { success = false, errors = new[] { "Error al actualizar el tipo de cliente" } });
    }

    [HttpPost]
    public async Task<JsonResult> EliminarTipoClienteJson([FromBody] EliminarTipoClienteRequest request)
    {
        var tipoCliente = await _dbContext.TiposCliente.FindAsync(Guid.Parse(request.Id));

        if (tipoCliente == null)
        {
            return Json(new { success = false, errors = new[] { "Tipo de cliente no encontrado" } });
        }

        // Verificar si el tipo de cliente tiene clientes asignados
        var clientes = await _dbContext.Clientes
            .Where(c => c.IdTipoCliente == tipoCliente.Id)
            .ToListAsync();

        if (clientes.Any())
        {
            return Json(new { success = false, errors = new[] { "No se puede eliminar el tipo de cliente porque tiene clientes asignados" } });
        }

        tipoCliente.Eliminar(GetCurrentUser());
        _dbContext.TiposCliente.Update(tipoCliente);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0)
        {
            return Json(new { success = true, message = "Tipo de cliente eliminado exitosamente" });
        }

        return Json(new { success = false, errors = new[] { "Error al eliminar el tipo de cliente" } });
    }


    #endregion

    #region Clientes

    [HttpGet]
    public async Task<IActionResult> Clientes(string palabraClave)
    {
        IEnumerable<Cliente> clientes = await _dbContext.Clientes
            .Where(c => palabraClave == null || (c.Nombre.ToLower().Contains(palabraClave.ToLower())) ||
                        (c.Descripcion.ToLower().Contains(palabraClave.ToLower())) ||
                        (c.Direccion.ToLower().Contains(palabraClave.ToLower())))
            .OrderBy(c => c.Nombre)
            .Include(c => c.TipoCliente)
            .ToListAsync();

        IndexClientesViewModel model = new() { PalabraClave = palabraClave, Clientes = clientes };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DetalleCliente(Guid id)
    {
        Cliente model = await _dbContext.Clientes
            .Include(c => c.TipoCliente)
            .Include(c => c.Contratos).ThenInclude(c => c.Proyectos)
            .ThenInclude(p => p.Area)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> AgregarCliente()
    {
        IEnumerable<TipoCliente> tiposCliente = await _dbContext.TiposCliente.ToListAsync();
        ViewBag.TiposCliente = tiposCliente.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarCliente(Cliente model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Cliente)), GetModelStateErrors()));

            IEnumerable<TipoCliente> tiposCliente = await _dbContext.TiposCliente.ToListAsync();
            ViewBag.TiposCliente = tiposCliente.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

            return View(model);
        }

        model.Nombre = model.Nombre.Trim();
        model.Descripcion = model.Descripcion.Trim();
        model.Direccion = model.Direccion.Trim();

        model.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
        await _dbContext.Clientes.AddAsync(model);

        // Add new Contrato for that Cliente
        Contrato contrato = new Contrato
        {
            IdCliente = model.Id,
            Identificacion = "CONTRATO-" + model.Id,
            Descripcion = "Contrato de " + model.Nombre.Trim(),
            FechaInicio = DateTime.UtcNow,
            IdArea = Guid.Parse("f9c46324-5f71-4faf-0171-08dd2fd1b693")
        };

        contrato.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
        await _dbContext.Contratos.AddAsync(contrato);

        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Clientes));

        ModelState.AddModelError("", Utils.MensajeErrorAgregar(nameof(Cliente)));
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditarCliente(Guid id)
    {
        Cliente model = await _dbContext.Clientes.FindAsync(id);

        if (model == null) return NotFound();

        IEnumerable<TipoCliente> tiposCliente = await _dbContext.TiposCliente.ToListAsync();
        ViewBag.TiposCliente = tiposCliente.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditarCliente(Cliente model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorActualizar(nameof(Cliente)), GetModelStateErrors()));

            IEnumerable<TipoCliente> tiposCliente = await _dbContext.TiposCliente.ToListAsync();
            ViewBag.TiposCliente = tiposCliente.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

            return View(model);
        }

        Cliente cliente = await _dbContext.Clientes.FindAsync(model.Id);

        if (cliente == null) return NotFound();

        cliente.Actualizar(model, GetCurrentUser());
        _dbContext.Clientes.Update(cliente);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Clientes));
        ModelState.AddModelError("", Utils.MensajeErrorActualizar(nameof(Cliente)));

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EliminarCliente(Guid id)
    {
        var model = await _dbContext.Clientes.FindAsync(id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarCliente(Cliente model)
    {
        Cliente cliente = await _dbContext.Clientes.FindAsync(model.Id);

        if (cliente == null) return NotFound();

        cliente.Eliminar(GetCurrentUser());
        _dbContext.Clientes.Update(cliente);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Clientes));

        ModelState.AddModelError("", Utils.MensajeErrorEliminar(nameof(Cliente)));
        return View(model);
    }

    #endregion

    #region Contratos

    [HttpGet]
    public async Task<IActionResult> Contratos()
    {
        List<Contrato> contratos =
            await _dbContext.Contratos.Include(c => c.Cliente).Include(c => c.Area).ToListAsync();
        return View(contratos);
    }

    [HttpGet]
    public async Task<IActionResult> DetalleContrato(Guid id)
    {
        Contrato model = await _dbContext.Contratos
            .Include(c => c.Area)
            .Include(c => c.Cliente)
            .Include(c => c.Proyectos)
            .ThenInclude(p => p.Area)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> AgregarContrato()
    {
        IEnumerable<Cliente> clientes = await _dbContext.Clientes.ToListAsync();
        ViewBag.Clientes = clientes.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

        IEnumerable<Area> areas = await _dbContext.Areas.ToListAsync();
        ViewBag.Areas = areas.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarContrato(Contrato model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Contrato)), GetModelStateErrors()));

            IEnumerable<Cliente> clientes = await _dbContext.Clientes.ToListAsync();
            ViewBag.Clientes = clientes.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

            IEnumerable<Area> areas = await _dbContext.Areas.ToListAsync();
            ViewBag.Areas = areas.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

            return View(model);
        }

        model.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
        await _dbContext.Contratos.AddAsync(model);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Contratos));

        ModelState.AddModelError("", Utils.MensajeErrorAgregar(nameof(Contrato)));
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditarContrato(Guid id)
    {
        Contrato model = await _dbContext.Contratos.FindAsync(id);

        if (model == null) return NotFound();

        IEnumerable<Cliente> clientes = await _dbContext.Clientes.ToListAsync();
        ViewBag.Clientes = clientes.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

        IEnumerable<Area> areas = await _dbContext.Areas.ToListAsync();
        ViewBag.Areas = areas.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditarContrato(Contrato model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorActualizar(nameof(Contrato)), GetModelStateErrors()));

            IEnumerable<Cliente> clientes = await _dbContext.Clientes.ToListAsync();
            ViewBag.Clientes = clientes.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

            IEnumerable<Area> areas = await _dbContext.Areas.ToListAsync();
            ViewBag.Areas = areas.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

            return View(model);
        }

        Contrato contrato = await _dbContext.Contratos.FindAsync(model.Id);

        if (contrato == null) return NotFound();

        contrato.Actualizar(model, GetCurrentUser());
        _dbContext.Contratos.Update(contrato);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Contratos));
        ModelState.AddModelError("", Utils.MensajeErrorActualizar(nameof(Contrato)));

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EliminarContrato(Guid id)
    {
        Contrato model =
            await _dbContext.Contratos.Include(c => c.Cliente).Where(c => c.Id == id).FirstOrDefaultAsync();

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarContrato(Contrato model)
    {
        Contrato contrato = await _dbContext.Contratos.FindAsync(model.Id);

        if (contrato == null) return NotFound();

        contrato.Eliminar(GetCurrentUser());
        _dbContext.Contratos.Update(contrato);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Contratos));

        ModelState.AddModelError("", Utils.MensajeErrorEliminar(nameof(Contrato)));
        return View(model);
    }

    #endregion

    #region Proyectos

    [HttpGet]
    public async Task<IActionResult> Proyectos(string palabraClave)
    {
        IEnumerable<Proyecto> proyectos = await _dbContext.Proyectos
            .Where(c => palabraClave == null || (c.Nombre.ToLower().Contains(palabraClave.ToLower())) ||
                        (c.Contrato.Cliente.Nombre.ToLower().Contains(palabraClave.ToLower())) ||
                        (c.Area.Nombre.ToLower().Contains(palabraClave.ToLower())))
            .OrderBy(p => p.Nombre)
            .Include(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Include(p => p.Area)
            .ToListAsync();

        IndexProyectosViewModel model = new() { PalabraClave = palabraClave, Proyectos = proyectos };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DetalleProyecto(Guid id)
    {
        Proyecto model = await _dbContext.Proyectos
            .Include(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Include(p => p.Area)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> AgregarProyecto()
    {
        IEnumerable<Contrato> contratos = await _dbContext.Contratos.Include(c => c.Cliente).ToListAsync();
        ViewBag.Contratos = contratos.Select(c =>
            new SelectListItem(text: $"{c.Cliente.Nombre}", c.Id.ToString()));

        IEnumerable<Area> areas = await _dbContext.Areas.ToListAsync();
        ViewBag.Areas = areas.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarProyecto(Proyecto model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Proyecto)), GetModelStateErrors()));

            IEnumerable<Contrato> contratos = await _dbContext.Contratos.Include(c => c.Cliente).ToListAsync();
            ViewBag.Contratos = contratos.Select(c =>
                new SelectListItem(text: $"{c.Cliente.Nombre} - {c.Identificacion}", c.Id.ToString()));

            IEnumerable<Area> areas = await _dbContext.Areas.ToListAsync();
            ViewBag.Areas = areas.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

            return View(model);
        }

        model.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
        await _dbContext.Proyectos.AddAsync(model);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Proyectos));

        ModelState.AddModelError("", Utils.MensajeErrorAgregar(nameof(Proyecto)));
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditarProyecto(Guid id)
    {
        Proyecto model = await _dbContext.Proyectos.FindAsync(id);

        if (model == null) return NotFound();

        IEnumerable<Contrato> contratos = await _dbContext.Contratos.Include(c => c.Cliente).ToListAsync();
        ViewBag.Contratos = contratos.Select(c =>
            new SelectListItem(text: $"{c.Cliente.Nombre}", c.Id.ToString()));

        IEnumerable<Area> areas = await _dbContext.Areas.ToListAsync();
        ViewBag.Areas = areas.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditarProyecto(Proyecto model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorActualizar(nameof(Proyecto)), GetModelStateErrors()));

            IEnumerable<Contrato> contratos = await _dbContext.Contratos.Include(c => c.Cliente).ToListAsync();
            ViewBag.Contratos = contratos.Select(c =>
                new SelectListItem(text: $"{c.Cliente.Nombre} - {c.Identificacion}", c.Id.ToString()));

            IEnumerable<Area> areas = await _dbContext.Areas.ToListAsync();
            ViewBag.Areas = areas.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

            return View(model);
        }

        Proyecto proyecto = await _dbContext.Proyectos.FindAsync(model.Id);

        if (proyecto == null) return NotFound();

        proyecto.Actualizar(model, GetCurrentUser());
        _dbContext.Proyectos.Update(proyecto);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Proyectos));
        ModelState.AddModelError("", Utils.MensajeErrorActualizar(nameof(Proyecto)));

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EliminarProyecto(Guid id)
    {
        Proyecto model = await _dbContext.Proyectos
            .Include(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarProyecto(Proyecto model)
    {
        Proyecto proyecto = await _dbContext.Proyectos.FindAsync(model.Id);

        if (proyecto == null) return NotFound();

        proyecto.Eliminar(GetCurrentUser());
        _dbContext.Proyectos.Update(proyecto);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Proyectos));

        ModelState.AddModelError("", Utils.MensajeErrorEliminar(nameof(Proyecto)));
        return View(model);
    }

    #endregion

    #region Asignaciones

    [HttpGet]
    public async Task<IActionResult> Asignaciones()
    {
        var colaboradores = _dbContext.Usuarios.OrderBy(u => u.Name).ToList();
        ViewBag.Colaboradores = colaboradores.Select(c => new SelectListItem(text: c.FullName, c.Id));

        var proyectos = _dbContext.Proyectos.Include(p => p.Contrato).ThenInclude(c => c.Cliente)
            .OrderBy(c => c.Contrato.Cliente.Nombre).ToList();

        ViewBag.Proyectos = proyectos.Select(c =>
            new SelectListItem(text: $"{c.Contrato.Cliente.Nombre} - {c.Nombre}", c.Id.ToString()));

        var asignaciones = await _dbContext.Asignaciones
            .Include(a => a.ApplicationUser)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .ToListAsync();

        var viewModel = new AsignacionesIndexViewModel
        {
            ProyectosAsignaciones = asignaciones
                .GroupBy(a => a.IdProyecto)
                .Select(group => new ProyectoAsignacionesViewModel
                {
                    IdProyecto = group.Key,
                    NombreProyecto = group.First().Proyecto.Nombre,
                    NombreCliente = group.First().Proyecto.Contrato.Cliente.Nombre,
                    Asignaciones = group.ToList()
                })
                .OrderBy(p => p.NombreProyecto)
                .ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Asignaciones(AsignacionesIndexViewModel model)
    {
        var colaboradores = _dbContext.Usuarios.OrderBy(u => u.Name).ToList();
        ViewBag.Colaboradores = colaboradores.Select(c => new SelectListItem(text: c.FullName, c.Id));

        var proyectos = _dbContext.Proyectos.Include(p => p.Contrato).ThenInclude(c => c.Cliente)
            .OrderBy(c => c.Contrato.Cliente.Nombre).ToList();
        ViewBag.Proyectos = proyectos.Select(c =>
            new SelectListItem(text: $"{c.Contrato.Cliente.Nombre} - {c.Nombre}", c.Id.ToString()));

        var asignaciones = await _dbContext.Asignaciones
            .Where(a => (model.IdUsuario == null || a.IdColaborador == model.IdUsuario) &&
                        (model.IdProyecto == null || a.IdProyecto.ToString() == model.IdProyecto))
            .Include(a => a.ApplicationUser)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .ToListAsync();

        var viewModel = new AsignacionesIndexViewModel
        {
            IdUsuario = model.IdUsuario,
            ProyectosAsignaciones = asignaciones
                .GroupBy(a => a.IdProyecto)
                .Select(group => new ProyectoAsignacionesViewModel
                {
                    IdProyecto = group.Key,
                    NombreProyecto = group.First().Proyecto.Nombre,
                    NombreCliente = group.First().Proyecto.Contrato.Cliente.Nombre,
                    Asignaciones = group.ToList()
                })
                .OrderBy(p => p.NombreProyecto)
                .ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarAsignaciones(
        string idUsuario = null,
        string idProyecto = null)
    {
        var asignaciones = await _dbContext.Asignaciones
            .Where(a => (idUsuario == null || a.IdColaborador == idUsuario) &&
                        (idProyecto == null || a.IdProyecto.ToString() == idProyecto))
            .Include(a => a.ApplicationUser)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .OrderBy(a => a.Proyecto.Contrato.Cliente.Nombre)
            .ThenBy(a => a.Proyecto.Nombre)
            .ToListAsync();

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Asignaciones");

        // Headers
        worksheet.Cell(1, 1).Value = "Cliente";
        worksheet.Cell(1, 2).Value = "Proyecto";
        worksheet.Cell(1, 3).Value = "Colaborador";
        worksheet.Cell(1, 4).Value = "Horas Estimadas";
        worksheet.Cell(1, 5).Value = "Descripción";

        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 5);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#1e3a5f");
        headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;

        // Data
        int row = 2;
        foreach (var asignacion in asignaciones)
        {
            worksheet.Cell(row, 1).Value = asignacion.Proyecto?.Contrato?.Cliente?.Nombre;
            worksheet.Cell(row, 2).Value = asignacion.Proyecto?.Nombre;
            worksheet.Cell(row, 3).Value = asignacion.ApplicationUser?.FullName;
            worksheet.Cell(row, 4).Value = asignacion.HorasEstimadas;
            worksheet.Cell(row, 5).Value = asignacion.Descripcion;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"Asignaciones_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> MisAsignaciones()
    {
        ApplicationUser usuario = await _userManager.FindByEmailAsync(GetCurrentUser());

        var asignaciones = await _dbContext.Asignaciones
            .Include(a => a.ApplicationUser)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Where(a => a.IdColaborador == usuario.Id)
            .ToListAsync();

        var viewModel = new AsignacionesIndexViewModel
        {
            ProyectosAsignaciones = asignaciones
                .GroupBy(a => a.IdProyecto)
                .Select(group => new ProyectoAsignacionesViewModel
                {
                    IdProyecto = group.Key,
                    NombreProyecto = group.First().Proyecto.Nombre,
                    NombreCliente = group.First().Proyecto.Contrato.Cliente.Nombre,
                    Asignaciones = group.ToList()
                })
                .OrderBy(p => p.NombreProyecto)
                .ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> AsignarProyecto(Guid id)
    {
        ApplicationUser colaborador = await _dbContext.Usuarios.FindAsync(id.ToString());
        AgregarAsignacionModel model = new AgregarAsignacionModel
        {
            NombreColaborador = colaborador.FullName,
            IdUsuario = id
        };

        IEnumerable<Proyecto> proyectos =
            await _dbContext.Proyectos.Include(c => c.Contrato).ThenInclude(c => c.Cliente).ToListAsync();
        ViewBag.Proyectos = proyectos.Select(c =>
            new SelectListItem(text: $"{c.Contrato.Cliente.Nombre} - {c.Nombre}", c.Id.ToString()));

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarProyecto(AgregarAsignacionModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Asignacion)), GetModelStateErrors()));

            IEnumerable<Proyecto> proyectos = await _dbContext.Proyectos.Include(c => c.Contrato)
                .ThenInclude(c => c.Cliente).ToListAsync();
            ViewBag.Proyectos = proyectos.Select(c =>
                new SelectListItem(text: $"{c.Contrato.Cliente.Nombre} - {c.Nombre}", c.Id.ToString()));

            return View(model);
        }

        // Validar que no exista una asignación activa para este proyecto y colaborador
        // Nota: El QueryFilter automáticamente filtra IsDeleted = false
        var asignacionExistente = await _dbContext.Asignaciones
            .Include(a => a.Proyecto)
                .ThenInclude(p => p.Contrato)
                .ThenInclude(c => c.Cliente)
            .FirstOrDefaultAsync(a =>
                a.IdProyecto == model.IdProyecto &&
                a.IdColaborador == model.IdUsuario.ToString());

        if (asignacionExistente != null)
        {
            ModelState.AddModelError("",
                $"El colaborador ya está asignado al proyecto '{asignacionExistente.Proyecto.Nombre}' " +
                $"del cliente '{asignacionExistente.Proyecto.Contrato.Cliente.Nombre}'. " +
                $"No se pueden crear asignaciones duplicadas.");

            IEnumerable<Proyecto> proyectos = await _dbContext.Proyectos.Include(c => c.Contrato)
                .ThenInclude(c => c.Cliente).ToListAsync();
            ViewBag.Proyectos = proyectos.Select(c =>
                new SelectListItem(text: $"{c.Contrato.Cliente.Nombre} - {c.Nombre}", c.Id.ToString()));

            return View(model);
        }

        Asignacion asignacion = new Asignacion()
        {
            IdColaborador = model.IdUsuario.ToString(),
            IdProyecto = model.IdProyecto,
            HorasEstimadas = model.HorasEstimadas,
            Descripcion = model.Descripcion
        };

        asignacion.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
        await _dbContext.Asignaciones.AddAsync(asignacion);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0)
        {
            var colaborador = await _dbContext.Usuarios.Where(u => u.Id == model.IdUsuario.ToString())
                .FirstOrDefaultAsync();
            var proyecto = await _dbContext.Proyectos.Include(p => p.Contrato).ThenInclude(c => c.Cliente)
                .Where(p => p.Id == model.IdProyecto).FirstOrDefaultAsync();

            var asignacionesColaborador = await _dbContext.Asignaciones
                .Include(a => a.Proyecto)
                .ThenInclude(p => p.Contrato)
                .ThenInclude(c => c.Cliente)
                .Where(a => a.IdColaborador == colaborador.Id)
                .OrderBy(a => a.Proyecto.Nombre)
                .ToListAsync();

            StringBuilder sbProyectos = new StringBuilder();
            sbProyectos.AppendLine("Proyectos asignados actualmente:");
            foreach (var asignacionColaborador in asignacionesColaborador)
            {
                sbProyectos.AppendLine(
                    $"{asignacionColaborador.Proyecto.Nombre} - {asignacionColaborador.Proyecto.Contrato.Cliente.Nombre}");
            }

            StringBuilder mensajeCorreo = new StringBuilder();
            mensajeCorreo.AppendLine(
                $"{colaborador.FullName}, Se le ha asignado un nuevo proyecto {proyecto.Nombre} del cliente {proyecto.Contrato.Cliente.Nombre}.");
            /*
            mensajeCorreo.AppendLine(sbProyectos.ToString());
            */
            await _emailSender.SendEmailAsync(colaborador.Email, "Asignación de proyecto", mensajeCorreo.ToString());

            return RedirectToAction(nameof(Asignaciones));
        }

        ModelState.AddModelError("", Utils.MensajeErrorAgregar(nameof(Asignacion)));
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EliminarAsignacion(Guid id)
    {
        var model = await _dbContext.Asignaciones
            .Include(a => a.ApplicationUser)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarAsignacion(Asignacion model)
    {
        Asignacion asignacion = await _dbContext.Asignaciones.FindAsync(model.Id);

        if (asignacion == null) return NotFound();

        asignacion.Eliminar(GetCurrentUser());
        _dbContext.Asignaciones.Update(asignacion);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0)
            return RedirectToAction("DetalleColaborador", "Cuentas", new { id = asignacion.IdColaborador });

        ModelState.AddModelError("", Utils.MensajeErrorEliminar(nameof(Asignacion)));
        return View(model);
    }

    #endregion

    #region Sesiones

    [HttpGet]
    public async Task<IActionResult> Sesiones(
        string idUsuario = null,
        string idProyecto = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null)
    {
        // Si no hay fechas, usar mes actual por defecto
        if (fechaInicio == null && fechaFin == null)
        {
            (fechaInicio, fechaFin) = _sesionesManager.ObtenerRangoMesActual();
        }

        ViewBag.Colaboradores = await ObtenerColaboradoresDropdown();
        ViewBag.Proyectos = await ObtenerProyectosDropdown();

        var sesiones = await _sesionesManager.ObtenerSesionesFiltradas(idUsuario, idProyecto, fechaInicio, fechaFin);

        var viewModel = new SesionesIndexViewModel
        {
            IdUsuario = idUsuario,
            IdProyecto = idProyecto,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Sesiones = sesiones
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarSesiones(
        string idUsuario = null,
        string idProyecto = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null)
    {
        var sesiones = await _sesionesManager.ObtenerSesionesFiltradas(idUsuario, idProyecto, fechaInicio, fechaFin);

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sesiones");

        // Headers
        worksheet.Cell(1, 1).Value = "Fecha";
        worksheet.Cell(1, 2).Value = "Colaborador";
        worksheet.Cell(1, 3).Value = "Cliente";
        worksheet.Cell(1, 4).Value = "Proyecto";
        worksheet.Cell(1, 5).Value = "Horas";
        worksheet.Cell(1, 6).Value = "Minutos";
        worksheet.Cell(1, 7).Value = "Detalle";

        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 7);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#1e3a5f");
        headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;

        // Data
        int row = 2;
        foreach (var sesion in sesiones)
        {
            worksheet.Cell(row, 1).Value = sesion.FechaInicio.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 2).Value = sesion.ApplicationUser?.FullName;
            worksheet.Cell(row, 3).Value = sesion.Proyecto?.Contrato?.Cliente?.Nombre;
            worksheet.Cell(row, 4).Value = sesion.Proyecto?.Nombre;
            worksheet.Cell(row, 5).Value = sesion.Horas;
            worksheet.Cell(row, 6).Value = sesion.Minutes;
            worksheet.Cell(row, 7).Value = sesion.Descripcion;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"Sesiones_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> MisSesiones(
        string idProyecto = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null)
    {
        ApplicationUser usuario = await _userManager.FindByEmailAsync(GetCurrentUser());

        ViewBag.Proyectos = await ObtenerProyectosAsignadosDropdown(usuario.Id);

        var sesiones = await _sesionesManager.ObtenerSesionesFiltradas(
            usuario.Id, idProyecto, fechaInicio, fechaFin);

        // Si no hay filtros, limitar a 25 sesiones
        if (fechaInicio == null && fechaFin == null && string.IsNullOrEmpty(idProyecto))
        {
            sesiones = sesiones.Take(25).ToList();
        }

        var viewModel = new SesionesIndexViewModel
        {
            IdProyecto = idProyecto,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Sesiones = sesiones,
            SesionesActivas = await _sesionesManager.ObtenerSesionesActivas(usuario.Id)
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> AgregarSesion()
    {
        ApplicationUser colaborador = await _userManager.FindByEmailAsync(GetCurrentUser());

        ViewBag.Servicios = await ObtenerServiciosDropdown();
        ViewBag.Proyectos = await ObtenerProyectosAsignadosDropdown(colaborador.Id);

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarSesion(AgregarSesionModel model)
    {
        ApplicationUser colaborador = await _userManager.FindByEmailAsync(GetCurrentUser());

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Sesion)), GetModelStateErrors()));

            ViewBag.Servicios = await ObtenerServiciosDropdown();
            ViewBag.Proyectos = await ObtenerProyectosAsignadosDropdown(colaborador.Id);

            return View(model);
        }

        var exito = await _sesionesManager.CrearSesionManual(model, colaborador.Id, GetCurrentUser());

        if (exito) return RedirectToAction(nameof(MisSesiones));

        ModelState.AddModelError("", Utils.MensajeErrorAgregar(nameof(Sesion)));
        return View(model);
    }

    [HttpGet]
    public ActionResult ErrorIniciarSesion()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> IniciarSesion()
    {
        ApplicationUser colaborador = await _userManager.FindByEmailAsync(GetCurrentUser());

        // Validar que no tenga más de 1 sesión activa
        var sesionesActivas = await _sesionesManager.ContarSesionesActivas(colaborador.Id);
        if (sesionesActivas > 1)
        {
            return RedirectToAction(nameof(ErrorIniciarSesion));
        }

        ViewBag.Servicios = await ObtenerServiciosDropdown();
        ViewBag.Proyectos = await ObtenerProyectosAsignadosDropdown(colaborador.Id);

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarSesion(AgregarSesionModel model)
    {
        ApplicationUser colaborador = await _userManager.FindByEmailAsync(GetCurrentUser());

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Sesion)), GetModelStateErrors()));

            ViewBag.Servicios = await ObtenerServiciosDropdown();
            ViewBag.Proyectos = await ObtenerProyectosAsignadosDropdown(colaborador.Id);

            return View(model);
        }

        var (exito, error) = await _sesionesManager.IniciarSesion(model, colaborador.Id, GetCurrentUser());

        if (exito) return RedirectToAction(nameof(MisSesiones));

        ModelState.AddModelError("", error ?? Utils.MensajeErrorAgregar(nameof(Sesion)));
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> PausarSesion(Guid id)
    {
        var sesion = await _sesionesManager.ObtenerSesionPorId(id);

        if (sesion == null) return NotFound();

        var model = new PausarSesionModel
        {
            IdSesion = sesion.Id,
            IdProyecto = sesion.IdProyecto,
            NombreProyecto = sesion.Proyecto.Nombre,
            IdServicio = sesion.IdServicio,
            NombreServicio = sesion.Servicio.Nombre,
            Descripcion = sesion.Descripcion,
            Horas = sesion.Horas
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PausarSesion(PausarSesionModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Sesion)), GetModelStateErrors()));
            return View(model);
        }

        var (exito, error) = await _sesionesManager.PausarSesion(model.IdSesion, model.Descripcion, GetCurrentUser());

        if (exito) return RedirectToAction(nameof(MisSesiones));

        ModelState.AddModelError("", error ?? Utils.MensajeErrorAgregar(nameof(Sesion)));
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ReanudarSesion(Guid id)
    {
        var sesion = await _sesionesManager.ObtenerSesionPorId(id);

        if (sesion == null) return NotFound();

        var model = new PausarSesionModel
        {
            IdSesion = sesion.Id,
            IdProyecto = sesion.IdProyecto,
            NombreProyecto = sesion.Proyecto.Nombre,
            IdServicio = sesion.IdServicio,
            NombreServicio = sesion.Servicio.Nombre,
            Descripcion = sesion.Descripcion,
            Horas = sesion.Horas
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReanudarSesion(PausarSesionModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Sesion)), GetModelStateErrors()));
            return View(model);
        }

        var (exito, error) = await _sesionesManager.ReanudarSesion(model.IdSesion, model.Descripcion, GetCurrentUser());

        if (exito) return RedirectToAction(nameof(MisSesiones));

        ModelState.AddModelError("", error ?? Utils.MensajeErrorAgregar(nameof(Sesion)));
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> FinalizarSesion(Guid id)
    {
        var sesion = await _sesionesManager.ObtenerSesionPorId(id);

        if (sesion == null) return NotFound();

        var model = new FinalizarSesionModel
        {
            IdSesion = sesion.Id,
            IdProyecto = sesion.IdProyecto,
            NombreProyecto = sesion.Proyecto.Nombre,
            IdServicio = sesion.IdServicio,
            NombreServicio = sesion.Servicio.Nombre,
            Descripcion = sesion.Descripcion
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizarSesion(FinalizarSesionModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Sesion)), GetModelStateErrors()));
            return View(model);
        }

        var (exito, error) = await _sesionesManager.FinalizarSesion(model.IdSesion, model.Descripcion, GetCurrentUser());

        if (exito) return RedirectToAction(nameof(MisSesiones));

        ModelState.AddModelError("", error ?? Utils.MensajeErrorAgregar(nameof(Sesion)));
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DetalleSesion(Guid id)
    {
        var model = await _sesionesManager.ObtenerSesionPorId(id);

        if (model == null) return NotFound();

        return View(model);
    }

    #endregion
}