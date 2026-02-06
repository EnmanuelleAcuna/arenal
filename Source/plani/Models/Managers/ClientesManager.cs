using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plani.Models.Data;
using plani.Models.ViewModels;

using plani.Models.Domain;

namespace plani.Models.Managers;

/// <summary>
/// Manager para la lógica de negocio de Clientes, TiposCliente y Contratos
/// </summary>
public class ClientesManager
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ClientesManager> _logger;

    public ClientesManager(ApplicationDbContext dbContext, ILogger<ClientesManager> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region TiposCliente

    /// <summary>
    /// Obtiene todos los tipos de cliente para listado
    /// </summary>
    public async Task<IEnumerable<TipoClienteListViewModel>> ObtenerTodosTiposClienteAsync()
    {
        var tiposCliente = await _dbContext.TiposCliente
            .AsNoTracking()
            .OrderBy(tc => tc.Nombre)
            .ToListAsync();

        return tiposCliente.Select(tc => new TipoClienteListViewModel(tc));
    }

    /// <summary>
    /// Obtiene tipos de cliente para dropdown
    /// </summary>
    public async Task<IEnumerable<SelectListItem>> ObtenerTiposClienteParaDropdownAsync()
    {
        return await _dbContext.TiposCliente
            .OrderBy(tc => tc.Nombre)
            .Select(tc => new SelectListItem(tc.Nombre, tc.Id.ToString()))
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene un tipo de cliente por ID con sus clientes relacionados
    /// </summary>
    public async Task<TipoCliente> ObtenerTipoClientePorIdAsync(Guid id)
    {
        return await _dbContext.TiposCliente
            .Include(tc => tc.Clientes)
            .FirstOrDefaultAsync(tc => tc.Id == id);
    }

    /// <summary>
    /// Crea un nuevo tipo de cliente
    /// </summary>
    public async Task<(bool Success, TipoClienteListViewModel Data, string Error)> CrearTipoClienteAsync(
        AgregarTipoClienteViewModel viewModel,
        string usuarioActual)
    {
        try
        {
            var tipoCliente = viewModel.ToEntity();
            tipoCliente.RegristrarCreacion(usuarioActual, DateTime.UtcNow);

            await _dbContext.TiposCliente.AddAsync(tipoCliente);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                var result = new TipoClienteListViewModel(tipoCliente);
                return (true, result, null);
            }

            return (false, null, "No se pudo guardar el tipo de cliente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tipo de cliente");
            return (false, null, "Error al crear el tipo de cliente");
        }
    }

    /// <summary>
    /// Actualiza un tipo de cliente existente
    /// </summary>
    public async Task<(bool Success, TipoClienteListViewModel Data, string Error)> ActualizarTipoClienteAsync(
        EditarTipoClienteViewModel viewModel,
        string usuarioActual)
    {
        try
        {
            var tipoCliente = await _dbContext.TiposCliente.FindAsync(Guid.Parse(viewModel.Id));

            if (tipoCliente == null)
            {
                return (false, null, "Tipo de cliente no encontrado");
            }

            if (tipoCliente.IsDeleted)
            {
                return (false, null, "El tipo de cliente ha sido eliminado y no puede ser modificado");
            }

            var updatedEntity = viewModel.ToEntity();
            tipoCliente.Actualizar(updatedEntity, usuarioActual);

            _dbContext.TiposCliente.Update(tipoCliente);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                var result = new TipoClienteListViewModel(tipoCliente);
                return (true, result, null);
            }

            return (false, null, "No se pudo actualizar el tipo de cliente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tipo de cliente con ID: {Id}", viewModel.Id);
            return (false, null, "Error al actualizar el tipo de cliente");
        }
    }

    /// <summary>
    /// Elimina (soft delete) un tipo de cliente
    /// </summary>
    public async Task<(bool Success, string Error)> EliminarTipoClienteAsync(Guid id, string usuarioActual)
    {
        try
        {
            var tipoCliente = await _dbContext.TiposCliente.FindAsync(id);

            if (tipoCliente == null)
            {
                return (false, "Tipo de cliente no encontrado");
            }

            if (tipoCliente.IsDeleted)
            {
                return (false, "El tipo de cliente ya ha sido eliminado");
            }

            // Verificar si tiene clientes asignados
            var tieneClientes = await _dbContext.Clientes
                .AnyAsync(c => c.IdTipoCliente == id && !c.IsDeleted);

            if (tieneClientes)
            {
                return (false, "No se puede eliminar el tipo de cliente porque tiene clientes asignados");
            }

            tipoCliente.Eliminar(usuarioActual);
            _dbContext.TiposCliente.Update(tipoCliente);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return (true, null);
            }

            return (false, "No se pudo eliminar el tipo de cliente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar tipo de cliente con ID: {Id}", id);
            return (false, "Error al eliminar el tipo de cliente");
        }
    }

    #endregion

    #region Clientes

    /// <summary>
    /// Obtiene todos los clientes con filtro opcional
    /// </summary>
    public async Task<IEnumerable<Cliente>> ObtenerTodosClientesAsync(string palabraClave = null)
    {
        var query = _dbContext.Clientes
            .Include(c => c.TipoCliente)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(palabraClave))
        {
            var keyword = palabraClave.ToLower();
            query = query.Where(c =>
                c.Nombre.ToLower().Contains(keyword) ||
                c.Descripcion.ToLower().Contains(keyword) ||
                c.Direccion.ToLower().Contains(keyword));
        }

        return await query
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene clientes para dropdown
    /// </summary>
    public async Task<IEnumerable<SelectListItem>> ObtenerClientesParaDropdownAsync()
    {
        return await _dbContext.Clientes
            .OrderBy(c => c.Nombre)
            .Select(c => new SelectListItem(c.Nombre, c.Id.ToString()))
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene un cliente por ID (sin relaciones)
    /// </summary>
    public async Task<Cliente> ObtenerClientePorIdAsync(Guid id)
    {
        return await _dbContext.Clientes.FindAsync(id);
    }

    /// <summary>
    /// Obtiene un cliente por ID con todas sus relaciones para vista de detalle
    /// </summary>
    public async Task<Cliente> ObtenerClienteDetalleAsync(Guid id)
    {
        return await _dbContext.Clientes
            .Include(c => c.TipoCliente)
            .Include(c => c.Contratos)
                .ThenInclude(c => c.Proyectos)
                .ThenInclude(p => p.Area)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <summary>
    /// Crea un nuevo cliente con su contrato inicial
    /// </summary>
    public async Task<(bool Success, Cliente Data, string Error)> CrearClienteAsync(
        Cliente cliente,
        string usuarioActual)
    {
        try
        {
            var primeraArea = await _dbContext.Areas.FirstOrDefaultAsync();
            if (primeraArea == null)
            {
                return (false, null, "No hay áreas disponibles para crear el contrato");
            }

            cliente.Nombre = cliente.Nombre?.Trim();
            cliente.Descripcion = cliente.Descripcion?.Trim();
            cliente.Direccion = cliente.Direccion?.Trim();

            cliente.RegristrarCreacion(usuarioActual, DateTime.UtcNow);
            await _dbContext.Clientes.AddAsync(cliente);

            var contrato = new Contrato
            {
                IdCliente = cliente.Id,
                Identificacion = "CONTRATO-" + cliente.Id,
                Descripcion = "Contrato de " + cliente.Nombre,
                FechaInicio = DateTime.UtcNow,
                IdArea = primeraArea.Id
            };
            contrato.RegristrarCreacion(usuarioActual, DateTime.UtcNow);
            await _dbContext.Contratos.AddAsync(contrato);

            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return (true, cliente, null);
            }

            return (false, null, "No se pudo guardar el cliente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cliente");
            return (false, null, "Error al crear el cliente");
        }
    }

    /// <summary>
    /// Actualiza un cliente existente
    /// </summary>
    public async Task<(bool Success, string Error)> ActualizarClienteAsync(
        Cliente clienteActualizado,
        string usuarioActual)
    {
        try
        {
            var cliente = await _dbContext.Clientes.FindAsync(clienteActualizado.Id);

            if (cliente == null)
            {
                return (false, "Cliente no encontrado");
            }

            cliente.Actualizar(clienteActualizado, usuarioActual);
            _dbContext.Clientes.Update(cliente);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return (true, null);
            }

            return (false, "No se pudo actualizar el cliente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cliente con ID: {Id}", clienteActualizado.Id);
            return (false, "Error al actualizar el cliente");
        }
    }

    /// <summary>
    /// Elimina (soft delete) un cliente
    /// </summary>
    public async Task<(bool Success, string Error)> EliminarClienteAsync(Guid id, string usuarioActual)
    {
        try
        {
            var cliente = await _dbContext.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return (false, "Cliente no encontrado");
            }

            if (cliente.IsDeleted)
            {
                return (false, "El cliente ya ha sido eliminado");
            }

            cliente.Eliminar(usuarioActual);
            _dbContext.Clientes.Update(cliente);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return (true, null);
            }

            return (false, "No se pudo eliminar el cliente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cliente con ID: {Id}", id);
            return (false, "Error al eliminar el cliente");
        }
    }

    #endregion

    #region Contratos

    /// <summary>
    /// Obtiene todos los contratos con sus relaciones
    /// </summary>
    public async Task<IEnumerable<Contrato>> ObtenerTodosContratosAsync()
    {
        return await _dbContext.Contratos
            .Include(c => c.Cliente)
            .Include(c => c.Area)
            .OrderBy(c => c.Cliente.Nombre)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene contratos para dropdown
    /// </summary>
    public async Task<IEnumerable<SelectListItem>> ObtenerContratosParaDropdownAsync()
    {
        return await _dbContext.Contratos
            .Include(c => c.Cliente)
            .OrderBy(c => c.Cliente.Nombre)
            .Select(c => new SelectListItem(
                $"{c.Cliente.Nombre} - {c.Identificacion}",
                c.Id.ToString()))
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene un contrato por ID (sin relaciones)
    /// </summary>
    public async Task<Contrato> ObtenerContratoPorIdAsync(Guid id)
    {
        return await _dbContext.Contratos.FindAsync(id);
    }

    /// <summary>
    /// Obtiene un contrato por ID con Cliente para vista de eliminación
    /// </summary>
    public async Task<Contrato> ObtenerContratoConClienteAsync(Guid id)
    {
        return await _dbContext.Contratos
            .Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <summary>
    /// Obtiene un contrato por ID con todas sus relaciones para vista de detalle
    /// </summary>
    public async Task<Contrato> ObtenerContratoDetalleAsync(Guid id)
    {
        return await _dbContext.Contratos
            .Include(c => c.Cliente)
            .Include(c => c.Area)
            .Include(c => c.Proyectos)
                .ThenInclude(p => p.Area)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <summary>
    /// Crea un nuevo contrato
    /// </summary>
    public async Task<(bool Success, Contrato Data, string Error)> CrearContratoAsync(
        Contrato contrato,
        string usuarioActual)
    {
        try
        {
            contrato.RegristrarCreacion(usuarioActual, DateTime.UtcNow);
            await _dbContext.Contratos.AddAsync(contrato);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return (true, contrato, null);
            }

            return (false, null, "No se pudo guardar el contrato");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear contrato");
            return (false, null, "Error al crear el contrato");
        }
    }

    /// <summary>
    /// Actualiza un contrato existente
    /// </summary>
    public async Task<(bool Success, string Error)> ActualizarContratoAsync(
        Contrato contratoActualizado,
        string usuarioActual)
    {
        try
        {
            var contrato = await _dbContext.Contratos.FindAsync(contratoActualizado.Id);

            if (contrato == null)
            {
                return (false, "Contrato no encontrado");
            }

            contrato.Actualizar(contratoActualizado, usuarioActual);
            _dbContext.Contratos.Update(contrato);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return (true, null);
            }

            return (false, "No se pudo actualizar el contrato");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar contrato con ID: {Id}", contratoActualizado.Id);
            return (false, "Error al actualizar el contrato");
        }
    }

    /// <summary>
    /// Elimina (soft delete) un contrato
    /// </summary>
    public async Task<(bool Success, string Error)> EliminarContratoAsync(Guid id, string usuarioActual)
    {
        try
        {
            var contrato = await _dbContext.Contratos.FindAsync(id);

            if (contrato == null)
            {
                return (false, "Contrato no encontrado");
            }

            if (contrato.IsDeleted)
            {
                return (false, "El contrato ya ha sido eliminado");
            }

            // Verificar si tiene proyectos activos
            var tieneProyectos = await _dbContext.Proyectos
                .AnyAsync(p => p.IdContrato == id && !p.IsDeleted);

            if (tieneProyectos)
            {
                return (false, "No se puede eliminar el contrato porque tiene proyectos activos");
            }

            contrato.Eliminar(usuarioActual);
            _dbContext.Contratos.Update(contrato);
            int changes = await _dbContext.SaveChangesAsync();

            if (changes > 0)
            {
                return (true, null);
            }

            return (false, "No se pudo eliminar el contrato");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar contrato con ID: {Id}", id);
            return (false, "Error al eliminar el contrato");
        }
    }

    #endregion
}