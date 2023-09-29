using Serilog;
using SharedLibrary.Models;
using OrderServer.API.Models;
using SharedLibrary.Extentions;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Abstract;
using SharedLibrary.UnitOfWork.Abstract;
using OrderServer.API.Services.Abstract;
using OrderServer.API.Services.Concrete;
using SharedLibrary.UnitOfWork.Concrete;
using OrderServer.API.BackgroundServices;
using SharedLibrary.Repositories.Abstract;
using OrderServer.API.Repositories.Concrete;
using SharedLibrary.Services.RabbitMqCustom;

namespace OrderServer.API.Extentions;

public static class StartUpExtention
{
	public static void AddScopeExtention(this IServiceCollection services)
	{
		services.AddScoped<IUnitOfWork, UnitOfWork<AppDbContext>>();
		services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
		services.AddScoped(typeof(IServiceGeneric<,>), typeof(ServiceGeneric<,>));
		services.AddScoped<IOrderService, OrderServiceForController>();
		services.AddScoped(typeof(RabbitMQPublisher<>));
	}

	public static void AddTransientWithExtention(this IServiceCollection services)
	{
		services.AddTransient<RabbitMQPublisher<OutBox>>();
	}

	public static void OtherAdditions(this IServiceCollection services)
	{
		// BackGroundService elave edirik projecte
		services.AddHostedService<DeliveryOrderBackgroundService>();
		services.AddHostedService<OutBoxBackground>();

		services.UseCustomValidationResponse();
		services.AddAuthorization();
	}

	public static void AddDbContextExtentions(this IServiceCollection services, IConfiguration configuration)
	{
		// Connectioni veririk
		services.AddDbContext<AppDbContext>(options =>
		{
			options.UseSqlServer(configuration.GetConnectionString("SqlServer"), opt => opt.EnableRetryOnFailure());
		});
	}

	public static void AddHttpClientExtention(this IServiceCollection services, IConfiguration configuration)
	{
		// AuthService -in methoduna müraciet 
		services.AddHttpClient("AuthServer", client =>
		{
			client.BaseAddress = new Uri(configuration["Microservices:AuthServiceBaseUrl"]);
		});
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
