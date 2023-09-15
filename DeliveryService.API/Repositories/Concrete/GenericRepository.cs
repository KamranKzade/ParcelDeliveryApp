using System.Linq.Expressions;
using DeliveryServer.API.Models;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Repositories.Abstract;

namespace DeliveryServer.API.Repositories.Concrete;


public class GenericRepository<Tentity> : IGenericRepository<Tentity> where Tentity : class
{
	private readonly DbContext _dbContext;
	private readonly DbSet<Tentity> _dbSet;
	private readonly ILogger<GenericRepository<Tentity>> _logger;

	public GenericRepository(AppDbContext dbContext, ILogger<GenericRepository<Tentity>> logger)
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
			return entity;
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
			_logger.LogInformation($"Updating entity of type {entity.GetType().Name} with ID {entity}");
			_dbContext.Entry(entity).State = EntityState.Modified;
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
