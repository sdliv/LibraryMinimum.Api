using FluentValidation.Results;
using FluentValidation;
using LibraryMinimumAPI.Endpoints.Internal;
using LibraryMinimumAPI.Models;
using LibraryMinimumAPI.Validators;
using Microsoft.AspNetCore.Cors;
using System.Reflection;
using System.Net.NetworkInformation;

namespace LibraryMinimumAPI.Endpoints
{
    public class LibraryEndpoints : IEndpoints
    {
        public static void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IBookService, BookService>();

        }

        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("books", CreateBookAsync)
                .WithName("CreateBook")
                .Accepts<Book>("application/json")
                .Produces<Book>(201)
                .Produces<IEnumerable<ValidationFailure>>(400)
                .WithTags("Books");

            app.MapPut("books/{isbn}", UpdateBookByIsbnAsync) 
                .WithName("UpdateBook")
                .Accepts<Book>("application/json")
                .Produces<Book>(200)
                .Produces<IEnumerable<ValidationFailure>>(400)
                .WithTags("Books");

            app.MapGet("books", GetAllBooksAsync)
                .WithName("GetBooks")
                .Produces<IEnumerable<Book>>(200)
                .WithTags("Books");

            app.MapGet("books/{isbn}", GetBookByIsbnAsync)
                .WithName("GetBook")
                .Produces<Book>(200)
                .Produces(404)
                .WithTags("Books");

            app.MapDelete("books/{isbn}", DeleteBookByIsbn)
                .WithName("DeleteBook")
                .Produces(204)
                .Produces(404)
                .WithTags("Books");

            app.MapGet("status", Status)
                .ExcludeFromDescription(); // Removes endpoint from swagger. // .RequireCors("AnyOrigin") // Fluid approach to adding Cors.
        }

        internal static async Task<IResult> CreateBookAsync(Book book, IBookService bookService, IValidator<Book> validator)
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
            return Results.Created($"/book/{book.Isbn}", book); 
        }

        internal static async Task<IResult> GetAllBooksAsync(IBookService bookService, string? searchTerm) 
        {
            if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
            {
                var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
                return Results.Ok(matchedBooks);
            }
            var books = await bookService.GetAllAsync();
            return Results.Ok(books);
        }

        internal static async Task<IResult>GetBookByIsbnAsync(string isbn, IBookService bookService)
        {
            var book = await bookService.GetByIsbnAsync(isbn);

            return book is not null ? Results.Ok(book) : Results.NotFound();
        }

        internal static async Task<IResult>UpdateBookByIsbnAsync(Book book, IBookService bookService, IValidator<Book> validator)
        {
            var validationResult = await validator.ValidateAsync(book);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }

            var updated = await bookService.UpdateAsync(book);
            return updated ? Results.Ok(book) : Results.NotFound();
        }

        internal static async Task<IResult>DeleteBookByIsbn(string isbn, IBookService bookService)
        {
            var deleted = await bookService.DeleteAsync(isbn);
            return deleted ? Results.NoContent() : Results.NotFound();
        }

        [EnableCors("AnyOrigin")]
        internal static IResult Status()
        {
            return Results.Extensions.Html(@"<!doctype html>
            <html>
                <head><title>Status Page</title></head>
             <body>
                <h1>Status</h1>
                <p>The server is working fine. Bye bye!</p>
            </body>
            </html>");
        }
    }
}
