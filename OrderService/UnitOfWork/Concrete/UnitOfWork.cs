using OrderServer.API.Models;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.UnitOfWork.Abstract;

namespace OrderServer.API.UnitOfWork.Concrete;

public class UnitOfWork : IUnitOfWork
{
	private readonly DbContext _dbContext;
	private readonly ILogger<UnitOfWork> _logger;
	public UnitOfWork(AppDbContext dbContext, ILogger<UnitOfWork> logger)
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
