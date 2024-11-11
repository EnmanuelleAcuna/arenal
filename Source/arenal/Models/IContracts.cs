using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using arenal.Models.Entities;
using arenal.Models.ViewModels;

namespace arenal.Models;

public interface IBaseCore<T>
{
	Task<IList<T>> ReadAllAsync();
	Task<T> ReadByIdAsync(Guid id);
	Task CreateAsync(T model, string user);
	Task UpdateAsync(T model, string user);
	Task DeleteAsync(Guid id);
}
