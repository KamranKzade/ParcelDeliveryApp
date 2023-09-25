using Serilog;
using Serilog.Events;
using RabbitMQ.Client;
using SharedLibrary.Models;
using SharedLibrary.Configuration;
using Microsoft.Extensions.Configuration;
using SharedLibrary.Services.RabbitMqCustom;
using Microsoft.Extensions.DependencyInjection;

namespace SharedLibrary.Extentions;

public static class StartUpExtention
{
	public static void AddSingletonWithExtentionShared(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddSingleton(sp => new ConnectionFactory
		{
			Uri = new Uri(configuration.GetConnectionString("RabbitMQ")),
			DispatchConsumersAsync = true
		});
		// bura fikir ver

		services.AddTransient<RabbitMQPublisher<OrderDelivery>>();
		services.AddTransient<RabbitMQClientService>();
	}

	public static void AddCustomTokenAuthWithExtentionShared(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<CustomTokenOption>(configuration.GetSection("TokenOptions"));
		var tokenOptions = configuration.GetSection("TokenOptions").Get<CustomTokenOption>();
		services.AddCustomTokenAuth(tokenOptions);
	}

	public static void AddLoggingWithExtentionShared(this IServiceCollection services, IConfiguration config)
	{
		Log.Logger = new LoggerConfiguration()
					.ReadFrom.Configuration(config)
					.WriteTo.Logger(lc => lc
					.WriteTo.File(
						path: "Logs/log.txt",
						restrictedToMinimumLevel: LogEventLevel.Information,
						rollingInterval: RollingInterval.Day
					))
					.Filter.ByExcluding(e => e.Level < LogEventLevel.Information) // Sadece Information ve daha yüksek seviyedeki logları kaydet
					.CreateLogger();

		services.AddLogging(loggingBuilder =>
		{
			loggingBuilder.AddSerilog();
		});
	}

	public static void AddCorsWithExtentionShared(this IServiceCollection services)
	{
		services.AddCors(opts =>
		{
			opts.AddPolicy("corsapp",
				builder =>
				{
					builder.WithOrigins("*")
					.AllowAnyHeader()
					.AllowAnyHeader();
				});
		});
	}
}
