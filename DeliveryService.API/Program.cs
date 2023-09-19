using DeliveryServer.API.Extentions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContextWithExtention(builder.Configuration);
builder.Services.AddScopeWithExtention();
builder.Services.AddSingletonWithExtention(builder.Configuration);
builder.Services.AddCustomTokenAuthWithExtention(builder.Configuration);
builder.Services.OtherAdditionWithExtention();
builder.Services.AddHttpClientWithExtention(builder.Configuration); 
builder.Services.AddLoggingWithExtention(builder.Configuration);




var app = builder.Build();

app.Services.AddMigrationWithExtention();

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
