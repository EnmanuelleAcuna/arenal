using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plani.Identity;
using plani.Models.Data;

using plani.Models.Domain;

namespace plani.Models.Managers;

/// <summary>
/// Manager para la lógica de negocio de Colaboradores
/// </summary>
public class ColaboradoresManager
{
    private readonly ApplicationDbContext _dbContext;

    public ColaboradoresManager(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Obtiene todos los colaboradores para dropdown
    /// </summary>
    public async Task<IEnumerable<SelectListItem>> ObtenerParaDropdownAsync()
    {
        return await _dbContext.Usuarios
            .OrderBy(u => u.Name)
            .ThenBy(u => u.FirstLastName)
            .Select(u => new SelectListItem(u.FullName, u.Id))
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene un colaborador por ID
    /// </summary>
    public async Task<ApplicationUser> ObtenerPorIdAsync(string id)
    {
        return await _dbContext.Usuarios.FindAsync(id);
    }

    /// <summary>
    /// Obtiene un colaborador con sus asignaciones y proyectos a cargo para vista de detalle
    /// </summary>
    public async Task<ApplicationUser> ObtenerDetalleAsync(string id)
    {
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
