using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plani.Identity;
using plani.Models.Data;
using plani.Models.Domain;

namespace plani.Models.Managers;

/// <summary>
///     Manager para la lógica de negocio de Colaboradores
/// </summary>
public class ColaboradoresManager {
    private readonly ApplicationDbContext _dbContext;

    public ColaboradoresManager(ApplicationDbContext dbContext) {
        _dbContext = dbContext;
    }

    /// <summary>
    ///     Obtiene todos los colaboradores para dropdown
    /// </summary>
    public async Task<IEnumerable<SelectListItem>> ObtenerParaDropdownAsync() {
        return await _dbContext.Usuarios
            .OrderBy(u => u.Name)
            .ThenBy(u => u.FirstLastName)
            .Select(u => new SelectListItem(u.FullName, u.Id))
            .ToListAsync();
    }

    /// <summary>
    ///     Obtiene un colaborador por ID
    /// </summary>
    public async Task<ApplicationUser> ObtenerPorIdAsync(string id) {
        return await _dbContext.Usuarios.FindAsync(id);
    }

    /// <summary>
    ///     Valida si un colaborador puede ser eliminado.
    ///     Retorna el mensaje de error si tiene dependencias activas, o null si se puede eliminar.
    /// </summary>
    public async Task<string> ValidarEliminacionAsync(string id) {
        bool tieneAsignaciones = await _dbContext.Asignaciones
            .AnyAsync(a => a.IdColaborador == id && !a.IsDeleted);

        if (tieneAsignaciones) {
            return "No se puede eliminar el usuario porque tiene asignaciones activas";
        }

        bool tieneSesionesSinFinalizar = await _dbContext.Sesiones
            .AnyAsync(s => s.IdColaborador == id && s.Estado != EstadoSesion.Finalizada && !s.IsDeleted);

        if (tieneSesionesSinFinalizar) {
            return "No se puede eliminar el usuario porque tiene sesiones activas o pausadas";
        }

        bool esResponsableDeProyectos = await _dbContext.Proyectos
            .AnyAsync(p => p.IdResponsable == id && !p.IsDeleted);

        if (esResponsableDeProyectos) {
            return "No se puede eliminar el usuario porque es responsable de proyectos activos";
        }

        return null;
    }

    /// <summary>
    ///     Obtiene un colaborador con sus asignaciones y proyectos a cargo para vista de detalle
    /// </summary>
    public async Task<ApplicationUser> ObtenerDetalleAsync(string id) {
        return await _dbContext.Usuarios
            .Include(u => u.Asignaciones)
            .ThenInclude(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Include(u => u.ProyectosACargo)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Include(u => u.ProyectosACargo)
            .ThenInclude(p => p.Area)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}