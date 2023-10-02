using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace SharedLibrary.Repositories.Abstract;


public interface IGenericRepository<TContext,TEntity> where TEntity : class where TContext : DbContext
{
	Task<TEntity> GetByIdAsync(string id);
	Task<IEnumerable<TEntity>> GetAllAsync();
	IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
	Task AddAsync(TEntity entity);
	TEntity UpdateAsync(TEntity entity);
	void Remove(TEntity entity);
}
