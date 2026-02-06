using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plani.Models.Data;
using plani.Models.ViewModels;

using plani.Models.Domain;

namespace plani.Models.Managers;

/// <summary>
/// Manager para la lógica de negocio de Proyectos y Asignaciones
/// </summary>
public class ProyectosManager
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ProyectosManager> _logger;
    private readonly IEmailSender _emailSender;

    public ProyectosManager(
        ApplicationDbContext dbContext,
        ILogger<ProyectosManager> logger,
        IEmailSender emailSender)
    {
        _dbContext = dbContext;
        _logger = logger;
        _emailSender = emailSender;
    }

    #region Proyectos

    /// <summary>
    /// Obtiene todos los proyectos con filtro opcional
    /// </summary>
    public async Task<IEnumerable<Proyecto>> ObtenerTodosProyectosAsync(string palabraClave = null)
    {
        var query = _dbContext.Proyectos
            .Include(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Include(p => p.Area)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(palabraClave))
        {
            var keyword = palabraClave.ToLower();
            query = query.Where(p =>
                p.Nombre.ToLower().Contains(keyword) ||
                p.Contrato.Cliente.Nombre.ToLower().Contains(keyword) ||
                p.Area.Nombre.ToLower().Contains(keyword));
        }

        return await query
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene todos los proyectos para dropdown
    /// </summary>
    public async Task<IEnumerable<SelectListItem>> ObtenerParaDropdownAsync()
    {
        return await _dbContext.Proyectos
            .Include(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .OrderBy(p => p.Contrato.Cliente.Nombre)
            .ThenBy(p => p.Nombre)
            .Select(p => new SelectListItem(
                $"{p.Contrato.Cliente.Nombre} - {p.Nombre}",
                p.Id.ToString()))
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene proyectos asignados a un usuario para dropdown
    /// </summary>
    public async Task<IEnumerable<SelectListItem>> ObtenerAsignadosParaDropdownAsync(string idUsuario)
    {
        return await _dbContext.Asignaciones
            .Where(a => a.IdColaborador == idUsuario)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .OrderBy(a => a.Proyecto.Contrato.Cliente.Nombre)
            .ThenBy(a => a.Proyecto.Nombre)
            .Select(a => new SelectListItem(
                $"{a.Proyecto.Contrato.Cliente.Nombre} - {a.Proyecto.Nombre}",
                a.Proyecto.Id.ToString()))
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene un proyecto por ID (sin relaciones)
    /// </summary>
    public async Task<Proyecto> ObtenerProyectoPorIdAsync(Guid id)
    {
        return await _dbContext.Proyectos.FindAsync(id);
    }

    /// <summary>
    /// Obtiene un proyecto con sus relaciones para vista de detalle
    /// </summary>
    public async Task<Proyecto> ObtenerProyectoDetalleAsync(Guid id)
    {
        return await _dbContext.Proyectos
            .Include(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Include(p => p.Area)
            .Include(p => p.Responsable)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Obtiene un proyecto con Contrato y Cliente para vista de eliminación
    /// </summary>
    public async Task<Proyecto> ObtenerProyectoConContratoAsync(Guid id)
    {
        return await _dbContext.Proyectos
            .Include(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Crea un nuevo proyecto
    /// </summary>
    public async Task<(bool Success, Proyecto Data, string Error)> CrearProyectoAsync(
        Proyecto proyecto,
        string usuarioActual)
    {
        try
        {
            proyecto.RegristrarCreacion(usuarioActual, DateTime.UtcNow);
            await _dbContext.Proyectos.AddAsync(proyecto);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return (true, proyecto, null);
            }

            return (false, null, "No se pudo guardar el proyecto");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear proyecto");
            return (false, null, "Error al crear el proyecto");
        }
    }

    /// <summary>
    /// Actualiza un proyecto existente
    /// </summary>
    public async Task<(bool Success, string Error)> ActualizarProyectoAsync(
        Proyecto proyectoActualizado,
        string usuarioActual)
    {
        try
        {
            var proyecto = await _dbContext.Proyectos.FindAsync(proyectoActualizado.Id);

            if (proyecto == null)
            {
                return (false, "Proyecto no encontrado");
            }

            proyecto.Actualizar(proyectoActualizado, usuarioActual);
            _dbContext.Proyectos.Update(proyecto);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return (true, null);
            }

            return (false, "No se pudo actualizar el proyecto");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar proyecto con ID: {Id}", proyectoActualizado.Id);
            return (false, "Error al actualizar el proyecto");
        }
    }

    /// <summary>
    /// Elimina (soft delete) un proyecto
    /// </summary>
    public async Task<(bool Success, string Error)> EliminarProyectoAsync(Guid id, string usuarioActual)
    {
        try
        {
            var proyecto = await _dbContext.Proyectos.FindAsync(id);

            if (proyecto == null)
            {
                return (false, "Proyecto no encontrado");
            }

            if (proyecto.IsDeleted)
            {
                return (false, "El proyecto ya ha sido eliminado");
            }

            // Verificar si tiene asignaciones activas
            var tieneAsignaciones = await _dbContext.Asignaciones
                .AnyAsync(a => a.IdProyecto == id && !a.IsDeleted);

            if (tieneAsignaciones)
            {
                return (false, "No se puede eliminar el proyecto porque tiene asignaciones activas");
            }

            // Verificar si tiene sesiones activas
            var tieneSesiones = await _dbContext.Sesiones
                .AnyAsync(s => s.IdProyecto == id && !s.IsDeleted);

            if (tieneSesiones)
            {
                return (false, "No se puede eliminar el proyecto porque tiene sesiones registradas");
            }

            proyecto.Eliminar(usuarioActual);
            _dbContext.Proyectos.Update(proyecto);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return (true, null);
            }

            return (false, "No se pudo eliminar el proyecto");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar proyecto con ID: {Id}", id);
            return (false, "Error al eliminar el proyecto");
        }
    }

    #endregion

    #region Asignaciones

    /// <summary>
    /// Obtiene todas las asignaciones con sus relaciones
    /// </summary>
    public async Task<List<Asignacion>> ObtenerTodasAsignacionesAsync()
    {
        return await _dbContext.Asignaciones
            .Include(a => a.ApplicationUser)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene asignaciones filtradas
    /// </summary>
    public async Task<List<Asignacion>> ObtenerAsignacionesFiltradasAsync(
        string idUsuario = null,
        string idProyecto = null)
    {
        return await _dbContext.Asignaciones
            .Where(a => (idUsuario == null || a.IdColaborador == idUsuario) &&
                        (idProyecto == null || a.IdProyecto.ToString() == idProyecto))
            .Include(a => a.ApplicationUser)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene asignaciones filtradas para exportación (ordenadas)
    /// </summary>
    public async Task<List<Asignacion>> ObtenerAsignacionesParaExportarAsync(
        string idUsuario = null,
        string idProyecto = null)
    {
        return await _dbContext.Asignaciones
            .Where(a => (idUsuario == null || a.IdColaborador == idUsuario) &&
                        (idProyecto == null || a.IdProyecto.ToString() == idProyecto))
            .Include(a => a.ApplicationUser)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .OrderBy(a => a.Proyecto.Contrato.Cliente.Nombre)
            .ThenBy(a => a.Proyecto.Nombre)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene asignaciones de un usuario específico
    /// </summary>
    public async Task<List<Asignacion>> ObtenerAsignacionesUsuarioAsync(string idUsuario)
    {
        return await _dbContext.Asignaciones
            .Include(a => a.ApplicationUser)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .Where(a => a.IdColaborador == idUsuario)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene una asignación por ID con todas sus relaciones
    /// </summary>
    public async Task<Asignacion> ObtenerAsignacionDetalleAsync(Guid id)
    {
        return await _dbContext.Asignaciones
            .Include(a => a.ApplicationUser)
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <summary>
    /// Verifica si existe una asignación duplicada
    /// </summary>
    public async Task<Asignacion> ObtenerAsignacionExistenteAsync(Guid idProyecto, string idColaborador)
    {
        return await _dbContext.Asignaciones
            .Include(a => a.Proyecto)
            .ThenInclude(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .FirstOrDefaultAsync(a =>
                a.IdProyecto == idProyecto &&
                a.IdColaborador == idColaborador);
    }

    /// <summary>
    /// Crea una nueva asignación y envía notificación por correo
    /// </summary>
    public async Task<(bool Success, Asignacion Data, string Error)> CrearAsignacionAsync(
        AgregarAsignacionModel model,
        string usuarioActual)
    {
        try
        {
            var existente = await ObtenerAsignacionExistenteAsync(model.IdProyecto, model.IdUsuario.ToString());
            if (existente != null)
            {
                return (false, null,
                    $"El colaborador ya está asignado al proyecto '{existente.Proyecto.Nombre}' " +
                    $"del cliente '{existente.Proyecto.Contrato.Cliente.Nombre}'. " +
                    $"No se pueden crear asignaciones duplicadas.");
            }

            var asignacion = new Asignacion
            {
                IdColaborador = model.IdUsuario.ToString(),
                IdProyecto = model.IdProyecto,
                HorasEstimadas = model.HorasEstimadas,
                Descripcion = model.Descripcion
            };

            asignacion.RegristrarCreacion(usuarioActual, DateTime.UtcNow);
            await _dbContext.Asignaciones.AddAsync(asignacion);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                await EnviarNotificacionAsignacionAsync(model.IdUsuario.ToString(), model.IdProyecto);
                return (true, asignacion, null);
            }

            return (false, null, "No se pudo guardar la asignación");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear asignación");
            return (false, null, "Error al crear la asignación");
        }
    }

    /// <summary>
    /// Envía notificación por correo cuando se asigna un proyecto a un colaborador
    /// </summary>
    private async Task EnviarNotificacionAsignacionAsync(string idColaborador, Guid idProyecto)
    {
        try
        {
            var colaborador = await _dbContext.Usuarios.FindAsync(idColaborador);
            var proyecto = await ObtenerProyectoParaNotificacionAsync(idProyecto);

            if (colaborador != null && proyecto != null)
            {
                var mensaje = new StringBuilder();
                mensaje.AppendLine(
                    $"{colaborador.FullName}, Se le ha asignado un nuevo proyecto {proyecto.Nombre} " +
                    $"del cliente {proyecto.Contrato.Cliente.Nombre}.");

                await _emailSender.SendEmailAsync(colaborador.Email, "Asignación de proyecto", mensaje.ToString());
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al enviar notificación de asignación para proyecto {IdProyecto}", idProyecto);
        }
    }

    /// <summary>
    /// Elimina (soft delete) una asignación
    /// </summary>
    public async Task<(bool Success, string IdColaborador, string Error)> EliminarAsignacionAsync(
        Guid id,
        string usuarioActual)
    {
        try
        {
            var asignacion = await _dbContext.Asignaciones.FindAsync(id);

            if (asignacion == null)
            {
                return (false, null, "Asignación no encontrada");
            }

            if (asignacion.IsDeleted)
            {
                return (false, null, "La asignación ya ha sido eliminada");
            }

            var idColaborador = asignacion.IdColaborador;

            asignacion.Eliminar(usuarioActual);
            _dbContext.Asignaciones.Update(asignacion);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return (true, idColaborador, null);
            }

            return (false, null, "No se pudo eliminar la asignación");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar asignación con ID: {Id}", id);
            return (false, null, "Error al eliminar la asignación");
        }
    }

    /// <summary>
    /// Obtiene un proyecto con sus detalles para notificación de asignación
    /// </summary>
    public async Task<Proyecto> ObtenerProyectoParaNotificacionAsync(Guid id)
    {
        return await _dbContext.Proyectos
            .Include(p => p.Contrato)
            .ThenInclude(c => c.Cliente)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Agrupa asignaciones por proyecto para vista
    /// </summary>
    public List<ProyectoAsignacionesViewModel> AgruparAsignacionesPorProyecto(List<Asignacion> asignaciones)
    {
        return asignaciones
            .GroupBy(a => a.IdProyecto)
            .Select(group => new ProyectoAsignacionesViewModel
            {
                IdProyecto = group.Key,
                NombreProyecto = group.First().Proyecto.Nombre,
                NombreCliente = group.First().Proyecto.Contrato.Cliente.Nombre,
                Asignaciones = group.ToList()
            })
            .OrderBy(p => p.NombreProyecto)
            .ToList();
    }

    /// <summary>
    /// Genera archivo Excel con las asignaciones
    /// </summary>
    public byte[] ExportarAsignacionesExcel(List<Asignacion> asignaciones)
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Asignaciones");

        worksheet.Cell(1, 1).Value = "Cliente";
        worksheet.Cell(1, 2).Value = "Proyecto";
        worksheet.Cell(1, 3).Value = "Colaborador";
        worksheet.Cell(1, 4).Value = "Horas Estimadas";
        worksheet.Cell(1, 5).Value = "Descripción";

        var headerRange = worksheet.Range(1, 1, 1, 5);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#1e3a5f");
        headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;

        int row = 2;
        foreach (var asignacion in asignaciones)
        {
            worksheet.Cell(row, 1).Value = asignacion.Proyecto?.Contrato?.Cliente?.Nombre;
            worksheet.Cell(row, 2).Value = asignacion.Proyecto?.Nombre;
            worksheet.Cell(row, 3).Value = asignacion.ApplicationUser?.FullName;
            worksheet.Cell(row, 4).Value = asignacion.HorasEstimadas;
            worksheet.Cell(row, 5).Value = asignacion.Descripcion;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion
}
