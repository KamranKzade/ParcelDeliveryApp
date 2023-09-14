using OrderServer.API.Extentions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Connectioni veririk
builder.Services.AddDbContextExtentions(builder.Configuration);

// Scope And Singleton LifeCycle
builder.Services.AddScopeExtention();
builder.Services.AddSingletonExtention(builder.Configuration);

// CustomToken Elave edirik Sisteme
builder.Services.AddCustomTokenAuthExtention(builder.Configuration);

// OtherExtention
builder.Services.OtherAdditions();

// AuthService -in methoduna müraciet 
builder.Services.AddHttpClientExtention(builder.Configuration);


var app = builder.Build();

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
app.UseAuthorization();

app.MapControllers();

app.Run();
