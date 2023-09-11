using OrderService.API.Models;
using SharedLibrary.Extentions;
using SharedLibrary.Configuration;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Abstract;
using SharedLibrary.UnitOfWork.Abstract;
using OrderService.API.Services.Abstract;
using OrderService.API.Services.Concrete;
using SharedLibrary.Repositories.Abstract;
using OrderService.API.UnitOfWork.Concrete;
using OrderService.API.Repositories.Concrete;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connectioni veririk
builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

//builder.Services.AddScoped<AppDbContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IServiceGeneric<,>), typeof(ServiceGeneric<,>));
builder.Services.AddScoped<IOrderService, OrderServiceForController>();

builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOptions"));
var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<CustomTokenOption>();
builder.Services.AddCustomTokenAuth(tokenOptions);
builder.Services.UseCustomValidationResponse();
builder.Services.AddAuthorization();
//builder.Services.AddHttpClient<AuthServer.API.Controllers.AuthController>();
builder.Services.AddHttpClient("AuthServer", client =>
{
	client.BaseAddress = new Uri(builder.Configuration["AuthServiceBaseUrl"]);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
