using plani.Models;
using Microsoft.EntityFrameworkCore;
using plani.Models.Data;

namespace plani.Models;

public class ClientesManager : IBaseCore<Cliente>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly IBaseCore<TipoCliente> _tiposCliente;

	public ClientesManager(ApplicationDbContext dbContext, IBaseCore<TipoCliente> tiposCliente)
	{
		_dbContext = dbContext;
		_tiposCliente = tiposCliente;
	}

	public async Task<IList<Cliente>> ReadAllAsync()
	{
		var ejercicios = await _dbContext.Clientes.Include(z => z.TipoCliente).ToListAsync();
		return ejercicios ?? new List<Cliente>();
	}

	public async Task<Cliente> ReadByIdAsync(Guid id)
	{
		var ejercicio = await _dbContext.Clientes.Include(z => z.TipoCliente).FirstOrDefaultAsync(z => z.Id == id);
		return ejercicio ?? throw new KeyNotFoundException($"No se encontró un ejercicio con el id {id}");
	}

	public async Task CreateAsync(Cliente cliente, string user)
	{
		var existingTipoEjercicio = await _tiposCliente.ReadByIdAsync(cliente.IdTipoCliente);

		if (existingTipoEjercicio == null)
			throw new Exception($"El tipo de ejercicio {cliente.IdTipoCliente} para el ejercicio no se ha encontrado en la BD.");
		else
			cliente.TipoCliente = existingTipoEjercicio;

		cliente.RegristrarCreacion(user, DateTime.Now);

		await _dbContext.AddAsync(cliente);
		await _dbContext.SaveChangesAsync();
	}

	public async Task UpdateAsync(Cliente cliente, string user)
	{
		var record = await ReadByIdAsync(cliente.Id);

		record.RegistrarActualizacion(user, DateTime.Now);

		_dbContext.Update(record);
		await _dbContext.SaveChangesAsync();
	}

	public async Task DeleteAsync(Guid id)
	{
		var record = await ReadByIdAsync(id);

		_dbContext.Remove(record);
		await _dbContext.SaveChangesAsync();
	}
}
