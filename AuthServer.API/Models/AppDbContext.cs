using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SharedLibrary.Models;

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

		// LogEntry tablosunu özelleştirmek için
		builder.Entity<LogEntry>().HasKey(l => l.Id); // Primary key olarak Id kullan
		builder.Entity<LogEntry>().Property(l => l.Id).ValueGeneratedOnAdd(); // Id otomatik artan olarak ayarla
		builder.Entity<LogEntry>().Property(l => l.MessageTemplate).IsRequired().HasMaxLength(255); // Message özelliği zorunlu ve maksimum 255 karakter uzunluğunda
		builder.Entity<LogEntry>().Property(l => l.Timestamp).IsRequired(); // Timestamp özelliği zorunlu
		builder.Entity<LogEntry>().Property(l => l.Level).IsRequired().HasMaxLength(50); // LogLevel özelliği zorunlu ve maksimum 50 karakter uzunluğunda
		builder.Entity<LogEntry>().Property(l => l.Exception).HasMaxLength(255); // Logger özelliği maksimum 255 karakter uzunluğunda
		builder.Entity<LogEntry>().Property(l => l.Properties).HasMaxLength(256); // UserName özelliği maksimum 256 karakter uzunluğunda

		base.OnModelCreating(builder);
	}
}
