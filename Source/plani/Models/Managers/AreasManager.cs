using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plani.Models.Data;
using plani.Models.Domain;
using plani.Models.ViewModels;

namespace plani.Models.Managers;

/// <summary>
///     Manager para la lógica de negocio de Áreas
/// </summary>
public class AreasManager {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AreasManager> _logger;

    public AreasManager(ApplicationDbContext dbContext, ILogger<AreasManager> logger) {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    ///     Obtiene todas las áreas activas (no eliminadas)
    /// </summary>
    public async Task<IEnumerable<AreaListViewModel>> ObtenerTodasAsync() {
        List<Area> areas = await _dbContext.Areas
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.Nombre)
            .ToListAsync();

        return areas.Select(a => new AreaListViewModel(area: a));
    }

    /// <summary>
    ///     Obtiene áreas para dropdown
    /// </summary>
    public async Task<IEnumerable<SelectListItem>> ObtenerParaDropdownAsync() {
        return await _dbContext.Areas
            .OrderBy(a => a.Nombre)
            .Select(a => new SelectListItem(a.Nombre, a.Id.ToString()))
            .ToListAsync();
    }

    /// <summary>
    ///     Obtiene un área por ID
    /// </summary>
    public async Task<Area> ObtenerPorIdAsync(Guid id) {
        return await _dbContext.Areas.FindAsync(id);
    }

    /// <summary>
    ///     Obtiene un área con todas sus relaciones para vista de detalle
    /// </summary>
    public async Task<Area> ObtenerDetalleAsync(Guid id) {
        return await _dbContext.Areas
            .AsNoTracking()
            .Include(a => a.Servicios)
            .Include(a => a.Contratos)
            .ThenInclude(c => c.Cliente)
            .Include(a => a.Proyectos)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <summary>
    ///     Crea una nueva área
    /// </summary>
    public async Task<(bool Success, AreaListViewModel Data, string Error)> CrearAsync(
        AgregarAreaViewModel viewModel,
        string usuarioActual) {
        try {
            Area area = viewModel.ToEntity();
            area.RegristrarCreacion(creadoPor: usuarioActual, creadoEl: DateTime.UtcNow);

            await _dbContext.Areas.AddAsync(entity: area);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0) {
                AreaListViewModel result = new(area: area);
                return (true, result, null);
            }

            return (false, null, "No se pudo guardar el área");
        }
        catch (Exception ex) {
            _logger.LogError(exception: ex, "Error al crear área");
            return (false, null, "Error al crear el área");
        }
    }

    /// <summary>
    ///     Actualiza un área existente
    /// </summary>
    public async Task<(bool Success, AreaListViewModel Data, string Error)> ActualizarAsync(
        EditarAreaViewModel viewModel,
        string usuarioActual) {
        try {
            Area area = await _dbContext.Areas.FindAsync(Guid.Parse(input: viewModel.Id));

            if (area == null) {
                return (false, null, "Área no encontrada");
            }

            if (area.IsDeleted) {
                return (false, null, "El área ha sido eliminada y no puede ser modificada");
            }

            Area updatedArea = viewModel.ToEntity();
            area.Actualizar(area: updatedArea, actualizadoPor: usuarioActual);

            _dbContext.Areas.Update(entity: area);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0) {
                AreaListViewModel result = new(area: area);
                return (true, result, null);
            }

            return (false, null, "No se pudo actualizar el área");
        }
        catch (Exception ex) {
            _logger.LogError(exception: ex, "Error al actualizar área con ID: {Id}", viewModel.Id);
            return (false, null, "Error al actualizar el área");
        }
    }

    /// <summary>
    ///     Elimina (soft delete) un área
    /// </summary>
    public async Task<(bool Success, string Error)> EliminarAsync(Guid id, string usuarioActual) {
        try {
            Area area = await _dbContext.Areas.FindAsync(id);

            if (area == null) {
                return (false, "Área no encontrada");
            }

            if (area.IsDeleted) {
                return (false, "El área ya ha sido eliminada");
            }

            // Verificar si el área tiene relaciones activas
            bool tieneServicios = await _dbContext.Servicios
                .AnyAsync(s => s.IdArea == id && !s.IsDeleted);

            bool tieneContratos = await _dbContext.Contratos
                .AnyAsync(c => c.IdArea == id && !c.IsDeleted);

            bool tieneProyectos = await _dbContext.Proyectos
                .AnyAsync(p => p.IdArea == id && !p.IsDeleted);

            if (tieneServicios || tieneContratos || tieneProyectos) {
                return (false, "No se puede eliminar el área porque está siendo utilizada en servicios, contratos o proyectos");
            }

            area.Eliminar(eliminadoPor: usuarioActual);
            _dbContext.Areas.Update(entity: area);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0) {
                return (true, null);
            }

            return (false, "No se pudo eliminar el área");
        }
        catch (Exception ex) {
            _logger.LogError(exception: ex, "Error al eliminar área con ID: {Id}", id);
            return (false, "Error al eliminar el área. Puede estar siendo utilizada en otros registros.");
        }
    }

    /// <summary>
    ///     Verifica si existe un área con el mismo nombre (útil para validaciones)
    /// </summary>
    public async Task<bool> ExisteNombreAsync(string nombre, Guid excludeId = default) {
        IQueryable<Area> query = _dbContext.Areas
            .Where(a => !a.IsDeleted && a.Nombre.ToLower() == nombre.ToLower());

        if (excludeId != default) {
            query = query.Where(a => a.Id != excludeId);
        }

        return await query.AnyAsync();
    }
}