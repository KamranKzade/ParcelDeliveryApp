using SharedLibrary.Extentions;
using AuthServer.API.Extentions;
using AuthServer.API.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersExtention();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI lari sisteme tanitmaq
builder.Services.AddScopeWithExtention();

// Connectioni veririk
builder.Services.AddDbContextExtention(builder.Configuration);

// Identity-ni sisteme tanidiriq
builder.Services.AddIdentityWithExtention();

builder.Services.Configure<List<Client>>(builder.Configuration.GetSection("Clients"));

// TokenOption elave edirik Configure-a 
builder.Services.AddCustomTokenAuthExtention(builder.Configuration);

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
