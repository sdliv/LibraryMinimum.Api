using LibraryMiniumAPI.Data;
using LibraryMiniumAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Services Setup

// Swagger Setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registered Connection Factory Service referencing a value in the appsettings.json
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory(builder.Configuration.GetValue<string>("Database:ConnectionString")));

// Registers the Database Initializer as a Service.
builder.Services.AddSingleton<DatabaseInitializer>();

var app = builder.Build();

// Using Swagger Middleware, which must come after .Build() is called.
app.UseSwagger();
app.UseSwaggerUI();


// DB init here
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();
