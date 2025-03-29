using System.ComponentModel;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plani.Identity;
using plani.Models;
using plani.Models.Data;

namespace plani.Controllers;

[Authorize]
public class ClientesController : BaseController
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ClientesController> _logger;
    private readonly ApplicationUserManager _userManager;

    public ClientesController(
        ApplicationDbContext dbContext,
        ApplicationUserManager userManager,
        ApplicationRoleManager roleManager,
        IConfiguration configuration,
        ILogger<ClientesController> logger,
        IHttpContextAccessor contextAccesor,
        IWebHostEnvironment environment)
        : base(userManager, roleManager, configuration, contextAccesor, environment)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Construccion()
    {
        return View();
    }

    #region Tipos de cliente

    [HttpGet]
    public async Task<IActionResult> TiposCliente()
    {
        List<TipoCliente> model = await _dbContext.TiposCliente.ToListAsync();
        return View(model);
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

    [HttpGet]
    public IActionResult AgregarTipoCliente() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarTipoCliente(TipoCliente model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(TipoCliente)), GetModelStateErrors()));
            return View(model);
        }

        model.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
        await _dbContext.TiposCliente.AddAsync(model);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(TiposCliente));
        ModelState.AddModelError("", Utils.MensajeErrorAgregar(nameof(TipoCliente)));

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditarTipoCliente(Guid id)
    {
        var model = await _dbContext.TiposCliente.FindAsync(id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditarTipoCliente(TipoCliente model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorActualizar(nameof(TipoCliente)), GetModelStateErrors()));
            return View(model);
        }

        TipoCliente tipoCliente = await _dbContext.TiposCliente.FindAsync(model.Id);

        if (tipoCliente == null) return NotFound();

        tipoCliente.Actualizar(model, GetCurrentUser());
        _dbContext.TiposCliente.Update(tipoCliente);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(TiposCliente));
        ModelState.AddModelError("", Utils.MensajeErrorActualizar(nameof(TipoCliente)));

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EliminarTipoCliente(Guid id)
    {
        TipoCliente model = await _dbContext.TiposCliente.FindAsync(id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarTipoCliente(TipoCliente model)
    {
        TipoCliente tipoCliente = await _dbContext.TiposCliente.FindAsync(model.Id);

        if (tipoCliente == null) return NotFound();

        tipoCliente.Eliminar(GetCurrentUser());
        _dbContext.TiposCliente.Update(tipoCliente);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(TiposCliente));

        ModelState.AddModelError("", Utils.MensajeErrorEliminar(nameof(TipoCliente)));
        return View(model);
    }

    #endregion

    #region Clientes

    [HttpGet]
    public async Task<IActionResult> Clientes()
    {
        IEnumerable<Cliente> model = await _dbContext.Clientes.Include(c => c.TipoCliente).ToListAsync();
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

        model.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
        await _dbContext.Clientes.AddAsync(model);
        
        // Add new Contrato for that Cliente
        Contrato contrato = new Contrato
        {
            IdCliente = model.Id,
            Identificacion = "CONTRATO-" + model.Id,
            Descripcion = "Contrato de " + model.Nombre,
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
    public async Task<IActionResult> Proyectos()
    {
        IEnumerable<Proyecto> proyectos = await _dbContext.Proyectos
            .Include(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Include(p => p.Area)
            .ToListAsync();

        return View(proyectos);
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
        var colaboradores = _dbContext.Usuarios.ToList();
        ViewBag.Colaboradores = colaboradores.Select(c => new SelectListItem(text: c.FullName, c.Id));

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
        var colaboradores = _dbContext.Usuarios.ToList();
        ViewBag.Colaboradores = colaboradores.Select(c => new SelectListItem(text: c.FullName, c.Id));

        var asignaciones = await _dbContext.Asignaciones
            .Include(a => a.ApplicationUser)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Where(a => model.IdUsuario == null || a.IdColaborador == model.IdUsuario)
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

        if (changes > 0) return RedirectToAction(nameof(Asignaciones));

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
    public async Task<IActionResult> Sesiones()
    {
        var colaboradores = _dbContext.Usuarios.ToList();
        ViewBag.Colaboradores = colaboradores.Select(c => new SelectListItem(text: c.FullName, c.Id));

        var sesiones = await _dbContext.Sesiones
            .Include(a => a.ApplicationUser)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .ToListAsync();

        var viewModel = new SesionesIndexViewModel
        {
            ProyectosSesiones = sesiones
                .GroupBy(a => a.IdProyecto)
                .Select(group => new ProyectoSesionesViewModel()
                {
                    IdProyecto = group.Key,
                    NombreProyecto = group.First().Proyecto.Nombre,
                    NombreCliente = group.First().Proyecto.Contrato.Cliente.Nombre,
                    Sesiones = group.ToList()
                })
                .OrderBy(p => p.NombreProyecto)
                .ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Sesiones(SesionesIndexViewModel model)
    {
        var colaboradores = _dbContext.Usuarios.ToList();
        ViewBag.Colaboradores = colaboradores.Select(c => new SelectListItem(text: c.FullName, c.Id));

        var sesiones = await _dbContext.Sesiones
            .Include(a => a.ApplicationUser)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Where(s => model.IdUsuario == null || s.IdColaborador == model.IdUsuario)
            .ToListAsync();

        var viewModel = new SesionesIndexViewModel
        {
            IdUsuario = model.IdUsuario,
            ProyectosSesiones = sesiones
                .GroupBy(a => a.IdProyecto)
                .Select(group => new ProyectoSesionesViewModel()
                {
                    IdProyecto = group.Key,
                    NombreProyecto = group.First().Proyecto.Nombre,
                    NombreCliente = group.First().Proyecto.Contrato.Cliente.Nombre,
                    Sesiones = group.ToList()
                })
                .OrderBy(p => p.NombreProyecto)
                .ToList()
        };

        return View(viewModel);
    }
    
    // [HttpPost]
    // public async Task<IActionResult> ExportarSesiones(string id)
    // {
    //     var colaboradores = _dbContext.Usuarios.ToList();
    //     ViewBag.Colaboradores = colaboradores.Select(c => new SelectListItem(text: c.FullName, c.Id));
    //
    //     var sesiones = await _dbContext.Sesiones
    //         .Include(a => a.ApplicationUser)
    //         .Include(a => a.Proyecto)
    //         .ThenInclude(p => p.Contrato)
    //         .ThenInclude(c => c.Cliente)
    //         .Where(s => model.IdUsuario == null || s.IdColaborador == model.IdUsuario)
    //         .ToListAsync();
    //
    //     var viewModel = new SesionesIndexViewModel
    //     {
    //         IdUsuario = model.IdUsuario,
    //         ProyectosSesiones = sesiones
    //             .GroupBy(a => a.IdProyecto)
    //             .Select(group => new ProyectoSesionesViewModel()
    //             {
    //                 IdProyecto = group.Key,
    //                 NombreProyecto = group.First().Proyecto.Nombre,
    //                 NombreCliente = group.First().Proyecto.Contrato.Cliente.Nombre,
    //                 Sesiones = group.ToList()
    //             })
    //             .OrderBy(p => p.NombreProyecto)
    //             .ToList()
    //     };
    //
    //     return View(viewModel);
    // }

    [HttpGet]
    public async Task<IActionResult> MisSesiones()
    {
        ApplicationUser usuario = await _userManager.FindByEmailAsync(GetCurrentUser());

        var sesiones = await _dbContext.Sesiones
            .Include(a => a.ApplicationUser)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Where(s => s.IdColaborador == usuario.Id)
            .ToListAsync();

        var viewModel = new SesionesIndexViewModel
        {
            ProyectosSesiones = sesiones
                .GroupBy(a => a.IdProyecto)
                .Select(group => new ProyectoSesionesViewModel()
                {
                    IdProyecto = group.Key,
                    NombreProyecto = group.First().Proyecto.Nombre,
                    NombreCliente = group.First().Proyecto.Contrato.Cliente.Nombre,
                    Sesiones = group.ToList()
                })
                .OrderBy(p => p.NombreProyecto)
                .ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> AgregarSesion()
    {
        var currentUser = GetCurrentUser();
        ApplicationUser colaborador = await _userManager.FindByEmailAsync(currentUser);

        IEnumerable<Servicio> servicios = await _dbContext.Servicios.ToListAsync();

        ViewBag.Servicios = servicios.Select(c => new SelectListItem(text: c.Nombre, c.Id.ToString()));

        IEnumerable<Asignacion> asignaciones =
            await _dbContext.Asignaciones.Include(a => a.Proyecto).ThenInclude(c => c.Contrato)
                .ThenInclude(c => c.Cliente).Where(a => a.IdColaborador == colaborador.Id).ToListAsync();

        ViewBag.Proyectos = asignaciones.Select(c =>
            new SelectListItem(text: $"{c.Proyecto.Contrato.Cliente.Nombre} - {c.Proyecto.Nombre}",
                c.IdProyecto.ToString()));

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarSesion(AgregarSesionModel model)
    {
        var currentUser = GetCurrentUser();
        ApplicationUser colaborador = await _userManager.FindByEmailAsync(currentUser);

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Sesion)), GetModelStateErrors()));

            IEnumerable<Servicio> servicios = await _dbContext.Servicios.ToListAsync();

            ViewBag.Servicios = servicios.Select(c => new SelectListItem(text: c.Nombre, c.Id.ToString()));

            IEnumerable<Asignacion> asignaciones =
                await _dbContext.Asignaciones.Include(a => a.Proyecto).ThenInclude(c => c.Contrato)
                    .ThenInclude(c => c.Cliente).Where(a => a.IdColaborador == colaborador.Id).ToListAsync();
            ViewBag.Proyectos = asignaciones.Select(c =>
                new SelectListItem(text: $"{c.Proyecto.Contrato.Cliente.Nombre} - {c.Proyecto.Nombre}",
                    c.IdProyecto.ToString()));

            return View(model);
        }

        Sesion sesion = new Sesion()
        {
            IdColaborador = colaborador.Id,
            IdProyecto = model.IdProyecto,
            IdServicio = model.IdServicio,
            FechaInicio = model.Fecha,
            FechaFin = model.Fecha,
            Horas = float.Parse(model.Horas, CultureInfo.InvariantCulture),
            Descripcion = model.Descripcion
        };

        sesion.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
        await _dbContext.Sesiones.AddAsync(sesion);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(MisSesiones));

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
        var currentUser = GetCurrentUser();
        ApplicationUser colaborador = await _userManager.FindByEmailAsync(currentUser);
        
        // Get user's active sesiones
        var activeSesiones = await _dbContext.Sesiones
            .Where(s => s.IdColaborador == colaborador.Id && s.FechaFin == null)
            .ToListAsync();
        
        if (activeSesiones.Count > 1)
        {
            ModelState.AddModelError("", "No puede iniciar una nueva sesión si tienes dos sesiones activa.");
            return RedirectToAction(nameof(ErrorIniciarSesion));
        }

        IEnumerable<Servicio> servicios = await _dbContext.Servicios.ToListAsync();

        ViewBag.Servicios = servicios.Select(c => new SelectListItem(text: c.Nombre, c.Id.ToString()));

        IEnumerable<Asignacion> asignaciones =
            await _dbContext.Asignaciones.Include(a => a.Proyecto).ThenInclude(c => c.Contrato)
                .ThenInclude(c => c.Cliente).Where(a => a.IdColaborador == colaborador.Id).ToListAsync();

        ViewBag.Proyectos = asignaciones.Select(c =>
            new SelectListItem(text: $"{c.Proyecto.Contrato.Cliente.Nombre} - {c.Proyecto.Nombre}",
                c.IdProyecto.ToString()));

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarSesion(AgregarSesionModel model)
    {
        var currentUser = GetCurrentUser();
        ApplicationUser colaborador = await _userManager.FindByEmailAsync(currentUser);

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Utils.MensajeErrorAgregar(nameof(Sesion)), GetModelStateErrors()));

            IEnumerable<Servicio> servicios = await _dbContext.Servicios.ToListAsync();

            ViewBag.Servicios = servicios.Select(c => new SelectListItem(text: c.Nombre, c.Id.ToString()));

            IEnumerable<Asignacion> asignaciones =
                await _dbContext.Asignaciones.Include(a => a.Proyecto).ThenInclude(c => c.Contrato)
                    .ThenInclude(c => c.Cliente).Where(a => a.IdColaborador == colaborador.Id).ToListAsync();
            ViewBag.Proyectos = asignaciones.Select(c =>
                new SelectListItem(text: $"{c.Proyecto.Contrato.Cliente.Nombre} - {c.Proyecto.Nombre}",
                    c.IdProyecto.ToString()));

            return View(model);
        }

        Sesion sesion = new Sesion()
        {
            IdColaborador = colaborador.Id,
            IdProyecto = model.IdProyecto,
            IdServicio = model.IdServicio,
            FechaInicio = DateTime.Now,
            Horas = 0,
            Descripcion = model.Descripcion
        };

        sesion.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
        await _dbContext.Sesiones.AddAsync(sesion);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(MisSesiones));

        ModelState.AddModelError("", Utils.MensajeErrorAgregar(nameof(Sesion)));
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> PausarSesion(Guid id)
    {
        Sesion sesion = await _dbContext.Sesiones.FindAsync(id);

        if (sesion == null) return NotFound();

        Proyecto proyecto = await _dbContext.Proyectos.FindAsync(sesion.IdProyecto);

        Servicio servicio = await _dbContext.Servicios.FindAsync(sesion.IdServicio);

        PausarSesionModel model = new PausarSesionModel
        {
            IdSesion = sesion.Id,
            IdProyecto = sesion.IdProyecto,
            NombreProyecto = proyecto.Nombre,
            IdServicio = sesion.IdServicio,
            NombreServicio = servicio.Nombre,
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

            IEnumerable<Servicio> servicios = await _dbContext.Servicios.ToListAsync();

            ViewBag.Servicios = servicios.Select(c => new SelectListItem(text: c.Nombre, c.Id.ToString()));

            IEnumerable<Asignacion> asignaciones =
                await _dbContext.Asignaciones.Include(a => a.Proyecto).ThenInclude(c => c.Contrato)
                    .ThenInclude(c => c.Cliente).ToListAsync();
            ViewBag.Proyectos = asignaciones.Select(c =>
                new SelectListItem(text: $"{c.Proyecto.Contrato.Cliente.Nombre} - {c.Proyecto.Nombre}",
                    c.IdProyecto.ToString()));

            return View(model);
        }

        Sesion sesion = await _dbContext.Sesiones.FindAsync(model.IdSesion);

        if (sesion == null) return NotFound();

        sesion.FechaPausa = DateTime.Now;

        double horas = (sesion.FechaPausa - sesion.FechaInicio).Value.Hours + 1;

        double roundedHours = Math.Round(horas * 2, MidpointRounding.AwayFromZero) / 2;

        sesion.Horas += roundedHours;
        sesion.Descripcion = model.Descripcion;

        _dbContext.Sesiones.Update(sesion);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(MisSesiones));

        ModelState.AddModelError("", Utils.MensajeErrorAgregar(nameof(Sesion)));
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> FinalizarSesion(Guid id)
    {
        Sesion sesion = await _dbContext.Sesiones.FindAsync(id);

        if (sesion == null) return NotFound();

        Proyecto proyecto = await _dbContext.Proyectos.FindAsync(sesion.IdProyecto);

        Servicio servicio = await _dbContext.Servicios.FindAsync(sesion.IdServicio);

        FinalizarSesionModel model = new FinalizarSesionModel
        {
            IdSesion = sesion.Id,
            IdProyecto = sesion.IdProyecto,
            NombreProyecto = proyecto.Nombre,
            IdServicio = sesion.IdServicio,
            NombreServicio = servicio.Nombre,
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

            IEnumerable<Servicio> servicios = await _dbContext.Servicios.ToListAsync();

            ViewBag.Servicios = servicios.Select(c => new SelectListItem(text: c.Nombre, c.Id.ToString()));

            IEnumerable<Asignacion> asignaciones =
                await _dbContext.Asignaciones.Include(a => a.Proyecto).ThenInclude(c => c.Contrato)
                    .ThenInclude(c => c.Cliente).ToListAsync();
            ViewBag.Proyectos = asignaciones.Select(c =>
                new SelectListItem(text: $"{c.Proyecto.Contrato.Cliente.Nombre} - {c.Proyecto.Nombre}",
                    c.IdProyecto.ToString()));

            return View(model);
        }

        Sesion sesion = await _dbContext.Sesiones.FindAsync(model.IdSesion);

        if (sesion == null) return NotFound();

        sesion.FechaFin = DateTime.Now;

        // sesion.Horas = (sesion.FechaInicio - DateTime.Now).Hours + 1;

        double horas = (sesion.FechaFin - sesion.FechaInicio).Value.Hours + 1;

        double roundedHours = Math.Round(horas * 2, MidpointRounding.AwayFromZero) / 2;

        sesion.Horas += roundedHours;
        sesion.Descripcion = model.Descripcion;

        _dbContext.Sesiones.Update(sesion);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(MisSesiones));

        ModelState.AddModelError("", Utils.MensajeErrorAgregar(nameof(Sesion)));
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ReanudarSesion(Guid id)
    {
        Sesion sesion = await _dbContext.Sesiones.FindAsync(id);

        if (sesion == null) return NotFound();

        Proyecto proyecto = await _dbContext.Proyectos.FindAsync(sesion.IdProyecto);

        Servicio servicio = await _dbContext.Servicios.FindAsync(sesion.IdServicio);

        PausarSesionModel model = new PausarSesionModel
        {
            IdSesion = sesion.Id,
            IdProyecto = sesion.IdProyecto,
            NombreProyecto = proyecto.Nombre,
            IdServicio = sesion.IdServicio,
            NombreServicio = servicio.Nombre,
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

            IEnumerable<Servicio> servicios = await _dbContext.Servicios.ToListAsync();

            ViewBag.Servicios = servicios.Select(c => new SelectListItem(text: c.Nombre, c.Id.ToString()));

            IEnumerable<Asignacion> asignaciones =
                await _dbContext.Asignaciones.Include(a => a.Proyecto).ThenInclude(c => c.Contrato)
                    .ThenInclude(c => c.Cliente).ToListAsync();
            ViewBag.Proyectos = asignaciones.Select(c =>
                new SelectListItem(text: $"{c.Proyecto.Contrato.Cliente.Nombre} - {c.Proyecto.Nombre}",
                    c.IdProyecto.ToString()));

            return View(model);
        }

        Sesion sesion = await _dbContext.Sesiones.FindAsync(model.IdSesion);

        if (sesion == null) return NotFound();

        sesion.FechaInicio = DateTime.Now;
        sesion.FechaPausa = null;
        sesion.Descripcion = model.Descripcion;

        _dbContext.Sesiones.Update(sesion);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(MisSesiones));

        ModelState.AddModelError("", Utils.MensajeErrorAgregar(nameof(Sesion)));
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DetalleSesion(Guid id)
    {
        Sesion model = await _dbContext.Sesiones
            .Include(s => s.Servicio)
            .Include(s => s.Proyecto)
            .ThenInclude(c => c.Contrato)
            .ThenInclude(c => c.Cliente)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (model == null) return NotFound();

        return View(model);
    }

    #endregion
}

// ViewModels/ProyectoAsignacionesViewModel.cs
public class ProyectoAsignacionesViewModel
{
    public Guid IdProyecto { get; set; }
    public string NombreProyecto { get; set; }
    public string NombreCliente { get; set; }
    public List<Asignacion> Asignaciones { get; set; }

    // Summary properties for the group
    public int TotalHorasEstimadas => Asignaciones?.Sum(a => a.HorasEstimadas) ?? 0;
    public int CantidadAsignaciones => Asignaciones?.Count ?? 0;
}

// ViewModels/AsignacionesIndexViewModel.cs
public class AsignacionesIndexViewModel
{
    public List<ProyectoAsignacionesViewModel> ProyectosAsignaciones { get; set; }
    public int TotalAsignaciones => ProyectosAsignaciones?.Sum(p => p.CantidadAsignaciones) ?? 0;
    public int TotalHorasEstimadas => ProyectosAsignaciones?.Sum(p => p.TotalHorasEstimadas) ?? 0;

    [DisplayName("Colaborador")] public string IdUsuario { get; set; }
}

public class AgregarAsignacionModel
{
    public string NombreColaborador { get; set; }
    public Guid IdProyecto { get; set; }
    public Guid IdUsuario { get; set; }
    public int HorasEstimadas { get; set; }
    public string Descripcion { get; set; }
}

// ViewModels/ProyectoAsignacionesViewModel.cs
public class ProyectoSesionesViewModel
{
    public Guid IdProyecto { get; set; }
    public string NombreProyecto { get; set; }
    public string NombreCliente { get; set; }
    public List<Sesion> Sesiones { get; set; }

    // Summary properties for the group
    public double TotalHoras => Sesiones?.Sum(a => a.Horas) ?? 0;
    public int CantidadSesiones => Sesiones?.Count ?? 0;
}

// ViewModels/AsignacionesIndexViewModel.cs
public class SesionesIndexViewModel
{
    public List<ProyectoSesionesViewModel> ProyectosSesiones { get; set; }
    public int TotalSesiones => ProyectosSesiones?.Sum(p => p.CantidadSesiones) ?? 0;
    public double TotalHoras => ProyectosSesiones?.Sum(p => p.TotalHoras) ?? 0;

    [DisplayName("Colaborador")] public string IdUsuario { get; set; }
}

public class AgregarSesionModel
{
    public string NombreColaborador { get; set; }
    public Guid IdProyecto { get; set; }
    public Guid IdUsuario { get; set; }
    public Guid IdServicio { get; set; }
    public DateTime Fecha { get; set; }
    public string Horas { get; set; }
    public string Descripcion { get; set; }
}

public class PausarSesionModel
{
    public Guid IdSesion { get; set; }
    public string NombreColaborador { get; set; }
    public Guid IdProyecto { get; set; }
    public string NombreProyecto { get; set; }
    public Guid IdUsuario { get; set; }
    public Guid IdServicio { get; set; }
    public string NombreServicio { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; }
    public double Horas { get; set; }
}

public class FinalizarSesionModel
{
    public Guid IdSesion { get; set; }
    public string NombreColaborador { get; set; }
    public Guid IdProyecto { get; set; }
    public string NombreProyecto { get; set; }
    public Guid IdUsuario { get; set; }
    public Guid IdServicio { get; set; }
    public string NombreServicio { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; }
}
