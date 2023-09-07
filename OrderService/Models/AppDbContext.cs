using Microsoft.EntityFrameworkCore;

namespace OrderService.API.Models;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> dbContext) : base(dbContext) { }

	public DbSet<Order> Orders { get; set; }
}
