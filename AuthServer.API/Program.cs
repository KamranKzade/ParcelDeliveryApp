﻿using AuthServer.API.Extentions;
using AuthServer.API.Models;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddLoggingWithExtention(builder.Configuration);


var app = builder.Build();


try
{
	using (var scope = app.Services.CreateScope())
	{
		var someService = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	 	await someService.Database.MigrateAsync();
	}

}
catch (Exception e)
{

}
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
