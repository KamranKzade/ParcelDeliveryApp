using SharedLibrary.Extentions;
using DeliveryServer.API.Models;
using SharedLibrary.Configuration;
using Microsoft.EntityFrameworkCore;
using DeliveryServer.API.Services.Abstract;
using DeliveryServer.API.Services.Concrete;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<AppDbContext>(opts =>
{
	opts.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddScoped<IDeliveryService, DeliveryService>();

builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOptions"));
var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<CustomTokenOption>();
builder.Services.AddCustomTokenAuth(tokenOptions);
builder.Services.UseCustomValidationResponse();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
