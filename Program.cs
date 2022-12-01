using FluentValidation;
using FluentValidation.Results;
using LibraryMiniumAPI;
using LibraryMiniumAPI.Data;
using LibraryMiniumAPI.Models;
using LibraryMiniumAPI.Validators;
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
builder.Services.AddSingleton<IBookService, BookService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Using Swagger Middleware, which must come after .Build() is called.

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();


app.MapPost("books", async (Book book, IBookService bookService, IValidator<Book> validator, LinkGenerator linker, HttpContext context) =>
{

    var validationResult = await validator.ValidateAsync(book);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }

    var created = await bookService.CreateAsync(book);
    if (!created)
    {
        return Results.BadRequest(new List<ValidationFailure>
        {
            new ValidationFailure("Isbn", "A book with this ISBN-13 already exists")
        }); 
    }
    var path = linker.GetPathByName("GetBook", new { isbn = book.Isbn })!;
    var locationUri = linker.GetUriByName(context, "GetBook", new { isbn = book.Isbn })!;
    return Results.Created(locationUri, book);
    // return Results.Created(path, book);
    // return Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book);
    // return Results.Created($"/books/{book.Isbn}", book);
}).WithName("CreateBook")
.Accepts<Book>("application/json")
.Produces<Book>(201)
.Produces<IEnumerable<ValidationFailure>>(400)
.WithTags("Books");

app.MapPut("books/{isbn}", async (Book book, IBookService bookService, IValidator<Book> validator) =>
{

    var validationResult = await validator.ValidateAsync(book);
    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }
    
    var updated = await bookService.UpdateAsync(book);
    return updated ? Results.Ok(book) : Results.NotFound();
}).WithName("UpdateBook")
.Accepts<Book>("application/json")
.Produces<Book>(200)
.Produces<IEnumerable<ValidationFailure>>(400)
.WithTags("Books");

app.MapGet("books", async (IBookService bookService, string? searchTerm) =>
{
    if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
    {
        var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
        return Results.Ok(matchedBooks);
    }
    var books = await bookService.GetAllAsync();
    return Results.Ok(books);
}).WithName("GetBooks")
.Produces<IEnumerable<Book>>(200)
.WithTags("Books");

app.MapGet("books/{isbn}", async (string isbn, IBookService bookService) =>
{
    var book = await bookService.GetByIsbnAsync(isbn);

    return book is not null ? Results.Ok(book) : Results.NotFound();
}).WithName("GetBook")
.Produces<Book>(200)
.Produces(404)
.WithTags("Books");

app.MapDelete("books/{isbn}", async (string isbn, IBookService bookService ) =>
{
    var deleted = await bookService.DeleteAsync(isbn);
    return deleted ? Results.NoContent() : Results.NotFound();
}).WithName("DeleteBook")
.Produces(204)
.Produces(404)
.WithTags("Books");

app.MapGet("status", [EnableCors("AnyOrigin")] () =>
{
    return Results.Extensions.Html(@"<!doctype html>
<html>
    <head><title>Status Page</title></head>
    <body>
        <h1>Status</h1>
        <p>The server is working fine. Bye bye!</p>
    </body>
</html>");
}).ExcludeFromDescription(); // Removes endpoint from swagger. // .RequireCors("AnyOrigin") // Fluid approach to adding Cors.

// DB init here
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();



// Unsupported Features for Minimal APIs