﻿using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using AuthServer.API.Repository.Abstract;

namespace AuthServer.API.Repository.Concrete;

public class GenericRepository<Tentity> : IGenericRepository<Tentity> where Tentity : class
{
	private readonly DbContext _dbContext;
	private readonly DbSet<Tentity> _dbSet;

	public GenericRepository(DbContext dbContext, DbSet<Tentity> dbSet)
	{
		_dbContext = dbContext;
		_dbSet = dbSet;
	}

	public async Task AddAsync(Tentity entity)
	{
		await _dbSet.AddAsync(entity);
	}

	public async Task<IEnumerable<Tentity>> GetAllAsync()
	{
		return await _dbSet.ToListAsync();
	}

	public async Task<Tentity> GetByIdAsync(int id)
	{
		var entity = await _dbSet.FindAsync(id);

		if (entity != null)
		{
			_dbContext.Entry(entity).State = EntityState.Detached;
		}
		return entity!;
	}

	public void Remove(Tentity entity)
	{
		_dbSet.Remove(entity);
	}

	public Tentity UpdateAsync(Tentity entity)
	{
		_dbContext.Entry(entity).State=EntityState.Modified;
		return entity;
	}

	public IQueryable<Tentity> Where(Expression<Func<Tentity, bool>> predicate)
	{
		return _dbSet.Where(predicate);
	}
}