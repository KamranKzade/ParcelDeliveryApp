using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AuthServer.API.Models;

public class AppDbContext : IdentityDbContext<UserApp, IdentityRole, string>
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserRefleshToken> UserRefleshTokens { get; set; }

	// Configurationlari Model olaraq yaradiriq, hamisi 1 yerde
	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
		base.OnModelCreating(builder);
	}
}
