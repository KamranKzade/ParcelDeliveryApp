using Ocelot.Middleware;
using Ocelot.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ParcelDeliveryGateway;

public class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddAuthentication().AddJwtBearer();

		services.AddOcelot();
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		await app.UseOcelot();
	}
}
