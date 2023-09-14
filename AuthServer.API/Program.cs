using AuthServer.API.Extentions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithExtention();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScopeWithExtention();
builder.Services.AddDbContextWithExtention(builder.Configuration);
builder.Services.AddIdentityWithExtention();
builder.Services.OtherAdditionWithExtention(builder.Configuration);
builder.Services.AddCustomTokenAuthWithExtention(builder.Configuration);

Log.Logger = new LoggerConfiguration()
					.ReadFrom.Configuration(builder.Configuration)
					.WriteTo.MSSqlServer(
									connectionString: builder.Configuration.GetConnectionString("SqlServer"),
									tableName: "LogEntries",
									autoCreateSqlTable: true,
									restrictedToMinimumLevel: LogEventLevel.Information
								)
						.Filter.ByExcluding(e => e.Level < LogEventLevel.Information) // Sadece Information ve daha yüksek seviyedeki logları kaydet
				   .CreateLogger();

// Log.Logger = new LoggerConfiguration()
// 			.ReadFrom.Configuration(builder.Configuration)
// 			.WriteTo.File("logs.txt", rollingInterval: RollingInterval.Day) // Dosya adı ve günlük günlük dosya dönemini belirleyin
// 			.Filter.ByExcluding(e => e.Level < LogEventLevel.Information) // Sadece Information ve daha yüksek seviyedeki logları kaydet
// 			.CreateLogger();

builder.Services.AddLogging(loggingBuilder =>
{
	loggingBuilder.AddSerilog(); // Serilog'u kullanarak loglama
});


var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
// 	app.UseDeveloperExceptionPage();
// 	app.UseSwagger();
// 	app.UseSwaggerUI();
// }
//

// Productionda da gorunmeyin isteyirikse swaggerin bele yazmaliyiq
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
