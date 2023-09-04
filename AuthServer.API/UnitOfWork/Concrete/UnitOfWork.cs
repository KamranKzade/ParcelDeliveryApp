using AuthServer.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.API.UnitOfWork.Concrete;

public class UnitOfWork
{
	private readonly DbContext _dbContext;

	public UnitOfWork(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public void Commit()
	{
		_dbContext.SaveChanges();
	}

	public async Task CommitAsync()
	{
		await _dbContext.SaveChangesAsync();
	}
}