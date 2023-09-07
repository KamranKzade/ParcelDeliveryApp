﻿using OrderService.API.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OrderService.API.Repositories.Abstract;

namespace OrderService.API.Repositories.Concrete;

public class GenericRepository<Tentity> : IGenericRepository<Tentity> where Tentity : class
{
	private readonly DbContext _dbContext;
	private readonly DbSet<Tentity> _dbSet;

	public GenericRepository(AppDbContext dbContext)
	{
		_dbContext = dbContext;
		_dbSet = _dbContext.Set<Tentity>();
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
		return entity;
	}

	public void Remove(Tentity entity)
	{
		_dbSet.Remove(entity);
	}

	public Tentity UpdateAsync(Tentity entity)
	{
		_dbContext.Entry(entity).State = EntityState.Modified;
		return entity;
	}

	public IQueryable<Tentity> Where(Expression<Func<Tentity, bool>> predicate)
	{
		return _dbSet.Where(predicate);
	}
}