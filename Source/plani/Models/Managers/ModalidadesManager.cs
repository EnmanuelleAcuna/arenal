using Microsoft.EntityFrameworkCore;
using plani.Models.Data;
using plani.Models.ViewModels;

using plani.Models.Domain;

namespace plani.Models.Managers;

/// <summary>
/// Manager para la lógica de negocio de Modalidades
/// </summary>
public class ModalidadesManager
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ModalidadesManager> _logger;

    public ModalidadesManager(ApplicationDbContext dbContext, ILogger<ModalidadesManager> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las modalidades activas (no eliminadas)
    /// </summary>
    public async Task<IEnumerable<ModalidadListViewModel>> ObtenerTodasAsync()
    {
        var modalidades = await _dbContext.Modalidades
            .AsNoTracking()
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.Nombre)
            .ToListAsync();

        return modalidades.Select(m => new ModalidadListViewModel(m));
    }

    /// <summary>
    /// Obtiene una modalidad por ID
    /// </summary>
    public async Task<Modalidad> ObtenerPorIdAsync(Guid id)
    {
        return await _dbContext.Modalidades.FindAsync(id);
    }

    /// <summary>
    /// Obtiene una modalidad con sus servicios para vista de detalle
    /// </summary>
    public async Task<Modalidad> ObtenerDetalleAsync(Guid id)
    {
        return await _dbContext.Modalidades
            .Include(m => m.Servicios)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    /// <summary>
    /// Crea una nueva modalidad
    /// </summary>
    public async Task<(bool Success, ModalidadListViewModel Data, string Error)> CrearAsync(
        AgregarModalidadViewModel viewModel,
        string usuarioActual)
    {
        try
        {
            var modalidad = viewModel.ToEntity();
            modalidad.RegristrarCreacion(usuarioActual, DateTime.UtcNow);

            await _dbContext.Modalidades.AddAsync(modalidad);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                var result = new ModalidadListViewModel(modalidad);
                return (true, result, null);
            }

            return (false, null, "No se pudo guardar la modalidad");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear modalidad");
            return (false, null, "Error al crear la modalidad");
        }
    }

    /// <summary>
    /// Actualiza una modalidad existente
    /// </summary>
    public async Task<(bool Success, ModalidadListViewModel Data, string Error)> ActualizarAsync(
        EditarModalidadViewModel viewModel,
        string usuarioActual)
    {
        try
        {
            var modalidad = await _dbContext.Modalidades.FindAsync(Guid.Parse(viewModel.Id));

            if (modalidad == null)
            {
                return (false, null, "Modalidad no encontrada");
            }

            if (modalidad.IsDeleted)
            {
                return (false, null, "La modalidad ha sido eliminada y no puede ser modificada");
            }

            var updatedModalidad = viewModel.ToEntity();
            modalidad.Actualizar(updatedModalidad, usuarioActual);

            _dbContext.Modalidades.Update(modalidad);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                var result = new ModalidadListViewModel(modalidad);
                return (true, result, null);
            }

            return (false, null, "No se pudo actualizar la modalidad");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar modalidad con ID: {Id}", viewModel.Id);
            return (false, null, "Error al actualizar la modalidad");
        }
    }

    /// <summary>
    /// Elimina (soft delete) una modalidad
    /// </summary>
    public async Task<(bool Success, string Error)> EliminarAsync(Guid id, string usuarioActual)
    {
        try
        {
            var modalidad = await _dbContext.Modalidades.FindAsync(id);

            if (modalidad == null)
            {
                return (false, "Modalidad no encontrada");
            }

            if (modalidad.IsDeleted)
            {
                return (false, "La modalidad ya ha sido eliminada");
            }

            // Verificar si la modalidad tiene relaciones activas
            var tieneServicios = await _dbContext.Servicios
                .AnyAsync(s => s.IdModalidad == id && !s.IsDeleted);

            if (tieneServicios)
            {
                return (false, "No se puede eliminar la modalidad porque está siendo utilizada en servicios");
            }

            modalidad.Eliminar(usuarioActual);
            _dbContext.Modalidades.Update(modalidad);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return (true, null);
            }

            return (false, "No se pudo eliminar la modalidad");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar modalidad con ID: {Id}", id);
            return (false, "Error al eliminar la modalidad. Puede estar siendo utilizada en otros registros.");
        }
    }

    /// <summary>
    /// Verifica si existe una modalidad con el mismo nombre (útil para validaciones)
    /// </summary>
    public async Task<bool> ExisteNombreAsync(string nombre, Guid excludeId = default)
    {
        var query = _dbContext.Modalidades
            .Where(m => !m.IsDeleted && m.Nombre.ToLower() == nombre.ToLower());

        if (excludeId != default)
        {
            query = query.Where(m => m.Id != excludeId);
        }

        return await query.AnyAsync();
    }
}
