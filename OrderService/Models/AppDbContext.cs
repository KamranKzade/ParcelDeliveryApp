using SharedLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace OrderServer.API.Models;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> dbContext) : base(dbContext) { }

	public DbSet<OrderDelivery> Orders { get; set; }
	public DbSet<LogEntry> LogEntries { get; set; }

}
