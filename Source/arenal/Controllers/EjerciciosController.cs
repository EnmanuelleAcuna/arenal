using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using arenal.Models;
using arenal.Models.Entities;
using arenal.Models.Extras;
using arenal.Models.Identity;
using arenal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace arenal.Controllers;

[Authorize]
public class EjerciciosController : BaseController
{
	private readonly IBaseCore<Ejercicio> _ejercicios;
	private readonly IBaseCore<TipoEjercicio> _tiposEjercicio;
	private readonly ILogger<EjerciciosController> _logger;

	public EjerciciosController(IBaseCore<Ejercicio> ejercicios,
								IBaseCore<TipoEjercicio> tiposEjercicio,
								ApplicationUserManager<ApplicationUser> userManager,
								RoleManager<ApplicationRole> roleManager,
								IConfiguration configuration,
								IHttpContextAccessor contextAccesor,
								ILogger<EjerciciosController> logger,
								IWebHostEnvironment environment)
	: base(userManager, roleManager, configuration, contextAccesor, environment)
	{
		_ejercicios = ejercicios;
		_tiposEjercicio = tiposEjercicio;
		_logger = logger;
	}

	[HttpGet]
	public async Task<ActionResult> ListarEjercicios()
	{
		var ejercicios = await _ejercicios.ReadAllAsync();
		var modelo = ejercicios.Select(x => new EjercicioViewModel(x)).ToList();
		return View(modelo);
	}

	[HttpGet]
	public async Task<ActionResult> AgregarEjercicio()
	{
		ViewBag.ListaTiposEjercicio = CargarListaSeleccionTiposEjercicio(await _tiposEjercicio.ReadAllAsync());
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<ActionResult> AgregarEjercicio(AgregarEjercicioViewModel modelo)
	{
		if (!ModelState.IsValid)
		{
			ViewBag.ListaTiposEjercicio = CargarListaSeleccionTiposEjercicio(await _tiposEjercicio.ReadAllAsync());
			ModelState.AddModelError("", Messages.MensajeModeloInvalido);
			return View(modelo);
		}

		await _ejercicios.CreateAsync(modelo.Entidad(), GetCurrentUser());
		return RedirectToAction(nameof(ListarEjercicios));
	}

	[HttpGet]
	public async Task<ActionResult> EditarEjercicio(string id)
	{
		var ejercicio = await _ejercicios.ReadByIdAsync(new Guid(id));
		ViewBag.ListaTiposEjercicio = CargarListaSeleccionTiposEjercicio(await _tiposEjercicio.ReadAllAsync());
		var modelo = new EditarEjercicioViewModel(ejercicio);
		return View(modelo);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<ActionResult> EditarEjercicio(EditarEjercicioViewModel modelo)
	{
		if (!ModelState.IsValid)
		{
			ViewBag.ListaTiposEjercicio = CargarListaSeleccionTiposEjercicio(await _tiposEjercicio.ReadAllAsync());
			ModelState.AddModelError("", Messages.MensajeErrorActualizar(nameof(Ejercicio)));
			return View(modelo);
		}

		await _ejercicios.UpdateAsync(modelo.Entidad(), GetCurrentUser());
		return RedirectToAction(nameof(ListarEjercicios));
	}

	[HttpGet]
	public async Task<ActionResult> EliminarEjercicio(string id)
	{
		var ejercicio = await _ejercicios.ReadByIdAsync(new Guid(id));
		var modelo = new EliminarEjercicioViewModel(ejercicio);
		return View(modelo);
	}

	[HttpPost]
	public async Task<ActionResult> EliminarEjercicio(EliminarEjercicioViewModel modelo)
	{
		if (!ModelState.IsValid)
		{
			ModelState.AddModelError("", Messages.MensajeErrorEliminar(nameof(Ejercicio)));
			return View(modelo);
		}

		await _ejercicios.DeleteAsync(new Guid(modelo.Id));
		return RedirectToAction(nameof(ListarEjercicios));
	}

	[HttpGet]
	public async Task<JsonResult> DetalleEjercicio(string id)
	{
		var ejercicio = await _ejercicios.ReadByIdAsync(new Guid(id));
		var modelo = new EjercicioViewModel(ejercicio);
		return Json(modelo);
	}

	[HttpGet]
	public async Task<ActionResult> ListarTiposEjercicio()
	{
		IEnumerable<TipoEjercicio> listaTiposEjercicio = await _tiposEjercicio.ReadAllAsync();
		IEnumerable<TipoEjercicioViewModel> modelo = listaTiposEjercicio.Select(x => new TipoEjercicioViewModel(x)).ToList();
		return View(modelo);
	}

	[HttpGet]
	public ActionResult AgregarTipoEjercicio()
	{
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<ActionResult> AgregarTipoEjercicio(AgregarTipoEjercicioViewModel modelo)
	{
		if (ModelState.IsValid)
		{
			await _tiposEjercicio.CreateAsync(modelo.Entidad(), GetCurrentUser());
			return RedirectToAction(nameof(ListarTiposEjercicio));
		}

		ModelState.AddModelError("", Messages.MensajeErrorCrear(nameof(TipoEjercicio)));
		return View(modelo);
	}

	[HttpGet]
	public async Task<ActionResult> EditarTipoEjercicio(string id)
	{
		TipoEjercicio tipoEjercicio = await _tiposEjercicio.ReadByIdAsync(new Guid(id));
		if (tipoEjercicio == null) return NotFound();
		EditarTipoEjercicioViewModel modelo = new(tipoEjercicio);
		return View(modelo);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<ActionResult> EditarTipoEjercicio(EditarTipoEjercicioViewModel modelo)
	{
		if (ModelState.IsValid)
		{
			TipoEjercicio tipoEjercicio = modelo.Entidad();
			await _tiposEjercicio.UpdateAsync(tipoEjercicio, GetCurrentUser());
			return RedirectToAction(nameof(ListarTiposEjercicio));
		}

		ModelState.AddModelError("", Messages.MensajeErrorActualizar(nameof(TipoEjercicio)));
		return View(modelo);
	}

	[HttpGet]
	public async Task<ActionResult> EliminarTipoEjercicio(string id)
	{
		TipoEjercicio tipoEjercicio = await _tiposEjercicio.ReadByIdAsync(new Guid(id));
		if (tipoEjercicio == null) return NotFound();
		EliminarTipoEjercicioViewModel modelo = new(tipoEjercicio);
		return View(modelo);
	}

	[HttpPost]
	public async Task<ActionResult> EliminarTipoEjercicio(EliminarTipoEjercicioViewModel modelo)
	{
		if (ModelState.IsValid)
		{
			await _tiposEjercicio.DeleteAsync(new Guid(modelo.IdTipoEjercicio));
			return RedirectToAction(nameof(ListarTiposEjercicio));
		}

		ModelState.AddModelError("", Messages.MensajeErrorActualizar(nameof(TipoEjercicio)));
		return View(modelo);
	}

	[HttpGet]
	public async Task<JsonResult> DetalleTipoEjercicio(string id)
	{
		TipoEjercicio tipoEjercicio = await _tiposEjercicio.ReadByIdAsync(new Guid(id));
		var modelo = new TipoEjercicioViewModel(tipoEjercicio);
		return Json(modelo);
	}
}
