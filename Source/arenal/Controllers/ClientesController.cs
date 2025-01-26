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
    public async Task<IActionResult> AgregarProyecto()
    {
        IEnumerable<Cliente> clientes = await _dbContext.Clientes.ToListAsync();
        ViewBag.Clientes = clientes.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

        IEnumerable<Contrato> contratos = await _dbContext.Contratos.ToListAsync();
        ViewBag.Contratos = contratos.Select(tc => new SelectListItem(text: tc.Identificacion, tc.Id.ToString()));

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

            IEnumerable<Cliente> clientes = await _dbContext.Clientes.ToListAsync();
            ViewBag.Clientes = clientes.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

            IEnumerable<Contrato> contratos = await _dbContext.Contratos.ToListAsync();
            ViewBag.Contratos = contratos.Select(tc => new SelectListItem(text: tc.Identificacion, tc.Id.ToString()));

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

        IEnumerable<Cliente> clientes = await _dbContext.Clientes.ToListAsync();
        ViewBag.Clientes = clientes.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

        IEnumerable<Contrato> contratos = await _dbContext.Contratos.ToListAsync();
        ViewBag.Contratos = contratos.Select(tc => new SelectListItem(text: tc.Identificacion, tc.Id.ToString()));

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

            IEnumerable<Cliente> clientes = await _dbContext.Clientes.ToListAsync();
            ViewBag.Clientes = clientes.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

            IEnumerable<Contrato> contratos = await _dbContext.Contratos.ToListAsync();
            ViewBag.Contratos = contratos.Select(tc => new SelectListItem(text: tc.Identificacion, tc.Id.ToString()));

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
}
