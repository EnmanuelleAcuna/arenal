namespace arenal.Domain;

public interface IBaseCore<T>
{
	Task<IList<T>> ReadAllAsync();
	Task<T> ReadByIdAsync(Guid id);
	Task CreateAsync(T model, string user);
	Task UpdateAsync(T model, string user);
	Task DeleteAsync(Guid id);
}
