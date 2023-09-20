using OrderServer.API.Extentions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContextExtentions(builder.Configuration);
builder.Services.AddScopeExtention();
builder.Services.AddSingletonExtention(builder.Configuration);
builder.Services.AddCustomTokenAuthExtention(builder.Configuration);
builder.Services.OtherAdditions();
builder.Services.AddHttpClientExtention(builder.Configuration);
builder.Services.AddLoggingWithExtention(builder.Configuration);
builder.Services.AddCorsWithExtention();

var app = builder.Build();

app.Services.AddMigrationWithExtention();

//// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
// 	app.UseDeveloperExceptionPage();
// 	app.UseSwagger();
// 	app.UseSwaggerUI();
// }

app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseCors("corsapp");
app.UseAuthorization();

app.MapControllers();

app.Run();
