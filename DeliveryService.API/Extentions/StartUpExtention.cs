using Serilog;
using Serilog.Events;
using RabbitMQ.Client;
using SharedLibrary.Models;
using SharedLibrary.Extentions;
using DeliveryServer.API.Models;
using SharedLibrary.Configuration;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Abstract;
using SharedLibrary.UnitOfWork.Concrete;
using SharedLibrary.UnitOfWork.Abstract;
using SharedLibrary.Repositories.Abstract;
using DeliveryServer.API.Services.Abstract;
using DeliveryServer.API.Services.Concrete;
using SharedLibrary.Services.RabbitMqCustom;
using OrderServer.API.Repositories.Concrete;

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

	public static void AddSingletonWithExtention(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddSingleton(sp => new ConnectionFactory
		{
			Uri = new Uri(configuration.GetConnectionString("RabbitMQ")),
			DispatchConsumersAsync = true
		});
		services.AddSingleton<RabbitMQPublisher<OrderDelivery>>();
		services.AddSingleton<RabbitMQClientService>();
	}

	public static void AddCustomTokenAuthWithExtention(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<CustomTokenOption>(configuration.GetSection("TokenOptions"));
		var tokenOptions = configuration.GetSection("TokenOptions").Get<CustomTokenOption>();
		services.AddCustomTokenAuth(tokenOptions);
	}

	public static void AddHttpClientWithExtention(this IServiceCollection services, IConfiguration configuration)
	{
		// AuthService -in methoduna müraciet 
		services.AddHttpClient("OrderService", client =>
		{
			client.BaseAddress = new Uri(configuration["OrderServiceBaseUrl"]);
		});
	}

	public static void OtherAdditionWithExtention(this IServiceCollection services)
	{
		services.UseCustomValidationResponse();
		services.AddAuthorization();
	}

	public static void AddLoggingWithExtention(this IServiceCollection services, IConfiguration config)
	{
		Log.Logger = new LoggerConfiguration()
					.ReadFrom.Configuration(config)
					.WriteTo.MSSqlServer(
									connectionString: config.GetConnectionString("SqlServer"),
									tableName: "LogEntries",
									autoCreateSqlTable: true,
									restrictedToMinimumLevel: LogEventLevel.Information
								)
						.Filter.ByExcluding(e => e.Level < LogEventLevel.Information) // Sadece Information ve daha yüksek seviyedeki logları kaydet
				   .CreateLogger();


		services.AddLogging(loggingBuilder =>
		{
			loggingBuilder.AddSerilog(); // Serilog'u kullanarak loglama
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
