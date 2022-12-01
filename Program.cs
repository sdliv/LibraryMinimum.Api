using FluentValidation;
using FluentValidation.Results;
using LibraryMinimumAPI;
using LibraryMinimumAPI.Data;
using LibraryMinimumAPI.Endpoints;
using LibraryMinimumAPI.Models;
using LibraryMinimumAPI.Validators;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Json;

// WebApplicationOptions for the builder.
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    /*WebRootPath = "./wwwroot",
    EnvironmentName = Environment.GetEnvironmentVariable("env"),
    ApplicationName = "Library.Api"
    */
});
// Configuration example.
builder.Configuration.AddJsonFile("appsettings.Local.json", true, true);

// Authentication and Authorization Setups

// Services Setup

// CORS implementation
builder.Services.AddCors(options =>
{
    options.AddPolicy("AnyOrigin", x => x.AllowAnyOrigin());
});

// Custom JSON Binding.
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.IncludeFields = true;
});

// Swagger Setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registered Connection Factory Service referencing a value in the appsettings.json
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory(builder.Configuration.GetValue<string>("Database:ConnectionString")));

// Registers the Database Initializer as a Service.
builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.AddLibraryEndpoints();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Using Swagger Middleware, which must come after .Build() is called.

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.UseLibraryEndpoints();


// DB init here
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();



// Unsupported Features for Minimal APIs