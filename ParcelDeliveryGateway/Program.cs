using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace ParcelDeliveryGateway;

public class Program
{
	public static void Main(string[] args)
	{
		CreateHostBuilder(args).Build().Run();
	}

	public static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
		.ConfigureAppConfiguration((hostingContext, config) =>
		{
			// Configuration file-larini aliriq
			config.AddJsonFile($"configuration.{hostingContext.HostingEnvironment.EnvironmentName.ToLower()}.json")
				  .AddEnvironmentVariables();
		})
		.ConfigureWebHostDefaults(webBuilder =>
		{
			webBuilder.UseStartup<Startup>();
		});


}
