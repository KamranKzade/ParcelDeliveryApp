using RabbitMQ.Client;
using SharedLibrary.Extentions;
using DeliveryServer.API.Models;
using SharedLibrary.Configuration;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Abstract;
using SharedLibrary.UnitOfWork.Abstract;
using SharedLibrary.Repositories.Abstract;
using DeliveryServer.API.Services.Abstract;
using DeliveryServer.API.Services.Concrete;
using DeliveryServer.API.UnitOfWork.Concrete;
using DeliveryServer.API.Repositories.Concrete;
using SharedLibrary.Services.RabbitMqCustom;
using SharedLibrary.Models;

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

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IServiceGeneric<,>), typeof(ServiceGeneric<,>));
builder.Services.AddScoped<IDeliveryService, DeliveryService>();

builder.Services.AddSingleton(sp => new ConnectionFactory
{
	Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")),
	DispatchConsumersAsync = true
});
builder.Services.AddSingleton<RabbitMQPublisher<OrderDelivery>>();
builder.Services.AddSingleton<RabbitMQClientService>();


builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOptions"));
var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<CustomTokenOption>();
builder.Services.AddCustomTokenAuth(tokenOptions);
builder.Services.UseCustomValidationResponse();
builder.Services.AddAuthorization();

// AuthService -in methoduna müraciet 
builder.Services.AddHttpClient("OrderService", client =>
{
	client.BaseAddress = new Uri(builder.Configuration["OrderServiceBaseUrl"]);
});


var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
// 	app.UseSwagger();
// 	app.UseSwaggerUI();
// }

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
