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

builder.Services.AddCors(opts =>
{
	opts.AddPolicy("corsapp",
		builder =>
		{
			builder.WithOrigins("*")
			.AllowAnyHeader()
			.AllowAnyHeader();
		});
});


var app = builder.Build();

app.Services.AddMigrationWithExtention();

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
app.UseCors("corsapp");
app.UseAuthorization();

app.MapControllers();

app.Run();