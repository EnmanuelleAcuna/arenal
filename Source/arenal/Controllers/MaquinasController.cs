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
public class MaquinasController : BaseController
{
	private readonly IBaseCore<TipoMaquina> _tiposMaquina;
	private readonly IBaseCore<Maquina> _maquinas;
	private readonly ILogger<MaquinasController> _logger;

	public MaquinasController(IBaseCore<TipoMaquina> tiposMaquina,
						  	  IBaseCore<Maquina> maquinas,
						  	  ApplicationUserManager<ApplicationUser> userManager,
						  	  RoleManager<ApplicationRole> roleManager,
						  	  IConfiguration configuration,
						  	  IHttpContextAccessor contextAccesor,
						  	  ILogger<MaquinasController> logger,
						  	  IWebHostEnvironment environment)
							  : base(userManager, roleManager, configuration, contextAccesor, environment)
	{
		_tiposMaquina = tiposMaquina;
		_maquinas = maquinas;
		_logger = logger;
	}

	[HttpGet]
	public async Task<IActionResult> ListarMaquinas()
	{
		var maquinas = await _maquinas.ReadAllAsync();
		var modeloVista = maquinas.Select(x => new MaquinaViewModel(x)).ToList();
		return View(modeloVista);
	}

	[HttpGet]
	public async Task<IActionResult> AgregarMaquina()
	{
		ViewBag.ListaTiposMaquina = CargarListaSeleccionTiposMaquina(await _tiposMaquina.ReadAllAsync());
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> AgregarMaquina(AgregarMaquinaViewModel modeloVista)
	{
		if (!ModelState.IsValid)
		{
			ViewBag.ListaTiposMaquina = CargarListaSeleccionTiposMaquina(await _tiposMaquina.ReadAllAsync());
			ModelState.AddModelError("", Messages.MensajeModeloInvalido);
			return View(modeloVista);
		}

		await _maquinas.CreateAsync(modeloVista.Entidad(), CurrentUser);
		return RedirectToAction(nameof(ListarMaquinas));
	}

	[HttpGet]
	public async Task<IActionResult> EditarMaquina(string id)
	{
		var maquina = await _maquinas.ReadByIdAsync(new Guid(id));
		if (maquina == null) return NotFound();
		ViewBag.ListaTiposMaquina = CargarListaSeleccionTiposMaquina(await _tiposMaquina.ReadAllAsync());
		var modelo = new EditarMaquinaViewModel(maquina);
		return View(modelo);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> EditarMaquina(EditarMaquinaViewModel modeloVista)
	{
		if (!ModelState.IsValid)
		{
			ViewBag.ListaTiposMaquina = CargarListaSeleccionTiposMaquina(await _tiposMaquina.ReadAllAsync());
			ModelState.AddModelError("", Messages.MensajeModeloInvalido);
			return View(modeloVista);
		}

		await _maquinas.UpdateAsync(modeloVista.Entidad(), CurrentUser);
		return RedirectToAction(nameof(ListarMaquinas));
	}

	[HttpGet]
	public async Task<IActionResult> EliminarMaquina(string id)
	{
		var maquina = await _maquinas.ReadByIdAsync(new Guid(id));
		if (maquina == null) return NotFound();
		EliminarMaquinaViewModel modeloVista = new(maquina);
		return View(modeloVista);
	}

	[HttpPost]
	public async Task<IActionResult> EliminarMaquina(EliminarMaquinaViewModel modelo)
	{
		if (!ModelState.IsValid)
		{
			ModelState.AddModelError("", Messages.MensajeErrorEliminar(nameof(Maquina)));
			return View(modelo);
		}

		await _maquinas.DeleteAsync(new Guid(modelo.Id));
		return RedirectToAction(nameof(ListarMaquinas));
	}

	[HttpGet]
	public async Task<JsonResult> DetalleMaquina(string id)
	{
		Maquina maquina = await _maquinas.ReadByIdAsync(new Guid(id));
		var modelo = new MaquinaViewModel(maquina);
		return Json(modelo);
	}

	[HttpGet]
	public async Task<ActionResult> ListarTiposMaquina()
	{
		IEnumerable<TipoMaquina> listaTiposMaquina = await _tiposMaquina.ReadAllAsync();
		IEnumerable<TipoMaquinaViewModel> modelo = listaTiposMaquina.Select(x => new TipoMaquinaViewModel(x)).ToList();
		return View(modelo);
	}

	[HttpGet]
	public ActionResult AgregarTipoMaquina()
	{
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<ActionResult> AgregarTipoMaquina(AgregarTipoMaquinaViewModel modelo)
	{
		if (ModelState.IsValid)
		{
			await _tiposMaquina.CreateAsync(modelo.Entidad(), GetCurrentUser());
			return RedirectToAction(nameof(ListarTiposMaquina));
		}

		ModelState.AddModelError("", Messages.MensajeErrorCrear(nameof(TipoMaquina)));
		return View(modelo);
	}

	[HttpGet]
	public async Task<ActionResult> EditarTipoMaquina(string id)
	{
		TipoMaquina tipoMaquina = await _tiposMaquina.ReadByIdAsync(new Guid(id));
		if (tipoMaquina == null) return NotFound();
		var modelo = new EditarTipoMaquinaViewModel(tipoMaquina);
		return View(modelo);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<ActionResult> EditarTipoMaquina(EditarTipoMaquinaViewModel modelo)
	{
		if (ModelState.IsValid)
		{
			TipoMaquina tipoMaquina = modelo.Entidad();
			await _tiposMaquina.UpdateAsync(tipoMaquina, GetCurrentUser());
			return RedirectToAction(nameof(ListarTiposMaquina));
		}

		ModelState.AddModelError("", Messages.MensajeErrorActualizar(nameof(TipoMaquina)));
		return View(modelo);
	}

	[HttpGet]
	public async Task<ActionResult> EliminarTipoMaquina(string id)
	{
		TipoMaquina tipoMaquina = await _tiposMaquina.ReadByIdAsync(new Guid(id));
		if (tipoMaquina == null) return NotFound();
		var modelo = new EliminarTipoMaquinaViewModel(tipoMaquina);
		return View(modelo);
	}

	[HttpPost]
	public async Task<ActionResult> EliminarTipoMaquina(EliminarTipoMaquinaViewModel modelo)
	{
		if (ModelState.IsValid)
		{
			await _tiposMaquina.DeleteAsync(new Guid(modelo.Id));
			return RedirectToAction(nameof(ListarTiposMaquina));
		}

		ModelState.AddModelError("", Messages.MensajeErrorActualizar(nameof(TipoMaquina)));
		return View(modelo);
	}

	[HttpGet]
	public async Task<JsonResult> DetalleTipoMaquina(string id)
	{
		TipoMaquina tipoMaquina = await _tiposMaquina.ReadByIdAsync(new Guid(id));
		var modelo = new TipoMaquinaViewModel(tipoMaquina);
		return Json(modelo);
	}
}
