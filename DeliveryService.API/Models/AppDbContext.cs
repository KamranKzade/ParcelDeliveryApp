using Microsoft.EntityFrameworkCore;

namespace DeliveryServer.API.Models;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

	public DbSet<OrderDelivery> OrderDeliveries { get; set; }
}
