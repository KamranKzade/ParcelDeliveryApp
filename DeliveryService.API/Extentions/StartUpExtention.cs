using RabbitMQ.Client;
using SharedLibrary.Models;
using SharedLibrary.Extentions;
using DeliveryServer.API.Models;
using SharedLibrary.Configuration;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Abstract;
using SharedLibrary.UnitOfWork.Abstract;
using SharedLibrary.Repositories.Abstract;
using DeliveryServer.API.Services.Abstract;
using DeliveryServer.API.Services.Concrete;
using SharedLibrary.Services.RabbitMqCustom;

namespace DeliveryServer.API.Extentions;

public static class StartUpExtention
{
	public static void AddScopeWithExtention(this IServiceCollection services)
	{

		services.AddScoped<IUnitOfWork, UnitOfWork.Concrete.UnitOfWork>();
		services.AddScoped(typeof(IGenericRepository<>), typeof(Repositories.Concrete.GenericRepository<>));
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
}
