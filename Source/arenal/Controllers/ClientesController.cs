using arenal.Data;
using arenal.Domain;
using arenal.Identity;
using arenal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace arenal.Controllers;

[Authorize]
public class ClientesController : BaseController
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ClientesController> _logger;
    private readonly ApplicationUserManager<ApplicationUser> _userManager;

    public ClientesController(
        ApplicationDbContext dbContext,
        ApplicationUserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
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
                string.Concat(Common.MensajeErrorAgregar(nameof(TipoCliente)), GetModelStateErrors()));
            return View(model);
        }

        model.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
        await _dbContext.TiposCliente.AddAsync(model);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(TiposCliente));
        ModelState.AddModelError("", Common.MensajeErrorAgregar(nameof(TipoCliente)));

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
                string.Concat(Common.MensajeErrorActualizar(nameof(TipoCliente)), GetModelStateErrors()));
            return View(model);
        }

        TipoCliente tipoCliente = await _dbContext.TiposCliente.FindAsync(model.Id);

        if (tipoCliente == null) return NotFound();

        tipoCliente.Actualizar(model, GetCurrentUser());
        _dbContext.TiposCliente.Update(tipoCliente);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(TiposCliente));
        ModelState.AddModelError("", Common.MensajeErrorActualizar(nameof(TipoCliente)));

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

        ModelState.AddModelError("", Common.MensajeErrorEliminar(nameof(TipoCliente)));
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
            .Include(c => c.Contratos)
            .ThenInclude(co => co.Area)
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
                string.Concat(Common.MensajeErrorAgregar(nameof(Cliente)), GetModelStateErrors()));

            IEnumerable<TipoCliente> tiposCliente = await _dbContext.TiposCliente.ToListAsync();
            ViewBag.TiposCliente = tiposCliente.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

            return View(model);
        }

        model.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
        await _dbContext.Clientes.AddAsync(model);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Clientes));

        ModelState.AddModelError("", Common.MensajeErrorAgregar(nameof(Cliente)));
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
                string.Concat(Common.MensajeErrorActualizar(nameof(Cliente)), GetModelStateErrors()));

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
        ModelState.AddModelError("", Common.MensajeErrorActualizar(nameof(Cliente)));

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

        ModelState.AddModelError("", Common.MensajeErrorEliminar(nameof(Cliente)));
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
                string.Concat(Common.MensajeErrorAgregar(nameof(Contrato)), GetModelStateErrors()));

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

        ModelState.AddModelError("", Common.MensajeErrorAgregar(nameof(Contrato)));
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
                string.Concat(Common.MensajeErrorActualizar(nameof(Contrato)), GetModelStateErrors()));

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
        ModelState.AddModelError("", Common.MensajeErrorActualizar(nameof(Contrato)));

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

        ModelState.AddModelError("", Common.MensajeErrorEliminar(nameof(Contrato)));
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
            new SelectListItem(text: $"{c.Cliente.Nombre} - {c.Identificacion}", c.Id.ToString()));

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
                string.Concat(Common.MensajeErrorAgregar(nameof(Proyecto)), GetModelStateErrors()));

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

        ModelState.AddModelError("", Common.MensajeErrorAgregar(nameof(Proyecto)));
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditarProyecto(Guid id)
    {
        Proyecto model = await _dbContext.Proyectos.FindAsync(id);

        if (model == null) return NotFound();

        IEnumerable<Contrato> contratos = await _dbContext.Contratos.Include(c => c.Cliente).ToListAsync();
        ViewBag.Contratos = contratos.Select(c =>
            new SelectListItem(text: $"{c.Cliente.Nombre} - {c.Identificacion}", c.Id.ToString()));

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
                string.Concat(Common.MensajeErrorActualizar(nameof(Proyecto)), GetModelStateErrors()));

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
        ModelState.AddModelError("", Common.MensajeErrorActualizar(nameof(Proyecto)));

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

        ModelState.AddModelError("", Common.MensajeErrorEliminar(nameof(Proyecto)));
        return View(model);
    }

    #endregion

    #region Asignaciones

    [HttpGet]
    public async Task<IActionResult> Asignaciones()
    {
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
                string.Concat(Common.MensajeErrorAgregar(nameof(Asignacion)), GetModelStateErrors()));

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

        ModelState.AddModelError("", Common.MensajeErrorAgregar(nameof(Asignacion)));
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

        ModelState.AddModelError("", Common.MensajeErrorEliminar(nameof(Asignacion)));
        return View(model);
    }

    #endregion

    #region Sesiones

    [HttpGet]
    public async Task<IActionResult> Sesiones()
    {
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
    
    [HttpGet]
    public async Task<IActionResult> AgregarSesion()
    {
        var currentUser = GetCurrentUser();
        ApplicationUser colaborador = await _userManager.FindByEmailAsync(currentUser);

        IEnumerable<Asignacion> asignaciones =
            await _dbContext.Asignaciones.Include(a => a.Proyecto).ThenInclude(c => c.Contrato).ThenInclude(c => c.Cliente).Where(a => a.IdColaborador == colaborador.Id).ToListAsync();
        ViewBag.Proyectos = asignaciones.Select(c =>
            new SelectListItem(text: $"{c.Proyecto.Contrato.Cliente.Nombre} - {c.Proyecto.Nombre}", c.IdProyecto.ToString()));

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
                string.Concat(Common.MensajeErrorAgregar(nameof(Sesion)), GetModelStateErrors()));

            IEnumerable<Asignacion> asignaciones =
                await _dbContext.Asignaciones.Include(a => a.Proyecto).ThenInclude(c => c.Contrato).ThenInclude(c => c.Cliente).Where(a => a.IdColaborador == colaborador.Id).ToListAsync();
            ViewBag.Proyectos = asignaciones.Select(c =>
                new SelectListItem(text: $"{c.Proyecto.Contrato.Cliente.Nombre} - {c.Proyecto.Nombre}", c.IdProyecto.ToString()));

            return View(model);
        }
        
        Sesion sesion = new Sesion()
        {
            IdColaborador = colaborador.Id,
            IdProyecto = model.IdProyecto,
            Fecha = model.Fecha,
            Horas = model.Horas,
            Descripcion = model.Descripcion
        };

        sesion.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
        await _dbContext.Sesiones.AddAsync(sesion);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Sesiones));

        ModelState.AddModelError("", Common.MensajeErrorAgregar(nameof(Sesion)));
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
    public int TotalHoras => Sesiones?.Sum(a => a.Horas) ?? 0;
    public int CantidadSesiones => Sesiones?.Count ?? 0;
}

// ViewModels/AsignacionesIndexViewModel.cs
public class SesionesIndexViewModel
{
    public List<ProyectoSesionesViewModel> ProyectosSesiones { get; set; }
    public int TotalAsignaciones => ProyectosSesiones?.Sum(p => p.CantidadSesiones) ?? 0;
    public int TotalHoras => ProyectosSesiones?.Sum(p => p.TotalHoras) ?? 0;
}

public class AgregarSesionModel
{
    public string NombreColaborador { get; set; }
    public Guid IdProyecto { get; set; }
    public Guid IdUsuario { get; set; }
    public DateTime Fecha { get; set; }
    public int Horas { get; set; }
    public string Descripcion { get; set; }
}
