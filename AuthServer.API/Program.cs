using AuthServer.API.Extentions;

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
