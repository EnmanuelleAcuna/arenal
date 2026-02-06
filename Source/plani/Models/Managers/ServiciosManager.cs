using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plani.Models.Data;
using plani.Models.ViewModels;

using plani.Models.Domain;

namespace plani.Models.Managers;

/// <summary>
/// Manager para la lógica de negocio de Servicios
/// </summary>
public class ServiciosManager
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ServiciosManager> _logger;

    public ServiciosManager(ApplicationDbContext dbContext, ILogger<ServiciosManager> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los servicios activos (no eliminados)
    /// </summary>
    public async Task<IEnumerable<ServicioListViewModel>> ObtenerTodosAsync()
    {
        var servicios = await _dbContext.Servicios
            .AsNoTracking()
            .Include(s => s.Area)
            .Include(s => s.Modalidad)
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Nombre)
            .ToListAsync();

        return servicios.Select(s => new ServicioListViewModel(s));
    }

    /// <summary>
    /// Obtiene servicios para dropdown
    /// </summary>
    public async Task<IEnumerable<SelectListItem>> ObtenerParaDropdownAsync()
    {
        return await _dbContext.Servicios
            .OrderBy(s => s.Nombre)
            .Select(s => new SelectListItem(s.Nombre, s.Id.ToString()))
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene un servicio por ID
    /// </summary>
    public async Task<Servicio> ObtenerPorIdAsync(Guid id)
    {
        return await _dbContext.Servicios
            .Include(s => s.Area)
            .Include(s => s.Modalidad)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    /// <summary>
    /// Obtiene el detalle completo de un servicio incluyendo los proyectos donde se ha usado
    /// </summary>
    public async Task<ServicioDetalleViewModel> ObtenerDetallePorIdAsync(Guid id)
    {
        var servicio = await _dbContext.Servicios
            .Include(s => s.Area)
            .Include(s => s.Modalidad)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (servicio == null)
            return null;

        // Obtener proyectos únicos donde se ha usado este servicio
        var proyectos = await _dbContext.Sesiones
            .Where(s => s.IdServicio == id && !s.IsDeleted)
            .Include(s => s.Proyecto)
                .ThenInclude(p => p.Contrato)
                .ThenInclude(c => c.Cliente)
            .Include(s => s.Proyecto)
                .ThenInclude(p => p.Area)
            .Select(s => s.Proyecto)
            .Where(p => !p.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        // Obtener proyectos únicos (Distinct en memoria después de cargar)
        var proyectosUnicos = proyectos
            .GroupBy(p => p.Id)
            .Select(g => g.First())
            .OrderBy(p => p.Nombre)
            .ToList();

        var viewModel = new ServicioDetalleViewModel
        {
            Id = servicio.Id,
            Nombre = servicio.Nombre,
            Descripcion = servicio.Descripcion,
            AreaNombre = servicio.Area?.Nombre ?? string.Empty,
            ModalidadNombre = servicio.Modalidad?.Nombre ?? string.Empty,
            ProyectosUsandoServicio = proyectosUnicos.Select(p => new ProyectoUsandoServicioViewModel
            {
                IdProyecto = p.Id,
                NombreProyecto = p.Nombre,
                NombreCliente = p.Contrato?.Cliente?.Nombre ?? string.Empty,
                NombreArea = p.Area?.Nombre ?? string.Empty
            }).ToList()
        };

        return viewModel;
    }

    /// <summary>
    /// Crea un nuevo servicio
    /// </summary>
    public async Task<(bool Success, ServicioListViewModel Data, string Error)> CrearAsync(
        AgregarServicioViewModel viewModel,
        string usuarioActual)
    {
        try
        {
            var area = await _dbContext.Areas.FindAsync(Guid.Parse(viewModel.IdArea));
            if (area == null || area.IsDeleted)
            {
                return (false, null, "El área seleccionada no existe o fue eliminada");
            }

            var modalidad = await _dbContext.Modalidades.FindAsync(Guid.Parse(viewModel.IdModalidad));
            if (modalidad == null || modalidad.IsDeleted)
            {
                return (false, null, "La modalidad seleccionada no existe o fue eliminada");
            }

            var servicio = viewModel.ToEntity();
            servicio.RegristrarCreacion(usuarioActual, DateTime.UtcNow);

            await _dbContext.Servicios.AddAsync(servicio);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                servicio = await ObtenerPorIdAsync(servicio.Id);
                var result = new ServicioListViewModel(servicio);
                return (true, result, null);
            }

            return (false, null, "No se pudo guardar el servicio");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear servicio");
            return (false, null, "Error al crear el servicio");
        }
    }

    /// <summary>
    /// Actualiza un servicio existente
    /// </summary>
    public async Task<(bool Success, ServicioListViewModel Data, string Error)> ActualizarAsync(
        EditarServicioViewModel viewModel,
        string usuarioActual)
    {
        try
        {
            var servicio = await _dbContext.Servicios.FindAsync(Guid.Parse(viewModel.Id));

            if (servicio == null)
            {
                return (false, null, "Servicio no encontrado");
            }

            if (servicio.IsDeleted)
            {
                return (false, null, "El servicio ha sido eliminado y no puede ser modificado");
            }

            var area = await _dbContext.Areas.FindAsync(Guid.Parse(viewModel.IdArea));
            if (area == null || area.IsDeleted)
            {
                return (false, null, "El área seleccionada no existe o fue eliminada");
            }

            var modalidad = await _dbContext.Modalidades.FindAsync(Guid.Parse(viewModel.IdModalidad));
            if (modalidad == null || modalidad.IsDeleted)
            {
                return (false, null, "La modalidad seleccionada no existe o fue eliminada");
            }

            var updatedServicio = viewModel.ToEntity();
            servicio.Actualizar(updatedServicio, usuarioActual);

            _dbContext.Servicios.Update(servicio);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                servicio = await ObtenerPorIdAsync(servicio.Id);
                var result = new ServicioListViewModel(servicio);
                return (true, result, null);
            }

            return (false, null, "No se pudo actualizar el servicio");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar servicio con ID: {Id}", viewModel.Id);
            return (false, null, "Error al actualizar el servicio");
        }
    }

    /// <summary>
    /// Elimina (soft delete) un servicio
    /// </summary>
    public async Task<(bool Success, string Error)> EliminarAsync(Guid id, string usuarioActual)
    {
        try
        {
            var servicio = await _dbContext.Servicios.FindAsync(id);

            if (servicio == null)
            {
                return (false, "Servicio no encontrado");
            }

            if (servicio.IsDeleted)
            {
                return (false, "El servicio ya ha sido eliminado");
            }

            servicio.Eliminar(usuarioActual);
            _dbContext.Servicios.Update(servicio);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return (true, null);
            }

            return (false, "No se pudo eliminar el servicio");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar servicio con ID: {Id}", id);
            return (false, "Error al eliminar el servicio. Puede estar siendo utilizado en otros registros.");
        }
    }

    /// <summary>
    /// Verifica si existe un servicio con el mismo nombre (útil para validaciones)
    /// </summary>
    public async Task<bool> ExisteNombreAsync(string nombre, Guid excludeId = default)
    {
        var query = _dbContext.Servicios
            .Where(s => !s.IsDeleted && s.Nombre.ToLower() == nombre.ToLower());

        if (excludeId != default)
        {
            query = query.Where(s => s.Id != excludeId);
        }

        return await query.AnyAsync();
    }
}
