using System.Reflection;
using AuthServer.API.Models;
using SharedLibrary.Extentions;
using AuthServer.API.Extentions;
using SharedLibrary.Configuration;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using AuthServer.API.Configurations;
using AuthServer.API.UnitOfWork.Abstract;
using AuthServer.API.UnitOfWork.Concrete;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
				.AddFluentValidation(opts => // FluentValidationlari sisteme tanidiriq
				{
					opts.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
				});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI lari sisteme tanitmaq
builder.Services.AddScopeWithExtention();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Connectioni veririk
builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

// Identity-ni sisteme tanidiriq
builder.Services.AddIdentityWithExtention();


builder.Services.Configure<List<Client>>(builder.Configuration.GetSection("Clients"));

// TokenOption elave edirik Configure-a 
// Option pattern --> DI uzerinden appsetting-deki datalari elde etmeye deyilir.
builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOptions"));
var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<CustomTokenOption>();

builder.Services.AddCustomTokenAuth(tokenOptions);


// Validationlari 1 yere yigib qaytaririq
builder.Services.UseCustomValidationResponse();

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
