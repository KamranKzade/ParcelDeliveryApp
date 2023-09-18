using SharedLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderServer.API.Models;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> dbContext) : base(dbContext) { }

	public DbSet<OrderDelivery> Orders { get; set; }
	public DbSet<LogEntry> LogEntries { get; set; }
}

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	public AppDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
		optionsBuilder.UseSqlServer("SqlServer", opt => opt.EnableRetryOnFailure());

		return new AppDbContext(optionsBuilder.Options);
	}
}