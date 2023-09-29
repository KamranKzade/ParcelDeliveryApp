using Serilog;
using SharedLibrary.Extentions;
using DeliveryServer.API.Models;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Abstract;
using SharedLibrary.UnitOfWork.Concrete;
using SharedLibrary.UnitOfWork.Abstract;
using SharedLibrary.Repositories.Abstract;
using DeliveryServer.API.Services.Abstract;
using DeliveryServer.API.Services.Concrete;
using OrderServer.API.Repositories.Concrete;
using DeliveryServer.API.BackgroundServices;

namespace DeliveryServer.API.Extentions;

public static class StartUpExtention
{
	public static void AddScopeWithExtention(this IServiceCollection services)
	{

		services.AddScoped<IUnitOfWork, UnitOfWork<AppDbContext>>();
		services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
		services.AddScoped(typeof(IServiceGeneric<,>), typeof(ServiceGeneric<,>));
		services.AddScoped<IDeliveryService, DeliveryService>();
	}

	public static void AddDbContextWithExtention(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<AppDbContext>(opts =>
		{
			opts.UseSqlServer(configuration.GetConnectionString("SqlServer"));
		});
	}

	public static void AddHttpClientWithExtention(this IServiceCollection services, IConfiguration configuration)
	{
		// AuthService -in methoduna müraciet 
		services.AddHttpClient("OrderService", client =>
		{
			client.BaseAddress = new Uri(configuration["MicroServices:OrderServiceBaseUrl"]);
		});
	}

	public static void OtherAdditionWithExtention(this IServiceCollection services)
	{
		// BackGroundService elave edirik projecte
		services.AddHostedService<DeliveryBackgroundService>();
		services.AddHostedService<OutBoxDeliveryBackgroundService>();

		services.UseCustomValidationResponse();
		services.AddAuthorization();
	}

	public static async void AddMigrationWithExtention(this IServiceProvider provider)
	{
		try
		{
			using (var scope = provider.CreateScope())
			{
				var someService = scope.ServiceProvider.GetRequiredService<AppDbContext>();
				await someService.Database.MigrateAsync();
			}

		}
		catch (Exception ex)
		{
			Log.Error($"Error: {ex.Message}");
		}
	}

}
