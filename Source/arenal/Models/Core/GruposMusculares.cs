using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using arenal.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace arenal.Models.Core;

public class GruposMusculares : IBaseCore<GrupoMuscular>
{
	private readonly ApplicationDbContext _dbContext;

	public GruposMusculares(ApplicationDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<IList<GrupoMuscular>> ReadAllAsync()
	{
		var gruposMusculares = await _dbContext.GruposMusculares.ToListAsync();
		return gruposMusculares ?? new List<GrupoMuscular>();
	}

	public async Task<GrupoMuscular> ReadByIdAsync(Guid id)
	{
		var grupomuscular = await _dbContext.GruposMusculares.FindAsync(id);

		if (grupomuscular == null)
			throw new KeyNotFoundException($"No se encontró el grupo muscular con el id: {id}");

		return grupomuscular;
	}

	public async Task CreateAsync(GrupoMuscular grupoMuscular, string user)
	{
		grupoMuscular.CreatedBy = user;
		grupoMuscular.DateCreated = DateTime.UtcNow;

		await _dbContext.AddAsync(grupoMuscular);
		await _dbContext.SaveChangesAsync();
	}

	public async Task UpdateAsync(GrupoMuscular grupoMuscular, string user)
	{
		GrupoMuscular record = await ReadByIdAsync(grupoMuscular.Id);

		record.Nombre = grupoMuscular.Nombre;
		record.Descripcion = grupoMuscular.Descripcion;

		record.UpdatedBy = user;
		record.DateUpdated = DateTime.UtcNow;

		_dbContext.GruposMusculares.Update(record);
		await _dbContext.SaveChangesAsync();
	}

	public async Task DeleteAsync(Guid id)
	{
		GrupoMuscular record = await ReadByIdAsync(id);

		_dbContext.GruposMusculares.Remove(record);
		await _dbContext.SaveChangesAsync();
	}
}
