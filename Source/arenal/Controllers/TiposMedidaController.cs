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

namespace arenal.Controllers;

[Authorize]
public class TiposMedidaController : BaseController
{
	private readonly IBaseCore<TipoMedida> _tiposMedida;

	public TiposMedidaController(IBaseCore<TipoMedida> tiposMedida,
								 ApplicationUserManager<ApplicationUser> userManager,
								 RoleManager<ApplicationRole> roleManager,
								 IConfiguration configuration,
								 IHttpContextAccessor contextAccesor,
								 IWebHostEnvironment environment)
	: base(userManager, roleManager, configuration, contextAccesor, environment)
	{
		_tiposMedida = tiposMedida;
	}

	[HttpGet]
	public async Task<ActionResult> Listar()
	{
		IEnumerable<TipoMedida> listaTiposMedida = await _tiposMedida.ReadAllAsync();
		IEnumerable<TipoMedidaViewModel> modelo = listaTiposMedida.Select(x => new TipoMedidaViewModel(x)).ToList();
		return View(modelo);
	}

	[HttpGet]
	public ActionResult Agregar()
	{
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<ActionResult> Agregar(AgregarTipoMedidaViewModel modelo)
	{
		if (ModelState.IsValid)
		{
			await _tiposMedida.CreateAsync(modelo.Entidad(), GetCurrentUser());
			return RedirectToAction(nameof(Listar));
		}

		ModelState.AddModelError("", Messages.MensajeErrorCrear(nameof(TipoMedida)));
		return View(modelo);
	}

	[HttpGet]
	public async Task<ActionResult> Editar(string id)
	{
		TipoMedida tipoMedida = await _tiposMedida.ReadByIdAsync(new Guid(id));
		if (tipoMedida == null) return NotFound();
		EditarTipoMedidaViewModel modelo = new(tipoMedida);
		return View(modelo);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<ActionResult> Editar(EditarTipoMedidaViewModel modelo)
	{
		if (ModelState.IsValid)
		{
			await _tiposMedida.UpdateAsync(modelo.Entidad(), GetCurrentUser());
			return RedirectToAction(nameof(Listar));
		}

		ModelState.AddModelError("", Messages.MensajeErrorActualizar(nameof(TipoMedida)));
		return View(modelo);
	}

	[HttpGet]
	public async Task<ActionResult> Eliminar(string id)
	{
		TipoMedida tipoMedida = await _tiposMedida.ReadByIdAsync(new Guid(id));
		if (tipoMedida == null) return NotFound();
		EliminarTipoMedidaViewModel modelo = new(tipoMedida);
		return View(modelo);
	}

	[HttpPost]
	public async Task<ActionResult> Eliminar(EliminarTipoMedidaViewModel modelo)
	{
		if (ModelState.IsValid)
		{
			await _tiposMedida.DeleteAsync(new Guid(modelo.IdTipoMedida));
			return RedirectToAction(nameof(Listar));
		}

		ModelState.AddModelError("", Messages.MensajeErrorActualizar(nameof(TipoEjercicio)));
		return View(modelo);
	}

	[HttpGet]
	public async Task<JsonResult> Detalle(string id)
	{
		TipoMedida tipoMedida = await _tiposMedida.ReadByIdAsync(new Guid(id));
		var modelo = new TipoMedidaViewModel(tipoMedida);
		return Json(modelo);
	}
}
