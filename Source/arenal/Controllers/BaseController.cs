using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using arenal.Models;
using arenal.Models.Entities;
using arenal.Models.Identity;
using arenal.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using IO = System.IO;

namespace arenal.Controllers;

public class BaseController : Controller
{
	private readonly ApplicationUserManager<ApplicationUser> _userManager;
	private readonly RoleManager<ApplicationRole> _roleManager;
	private readonly IConfiguration _configuration;
	private readonly IHttpContextAccessor _contextAccessor;
	private readonly IWebHostEnvironment _environment;

	public BaseController(ApplicationUserManager<ApplicationUser> userManager,
						  RoleManager<ApplicationRole> roleManager,
						  IConfiguration configuration,
						  IHttpContextAccessor contextAccessor,
						  IWebHostEnvironment environment)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_configuration = configuration;
		_contextAccessor = contextAccessor;
		_environment = environment;
	}

	public async Task CreateDefaultUser()
	{
		if (await _userManager.Users.AnyAsync()) return;

		var rolAdministrador = new ApplicationRole(Guid.NewGuid().ToString(), "Administrador", "Administrador del sistema.");
		var rolInstructor = new ApplicationRole(Guid.NewGuid().ToString(), "Instructor", "Instructor del gimnasio.");
		var rolCliente = new ApplicationRole(Guid.NewGuid().ToString(), "Cliente", "Cliente del gimnasio.");

		await _roleManager.CreateAsync(rolAdministrador);
		await _roleManager.CreateAsync(rolInstructor);
		await _roleManager.CreateAsync(rolCliente);

		var usuario = new ApplicationUser(Guid.NewGuid().ToString(), "emanuelacu@gmail.com",
										  "emanuelacu@gmail.com", "Enmanuelle", "Acuña", "Arguedas",
										  "206830685", DateTime.Now, true);

		var rolesSeleccionados = new List<string> { "Administrador", "Instructor" };

		await _userManager.CreateAsync(usuario, "ContraseñaGenerica");
		await _userManager.AddToRolesAsync(usuario, rolesSeleccionados);

		await Task.CompletedTask;
	}

	public string GetCurrentUser() => User.Identity.Name;

	public string CurrentUser
	{
		get
		{
			return User.Identity.Name;
		}
	}

	public IActionResult RedirectToLocal(string returnUrl)
	{
		if (Url.IsLocalUrl(returnUrl))
		{
			return Redirect(returnUrl);
		}

		return RedirectToAction("Index", "Home");
	}

	public void AddErrors(IdentityResult Result)
	{
		foreach (IdentityError error in Result.Errors)
		{
			ModelState.AddModelError("", error.Description);
		}
	}

	public async Task<IList<SelectListItem>> CargarListaSeleccionProvincias()
	{
		// IList<Provincia> provincias = await _divisionTerritorial.Provincias.ReadAllAsync();
		// IList<SelectListItem> listaSeleccionProvincias = provincias.Select(p => new SelectListItem { Value = Convert.ToString(p.Id.ToString(), new CultureInfo("es-CR")), Text = p.Nombre }).ToList();
		// return listaSeleccionProvincias;
		await Task.CompletedTask;
		return new List<SelectListItem>();
	}

	public async Task<IList<CantonesDistritosSelectListItem>> CargarListaSeleccionCantones()
	{
		// IList<Canton> cantones = await _divisionTerritorial.Cantones.ReadAllAsync();
		// IList<CantonesDistritosSelectListItem> listaSeleccionCantones = cantones.Select(c => new CantonesDistritosSelectListItem { Value = c.Id.ToString(), Text = c.Nombre, IdPadre = c.Provincia.Id.ToString() }).ToList();
		// return listaSeleccionCantones;
		await Task.CompletedTask;
		return new List<CantonesDistritosSelectListItem>();
	}

	public async Task<IList<CantonesDistritosSelectListItem>> CargarListaSeleccionDistritos()
	{
		// IList<Distrito> distritos = await _divisionTerritorial.Distritos.ReadAllAsync();
		// IList<CantonesDistritosSelectListItem> listaSeleccionDistritos = distritos.Select(d => new CantonesDistritosSelectListItem { Value = d.Id.ToString(), Text = d.Nombre, IdPadre = d.Canton.Id.ToString() }).ToList();
		// return listaSeleccionDistritos;
		await Task.CompletedTask;
		return new List<CantonesDistritosSelectListItem>();
	}

	public async Task<IList<SelectListItem>> CargarListaSeleccionDistritosConCantonesProvincias()
	{
		// IList<Distrito> distritos = await _divisionTerritorial.Distritos.ReadAllAsync();
		// IList<SelectListItem> listaSeleccionDistritos = distritos.Select(d => new SelectListItem { Value = Convert.ToString(d.Id.ToString(), new CultureInfo("es-CR")), Text = String.Format("{0}, {1}, {2}", d.Canton.Provincia.Nombre, d.Canton.Nombre, d.Nombre) }).ToList();
		// return listaSeleccionDistritos;
		await Task.CompletedTask;
		return new List<SelectListItem>();
	}

	public IList<SelectListItem> CargarListaSeleccionRoles()
	{
		IList<ApplicationRole> listaRoles = _roleManager.Roles.ToList();
		IList<SelectListItem> listaSeleccionRoles = listaRoles.Select(p => new SelectListItem { Value = p.Id, Text = p.Name }).ToList();
		return listaSeleccionRoles;
	}

	public static IList<string> ObtenerRolesSeleccionados(IFormCollection collection)
	{
		// En la colección vienen los rols seleccionados y la llave es el id del rol chequeado
		// Recorrer los roles chequeados y tomar el id que es el id del rol y crear un
		// objeto rol asignando la propiedad id obtenida del collection
		IList<string> rolesSeleccionados = new List<string>();
		foreach (string key in collection.Keys)
		{
			if (key[..1].Equals("R", StringComparison.OrdinalIgnoreCase))
			{
				string rolSeleccionado = key[2..];
				rolesSeleccionados.Add(rolSeleccionado);
			}
		}

		return rolesSeleccionados;
	}

	// TODO: Mover esto a ApplicationUserManager
	public async Task<IList<ApplicationRole>> ObtenerRolesUsuario(ApplicationUser usuario)
	{
		IList<string> nombresRolesUsuario = await _userManager.GetRolesAsync(usuario);

		IList<ApplicationRole> rolesUsuario = new List<ApplicationRole>();
		foreach (string nombreRol in nombresRolesUsuario)
		{
			ApplicationRole rol = await _roleManager.FindByNameAsync(nombreRol);
			rolesUsuario.Add(rol);
		}

		return rolesUsuario;
	}

	public static IEnumerable<SelectListItem> CargarListaSeleccionTiposMaquina(IEnumerable<TipoMaquina> listaTiposMaquina)
	{
		IEnumerable<SelectListItem> listaSeleccionTiposMaquina = listaTiposMaquina.Select(p => new SelectListItem { Value = Convert.ToString(p.Id.ToString(), new CultureInfo("es-CR")), Text = p.Nombre }).ToList();
		return listaSeleccionTiposMaquina;
	}

	public static IEnumerable<SelectListItem> CargarListaSeleccionMaquinas(IEnumerable<Maquina> listaMaquinas)
	{
		IEnumerable<SelectListItem> listaSeleccionMaquinas = listaMaquinas.Select(p => new SelectListItem { Value = Convert.ToString(p.Id.ToString(), new CultureInfo("es-CR")), Text = p.Nombre }).ToList();
		return listaSeleccionMaquinas;
	}

	public static IEnumerable<SelectListItem> CargarListaSeleccionTiposEjercicio(IEnumerable<TipoEjercicio> listaTiposEjercicio)
	{
		IEnumerable<SelectListItem> listaSeleccionTiposEjercicio = listaTiposEjercicio.Select(p => new SelectListItem { Value = Convert.ToString(p.Id.ToString(), new CultureInfo("es-CR")), Text = p.Nombre }).ToList();
		return listaSeleccionTiposEjercicio;
	}

	public static IEnumerable<SelectListItem> CargarListaSeleccionEjercicios(IEnumerable<Ejercicio> listaEjercicios)
	{
		IEnumerable<SelectListItem> listaSeleccionEjercicios = listaEjercicios.Select(p => new SelectListItem { Value = Convert.ToString(p.Id.ToString(), new CultureInfo("es-CR")), Text = p.Nombre }).ToList();
		return listaSeleccionEjercicios;
	}

	public static IEnumerable<SelectListItem> CargarListaSeleccionGruposMusculares(IEnumerable<GrupoMuscular> listaGruposMusculares)
	{
		IEnumerable<SelectListItem> listaSeleccionGruposMusculares = listaGruposMusculares.Select(p => new SelectListItem { Value = Convert.ToString(p.Id.ToString(), new CultureInfo("es-CR")), Text = p.Nombre }).ToList();
		return listaSeleccionGruposMusculares;
	}

	public static IEnumerable<SelectListItem> CargarListaSeleccionTiposMedida(IEnumerable<TipoMedida> listaTiposMedida)
	{
		IEnumerable<SelectListItem> listaSeleccionTiposMedida = listaTiposMedida.Select(p => new SelectListItem { Value = Convert.ToString(p.Id.ToString(), new CultureInfo("es-CR")), Text = p.Nombre }).ToList();
		return listaSeleccionTiposMedida;
	}

	public IEnumerable<SelectListItem> CargarListaSeleccionUsuarios(IEnumerable<ApplicationUser> listaUsuarios)
	{
		IEnumerable<SelectListItem> listaSeleccionUsuarios = listaUsuarios.Select(p => new SelectListItem { Value = Convert.ToString(p.Id.ToString(), new CultureInfo("es-CR")), Text = string.Format("{0} {1} {2}", p.Name, p.FirstLastName, p.SecondLastName) }).ToList();
		return listaSeleccionUsuarios;
	}
	
	public IEnumerable<SelectListItem> CargarListaSeleccionClientes(IEnumerable<ApplicationUser> listaUsuarios)
	{
		IEnumerable<SelectListItem> listaSeleccionUsuarios = listaUsuarios.Select(p => new SelectListItem { Value = Convert.ToString(p.Id.ToString(), new CultureInfo("es-CR")), Text = string.Format("{0} {1} {2}", p.Name, p.FirstLastName, p.SecondLastName) }).ToList();
		return listaSeleccionUsuarios;
	}
	
	public IEnumerable<SelectListItem> CargarListaSeleccionInstructores(IEnumerable<ApplicationUser> listaUsuarios)
	{
		IEnumerable<SelectListItem> listaSeleccionUsuarios = listaUsuarios.Select(p => new SelectListItem { Value = Convert.ToString(p.Id.ToString(), new CultureInfo("es-CR")), Text = string.Format("{0} {1} {2}", p.Name, p.FirstLastName, p.SecondLastName) }).ToList();
		return listaSeleccionUsuarios;
	}

	// protected async Task CargarImagenUsuario(IEnumerable<Maquina> maquinas)
	// {
	// 	foreach (Maquina maquina in maquinas)
	// 	{
	// 		maquina.SetImagen(await ObtenerImagen(maquina.Id));
	// 	}
	// }

	// public async Task<Imagen> ObtenerImagen(Guid idObjeto)
	// {
	// 	Imagen imagen = await _repoImagenes.ReadByIdAsync(idObjeto);
	// 	return imagen;
	// }

	// public async Task<Imagen> GuardarImagenAsync(Guid idObjeto, IFormFile archivoSubido, string carpeta)
	// {
	// 	Uri uriImagen = GuardarImagenDisco(archivoSubido, carpeta);
	// 	Imagen imagen = new(idObjeto, uriImagen);
	// 	await _repoImagenes.CreateAsync(imagen);
	// 	return imagen;
	// }

	// public async Task<Imagen> ActualizarImagenAsync(Guid idObjeto, IFormFile archivoSubido, string carpeta)
	// {
	// 	Uri uriNuevaImagen = GuardarImagenDisco(archivoSubido, carpeta);
	// 	Imagen imagen = await _repoImagenes.ReadByIdAsync(idObjeto);
	// 	EliminarImagenDisco(imagen.URL);
	// 	imagen.EstablecerNuevaUri(uriNuevaImagen);
	// 	await _repoImagenes.UpdateAsync(imagen);
	// 	return imagen;
	// }

	// public async Task EliminarImagenAsync(Guid idObjeto)
	// {
	// 	Imagen imagen = await _repoImagenes.ReadByIdAsync(idObjeto);
	// 	EliminarImagenDisco(imagen.URL);
	// 	await _repoImagenes.DeleteAsync(idObjeto);
	// }

	public string GuardarImagenDisco(IFormFile archivo)
	{
		var filePath = Path.Combine(_environment.WebRootPath, "images", archivo.FileName);

		using (var stream = new FileStream(filePath, FileMode.Create))
		{
			archivo.CopyTo(stream);
		}

		return $"images/{archivo.FileName}";
	}

	private void EliminarImagenDisco(Uri uri)
	{
		string rutaImagen = Path.Combine(_environment.WebRootPath, uri.LocalPath.TrimStart('/').Replace("/", "\\"));
		if (IO.File.Exists(rutaImagen)) IO.File.Delete(rutaImagen);
	}

	private static void ValidarArchivoSubido(IFormFile archivoSubido)
	{
		// if (archivoSubido is null) throw new ArgumentNullException(paramName: nameof(Imagen), message: Messages.MensajeArchivoNulo);

		// TODO: Validar extension de archivo de imagen suministrado
	}
}
