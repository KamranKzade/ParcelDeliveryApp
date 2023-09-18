using SharedLibrary.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthServer.API.Models;

public class AppDbContext : IdentityDbContext<UserApp, IdentityRole, string>
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

	public DbSet<LogEntry> LogEntries { get; set; }
	public DbSet<UserRefleshToken> UserRefleshTokens { get; set; }


	// Configurationlari Model olaraq yaradiriq, hamisi 1 yerde
	protected override void OnModelCreating(ModelBuilder builder)
	{

		builder.ApplyConfigurationsFromAssembly(GetType().Assembly);		
		base.OnModelCreating(builder);
	}
}

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	public AppDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
		optionsBuilder.UseSqlServer("SqlServer");

		return new AppDbContext(optionsBuilder.Options);
	}
}
