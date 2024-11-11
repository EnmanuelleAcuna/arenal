using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using arenal.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace arenal.Models.Core;

public class Ejercicios : IBaseCore<Ejercicio>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly IBaseCore<TipoEjercicio> _tiposEjercicio;

	public Ejercicios(ApplicationDbContext dbContext, IBaseCore<TipoEjercicio> tiposEjercicio)
	{
		_dbContext = dbContext;
		_tiposEjercicio = tiposEjercicio;
	}

	public async Task<IList<Ejercicio>> ReadAllAsync()
	{
		var ejercicios = await _dbContext.Ejercicios.Include(z => z.TipoEjercicio).ToListAsync();
		return ejercicios ?? new List<Ejercicio>();
	}

	public async Task<Ejercicio> ReadByIdAsync(Guid id)
	{
		var ejercicio = await _dbContext.Ejercicios.Include(z => z.TipoEjercicio).FirstOrDefaultAsync(z => z.Id == id);
		return ejercicio ?? throw new KeyNotFoundException($"No se encontró un ejercicio con el id {id}");
	}

	public async Task CreateAsync(Ejercicio ejercicio, string user)
	{
		var existingTipoEjercicio = await _tiposEjercicio.ReadByIdAsync(ejercicio.IdTipoEjercicio);

		if (existingTipoEjercicio == null)
			throw new Exception($"El tipo de ejercicio {ejercicio.IdTipoEjercicio} para el ejercicio no se ha encontrado en la BD.");
		else
			ejercicio.TipoEjercicio = existingTipoEjercicio;

		ejercicio.CreatedBy = user;
		ejercicio.DateCreated = DateTime.Now;

		await _dbContext.AddAsync(ejercicio);
		await _dbContext.SaveChangesAsync();
	}

	public async Task UpdateAsync(Ejercicio ejercicio, string user)
	{
		var record = await ReadByIdAsync(ejercicio.Id);

		record.Codigo = ejercicio.Codigo;
		record.Nombre = ejercicio.Nombre;
		record.Estado = ejercicio.Estado;
		record.IdTipoEjercicio = ejercicio.IdTipoEjercicio;

		record.UpdatedBy = user;
		record.DateUpdated = DateTime.Now;

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

public class TiposEjercicio : IBaseCore<TipoEjercicio>
{
	private readonly ApplicationDbContext _dbContext;

	public TiposEjercicio(ApplicationDbContext dbContext) => _dbContext = dbContext;

	public async Task<IList<TipoEjercicio>> ReadAllAsync()
	{
		var tiposEjercicio = await _dbContext.TiposEjercicio.ToListAsync();
		return tiposEjercicio ?? new List<TipoEjercicio>();
	}

	public async Task<TipoEjercicio> ReadByIdAsync(Guid id)
	{
		TipoEjercicio tipoEjercicio = await _dbContext.TiposEjercicio.FindAsync(id);

		if (tipoEjercicio == null)
			throw new KeyNotFoundException($"No se encontró el tipo  de ejercicio con el id {id}");

		return tipoEjercicio;
	}

	public async Task CreateAsync(TipoEjercicio tipoEjercicio, string user)
	{
		tipoEjercicio.DateCreated = DateTime.Now;
		tipoEjercicio.CreatedBy = user;

		await _dbContext.AddAsync(tipoEjercicio);
		await _dbContext.SaveChangesAsync();
	}

	public async Task UpdateAsync(TipoEjercicio tipoEjercicio, string user)
	{
		TipoEjercicio record = await ReadByIdAsync(tipoEjercicio.Id);

		if (record == null)
			throw new KeyNotFoundException($"No se encontró el tipo  de ejercicio con el id {tipoEjercicio.Id}");

		record.Codigo = tipoEjercicio.Codigo;
		record.Nombre = tipoEjercicio.Nombre;
		record.Estado = tipoEjercicio.Estado;

		record.DateUpdated = DateTime.Now;
		record.UpdatedBy = user;

		_dbContext.Update(record);
		await _dbContext.SaveChangesAsync();
	}

	public async Task DeleteAsync(Guid id)
	{
		TipoEjercicio record = await ReadByIdAsync(id);

		if (record == null)
			throw new KeyNotFoundException($"No se encontró el tipo  de ejercicio con el id {id}");

		_dbContext.Remove(record);
		await _dbContext.SaveChangesAsync();
	}
}
