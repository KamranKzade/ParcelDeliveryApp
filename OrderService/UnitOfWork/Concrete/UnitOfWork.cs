using OrderService.Models;
using Microsoft.EntityFrameworkCore;
using OrderService.UnitOfWork.Abstract;

namespace OrderService.UnitOfWork.Concrete;

public class UnitOfWork : IUnitOfWork
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
