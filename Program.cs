using FluentValidation;
using FluentValidation.Results;
using LibraryMiniumAPI.Data;
using LibraryMiniumAPI.Models;
using LibraryMiniumAPI.Validators;

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
builder.Services.AddSingleton<IBookService, BookService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Using Swagger Middleware, which must come after .Build() is called.
app.UseSwagger();
app.UseSwaggerUI();


app.MapPost("books", async (Book book, IBookService bookService, IValidator<Book> validator) =>
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

    return Results.Created($"/books/{book.Isbn}", book);
});

app.MapGet("books", async (IBookService bookService) =>
{
    var books = await bookService.GetAllAsync();
    return Results.Ok(books);
});

// DB init here
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();
