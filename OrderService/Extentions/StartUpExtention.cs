using RabbitMQ.Client;
using SharedLibrary.Models;
using OrderServer.API.Models;
using SharedLibrary.Extentions;
using SharedLibrary.Configuration;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Abstract;
using SharedLibrary.UnitOfWork.Abstract;
using OrderServer.API.Services.Abstract;
using OrderServer.API.Services.Concrete;
using OrderServer.API.BackgroundServices;
using SharedLibrary.Repositories.Abstract;
using SharedLibrary.Services.RabbitMqCustom;

namespace OrderServer.API.Extentions;

public static class StartUpExtention
{
	public static void AddScopeExtention(this IServiceCollection services)
	{
		services.AddScoped<IUnitOfWork, UnitOfWork.Concrete.UnitOfWork>();
		services.AddScoped(typeof(IGenericRepository<>), typeof(Repositories.Concrete.GenericRepository<>));
		services.AddScoped(typeof(IServiceGeneric<,>), typeof(ServiceGeneric<,>));
		services.AddScoped<IOrderService, OrderServiceForController>();
	}

	public static void AddSingletonExtention(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddSingleton(sp => new ConnectionFactory
		{
			Uri = new Uri(configuration.GetConnectionString("RabbitMQ")),
			DispatchConsumersAsync = true
		});

		services.AddSingleton(typeof(RabbitMQPublisher<>).MakeGenericType(typeof(OrderDelivery)));
		services.AddSingleton<RabbitMQClientService>();
	}

	public static void AddCustomTokenAuthExtention(this IServiceCollection services, IConfiguration configuration)
	{
		// CustomToken Elave edirik Sisteme
		services.Configure<CustomTokenOption>(configuration.GetSection("TokenOptions"));
		var tokenOptions = configuration.GetSection("TokenOptions").Get<CustomTokenOption>();
		services.AddCustomTokenAuth(tokenOptions);
	}

	public static void OtherAdditions(this IServiceCollection services)
	{
		// BackGroundService elave edirik projecte
		services.AddHostedService<DeliveryOrderBackgroundService>();

		services.UseCustomValidationResponse();
		services.AddAuthorization();
	}

	public static void AddDbContextExtentions(this IServiceCollection services, IConfiguration configuration)
	{
		// Connectioni veririk
		services.AddDbContext<AppDbContext>(options =>
		{
			options.UseSqlServer(configuration.GetConnectionString("SqlServer"));
		});
	}

	public static void AddHttpClientExtention(this IServiceCollection services, IConfiguration configuration)
	{
		// AuthService -in methoduna müraciet 
		services.AddHttpClient("AuthServer", client =>
		{
			client.BaseAddress = new Uri(configuration["AuthServiceBaseUrl"]);
		});
	}
}
