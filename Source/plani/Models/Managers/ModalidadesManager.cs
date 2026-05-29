using Microsoft.EntityFrameworkCore;
using plani.Models.Data;
using plani.Models.Domain;
using plani.Models.ViewModels;

namespace plani.Models.Managers;

/// <summary>
///     Manager para la lógica de negocio de Modalidades
/// </summary>
public class ModalidadesManager {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ModalidadesManager> _logger;

    public ModalidadesManager(ApplicationDbContext dbContext, ILogger<ModalidadesManager> logger) {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    ///     Obtiene todas las modalidades activas (no eliminadas)
    /// </summary>
    public async Task<IEnumerable<ModalidadListViewModel>> ObtenerTodasAsync() {
        List<Modalidad> modalidades = await _dbContext.Modalidades
            .AsNoTracking()
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.Nombre)
            .ToListAsync();

        return modalidades.Select(m => new ModalidadListViewModel(modalidad: m));
    }

    /// <summary>
    ///     Obtiene una modalidad por ID
    /// </summary>
    public async Task<Modalidad> ObtenerPorIdAsync(Guid id) {
        return await _dbContext.Modalidades.FindAsync(id);
    }

    /// <summary>
    ///     Obtiene una modalidad con sus servicios para vista de detalle
    /// </summary>
    public async Task<Modalidad> ObtenerDetalleAsync(Guid id) {
        return await _dbContext.Modalidades
            .Include(m => m.Servicios)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    /// <summary>
    ///     Crea una nueva modalidad
    /// </summary>
    public async Task<(bool Success, ModalidadListViewModel Data, string Error)> CrearAsync(
        AgregarModalidadViewModel viewModel,
        string usuarioActual) {
        try {
            Modalidad modalidad = viewModel.ToEntity();
            modalidad.RegristrarCreacion(creadoPor: usuarioActual, creadoEl: DateTime.UtcNow);

            await _dbContext.Modalidades.AddAsync(entity: modalidad);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0) {
                ModalidadListViewModel result = new(modalidad: modalidad);
                return (true, result, null);
            }

            return (false, null, "No se pudo guardar la modalidad");
        }
        catch (Exception ex) {
            _logger.LogError(exception: ex, "Error al crear modalidad");
            return (false, null, "Error al crear la modalidad");
        }
    }

    /// <summary>
    ///     Actualiza una modalidad existente
    /// </summary>
    public async Task<(bool Success, ModalidadListViewModel Data, string Error)> ActualizarAsync(
        EditarModalidadViewModel viewModel,
        string usuarioActual) {
        try {
            Modalidad modalidad = await _dbContext.Modalidades.FindAsync(Guid.Parse(input: viewModel.Id));

            if (modalidad == null) {
                return (false, null, "Modalidad no encontrada");
            }

            if (modalidad.IsDeleted) {
                return (false, null, "La modalidad ha sido eliminada y no puede ser modificada");
            }

            Modalidad updatedModalidad = viewModel.ToEntity();
            modalidad.Actualizar(modalidad: updatedModalidad, actualizadoPor: usuarioActual);

            _dbContext.Modalidades.Update(entity: modalidad);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0) {
                ModalidadListViewModel result = new(modalidad: modalidad);
                return (true, result, null);
            }

            return (false, null, "No se pudo actualizar la modalidad");
        }
        catch (Exception ex) {
            _logger.LogError(exception: ex, "Error al actualizar modalidad con ID: {Id}", viewModel.Id);
            return (false, null, "Error al actualizar la modalidad");
        }
    }

    /// <summary>
    ///     Elimina (soft delete) una modalidad
    /// </summary>
    public async Task<(bool Success, string Error)> EliminarAsync(Guid id, string usuarioActual) {
        try {
            Modalidad modalidad = await _dbContext.Modalidades.FindAsync(id);

            if (modalidad == null) {
                return (false, "Modalidad no encontrada");
            }

            if (modalidad.IsDeleted) {
                return (false, "La modalidad ya ha sido eliminada");
            }

            // Verificar si la modalidad tiene relaciones activas
            bool tieneServicios = await _dbContext.Servicios
                .AnyAsync(s => s.IdModalidad == id && !s.IsDeleted);

            if (tieneServicios) {
                return (false, "No se puede eliminar la modalidad porque está siendo utilizada en servicios");
            }

            modalidad.Eliminar(eliminadoPor: usuarioActual);
            _dbContext.Modalidades.Update(entity: modalidad);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0) {
                return (true, null);
            }

            return (false, "No se pudo eliminar la modalidad");
        }
        catch (Exception ex) {
            _logger.LogError(exception: ex, "Error al eliminar modalidad con ID: {Id}", id);
            return (false, "Error al eliminar la modalidad. Puede estar siendo utilizada en otros registros.");
        }
    }

    /// <summary>
    ///     Verifica si existe una modalidad con el mismo nombre (útil para validaciones)
    /// </summary>
    public async Task<bool> ExisteNombreAsync(string nombre, Guid excludeId = default) {
        IQueryable<Modalidad> query = _dbContext.Modalidades
            .Where(m => !m.IsDeleted && m.Nombre.ToLower() == nombre.ToLower());

        if (excludeId != default) {
            query = query.Where(m => m.Id != excludeId);
        }

        return await query.AnyAsync();
    }
}