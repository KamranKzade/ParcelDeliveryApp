using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Repositories.Abstract;
using Microsoft.Extensions.Logging;

namespace OrderServer.API.Repositories.Concrete;

public class GenericRepository<TContext, Tentity> : IGenericRepository<TContext, Tentity> where Tentity : class where TContext : DbContext
{
	private readonly TContext _dbContext;
	private readonly DbSet<Tentity> _dbSet;
	private readonly ILogger<GenericRepository<TContext, Tentity>> _logger;

	public GenericRepository(TContext dbContext, ILogger<GenericRepository<TContext, Tentity>> logger)
	{
		_dbContext = dbContext;
		_dbSet = _dbContext.Set<Tentity>();
		_logger = logger;
	}


	public async Task AddAsync(Tentity entity)
	{
		try
		{
			_logger.LogInformation($"It was successfully added: {entity}");
			await _dbSet.AddAsync(entity);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while adding entity");
			throw;
		}
	}

	public async Task<IEnumerable<Tentity>> GetAllAsync()
	{
		try
		{
			_logger.LogInformation($"The data was successfully obtained");
			return await _dbSet.ToListAsync();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex.Message, "Error while getting all entities");
			throw;
		}
	}

	public async Task<Tentity> GetByIdAsync(string id)
	{
		try
		{
			var entity = await _dbSet.FindAsync(id);

			if (entity != null)
			{
				_dbContext.Entry(entity).State = EntityState.Detached;
			}

			_logger.LogInformation($"{entity} returned successfully");
			return entity!;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while getting entity by id");
			throw;
		}
	}

	public void Remove(Tentity entity)
	{
		try
		{
			_logger.LogInformation($"Removing entity of type {entity.GetType().Name} with ID {entity}");
			_dbSet.Remove(entity);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while removing entity");
			throw;
		}
	}

	public Tentity UpdateAsync(Tentity entity)
	{
		try
		{
			_dbContext.Entry(entity).State = EntityState.Modified;
			_logger.LogInformation($"Updating entity of type  with ID {entity}");
			return entity;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while updating entity");
			throw;
		}
	}

	public IQueryable<Tentity> Where(Expression<Func<Tentity, bool>> predicate)
	{
		try
		{
			_logger.LogInformation($"Constructing a LINQ query with predicate: {predicate}");
			return _dbSet.Where(predicate);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error while executing Where query");
			throw;
		}
	}
}
