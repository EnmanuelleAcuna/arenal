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
public class ServiciosController : BaseController
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ServiciosController> _logger;

    public ServiciosController(
        ApplicationDbContext dbContext,
        ApplicationUserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IConfiguration configuration,
        IHttpContextAccessor contextAccesor,
        ILogger<ServiciosController> logger,
        IWebHostEnvironment environment)
        : base(userManager, roleManager, configuration, contextAccesor, environment)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Areas

    [HttpGet]
    public async Task<IActionResult> Areas()
    {
        List<Area> areas = await _dbContext.Areas.ToListAsync();
        return View(areas);
    }

    [HttpGet]
    public async Task<IActionResult> DetalleArea(Guid id)
    {
        Area model = await _dbContext.Areas.Include(a => a.Servicios).FirstOrDefaultAsync(a => a.Id == id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpGet]
    public IActionResult AgregarArea() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarArea(Area model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Common.MensajeErrorAgregar(nameof(Area)), GetModelStateErrors()));
            return View(model);
        }

        model.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
        await _dbContext.Areas.AddAsync(model);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Areas));
        ModelState.AddModelError("", Common.MensajeErrorAgregar(nameof(Area)));

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditarArea(Guid id)
    {
        var model = await _dbContext.Areas.FindAsync(id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditarArea(Area model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Common.MensajeErrorActualizar(nameof(Area)), GetModelStateErrors()));
            return View(model);
        }

        Area area = await _dbContext.Areas.FindAsync(model.Id);

        if (area == null) return NotFound();

        area.Actualizar(model, GetCurrentUser());
        _dbContext.Areas.Update(area);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Areas));
        ModelState.AddModelError("", Common.MensajeErrorActualizar(nameof(Area)));

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EliminarArea(Guid id)
    {
        Area model = await _dbContext.Areas.FindAsync(id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarArea(Area model)
    {
        Area area = await _dbContext.Areas.FindAsync(model.Id);

        if (area == null) return NotFound();

        area.Eliminar(GetCurrentUser());
        _dbContext.Areas.Update(area);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Areas));

        ModelState.AddModelError("", Common.MensajeErrorEliminar(nameof(Area)));
        return View(model);
    }

    #endregion

    #region Modalidades

    [HttpGet]
    public async Task<IActionResult> Modalidades()
    {
        List<Modalidad> modalidades = await _dbContext.Modalidades.ToListAsync();
        return View(modalidades);
    }

    [HttpGet]
    public IActionResult AgregarModalidad() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarModalidad(Modalidad model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Common.MensajeErrorAgregar(nameof(Modalidad)), GetModelStateErrors()));
            return View(model);
        }

        model.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
        await _dbContext.Modalidades.AddAsync(model);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Modalidades));
        ModelState.AddModelError("", Common.MensajeErrorAgregar(nameof(Modalidad)));

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditarModalidad(Guid id)
    {
        var model = await _dbContext.Modalidades.FindAsync(id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EditarModalidad(Modalidad model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Common.MensajeErrorActualizar(nameof(Modalidad)), GetModelStateErrors()));
            return View(model);
        }

        Modalidad modalidad = await _dbContext.Modalidades.FindAsync(model.Id);

        if (modalidad == null) return NotFound();

        modalidad.Actualizar(model, GetCurrentUser());
        _dbContext.Modalidades.Update(modalidad);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Modalidades));
        ModelState.AddModelError("", Common.MensajeErrorActualizar(nameof(Modalidad)));

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EliminarModalidad(Guid id)
    {
        Modalidad model = await _dbContext.Modalidades.FindAsync(id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarModalidad(Modalidad model)
    {
        Modalidad modalidad = await _dbContext.Modalidades.FindAsync(model.Id);

        if (modalidad == null) return NotFound();

        modalidad.Eliminar(GetCurrentUser());
        _dbContext.Modalidades.Update(modalidad);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Modalidades));

        ModelState.AddModelError("", Common.MensajeErrorEliminar(nameof(Modalidad)));
        return View(model);
    }

    #endregion

    #region Servicios

    [HttpGet]
    public async Task<IActionResult> Servicios()
    {
        IEnumerable<Servicio> model =
            await _dbContext.Servicios.Include(c => c.Area).Include(c => c.Modalidad).ToListAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> AgregarServicio()
    {
        IEnumerable<Area> areas = await _dbContext.Areas.ToListAsync();
        ViewBag.Areas = areas.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

        IEnumerable<Modalidad> modalidades = await _dbContext.Modalidades.ToListAsync();
        ViewBag.Modalidades = modalidades.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarServicio(Servicio modelo)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Common.MensajeErrorAgregar(nameof(Servicio)), GetModelStateErrors()));

            IEnumerable<Area> areas = await _dbContext.Areas.ToListAsync();
            ViewBag.Areas = areas.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

            IEnumerable<Modalidad> modalidades = await _dbContext.Modalidades.ToListAsync();
            ViewBag.Modalidades = modalidades.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

            return View(modelo);
        }

        modelo.RegristrarCreacion(GetCurrentUser(), DateTime.UtcNow);
        await _dbContext.Servicios.AddAsync(modelo);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Servicios));

        ModelState.AddModelError("", Common.MensajeErrorAgregar(nameof(Servicio)));
        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> EditarServicio(Guid id)
    {
        Servicio servicio = await _dbContext.Servicios.FindAsync(id);

        if (servicio == null) return NotFound();

        IEnumerable<Area> areas = await _dbContext.Areas.ToListAsync();
        ViewBag.Areas = areas.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

        IEnumerable<Modalidad> modalidades = await _dbContext.Modalidades.ToListAsync();
        ViewBag.Modalidades = modalidades.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

        return View(servicio);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarServicio(Servicio modelo)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("",
                string.Concat(Common.MensajeErrorActualizar(nameof(Servicio)), GetModelStateErrors()));

            IEnumerable<Area> areas = await _dbContext.Areas.ToListAsync();
            ViewBag.Areas = areas.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

            IEnumerable<Modalidad> modalidades = await _dbContext.Modalidades.ToListAsync();
            ViewBag.Modalidades = modalidades.Select(tc => new SelectListItem(text: tc.Nombre, tc.Id.ToString()));

            return View(modelo);
        }

        Servicio servicio = await _dbContext.Servicios.FindAsync(modelo.Id);

        if (servicio == null) return NotFound();

        servicio.Actualizar(modelo, GetCurrentUser());
        _dbContext.Servicios.Update(servicio);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Servicios));
        ModelState.AddModelError("", Common.MensajeErrorActualizar(nameof(Servicio)));

        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> EliminarServicio(Guid id)
    {
        Servicio servicio = await _dbContext.Servicios.FindAsync(id);

        if (servicio == null) return NotFound();

        return View(servicio);
    }

    [HttpPost]
    public async Task<IActionResult> EliminarServicio(Servicio modelo)
    {
        Servicio servicio = await _dbContext.Servicios.FindAsync(modelo.Id);

        if (servicio == null) return NotFound();

        servicio.Eliminar(GetCurrentUser());

        _dbContext.Servicios.Update(servicio);
        int changes = await _dbContext.SaveChangesAsync();

        if (changes > 0) return RedirectToAction(nameof(Servicios));

        ModelState.AddModelError("", Common.MensajeErrorEliminar(nameof(Servicio)));
        return View(modelo);
    }

    #endregion
}
