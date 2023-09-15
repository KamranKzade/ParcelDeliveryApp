using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.UnitOfWork.Abstract;

namespace SharedLibrary.UnitOfWork.Concrete;


public class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
	private readonly TContext _dbContext;
	private readonly ILogger<TContext> _logger;
	public UnitOfWork(TContext dbContext, ILogger<TContext> logger)
	{
		_dbContext = dbContext;
		_logger = logger;
	}

	public void Commit()
	{
		try
		{
			_dbContext.SaveChanges();
			_logger.LogInformation("Changes to the database were successfully saved.");
		}
		catch (Exception ex)
		{
			// Handle specific DbUpdateException (Entity Framework related)
			_logger.LogError(ex, "Error while committing changes to the database");
			throw;
		}
	}

	public async Task CommitAsync()
	{
		try
		{
			await _dbContext.SaveChangesAsync();
			_logger.LogInformation("Changes to the database were successfully saved.");
		}
		catch (Exception ex)
		{
			// Handle specific DbUpdateException (Entity Framework related)
			_logger.LogError(ex, "Error while committing changes to the database");
			throw;
		}
	}
}
