#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Presentation.Api.Controllers.Common;
using Mediator;
using Microsoft.AspNetCore.Mvc;
#endregion

namespace Lumina.Presentation.Api.Controllers.Books;

/// <summary>
/// Controller for managing books.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BooksController : ApiController 
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly ISender mediator;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="BooksController"/> class.
    /// </summary>
    /// <param name="mediator">Injecting service for mediating commands and queries.</param>
    public BooksController(ISender mediator)
    {
        this.mediator = mediator;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Gets all books.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{T}"/> containing either a collection of books, or an error.</returns>
    [HttpGet()]
    public async Task<IActionResult> GetBooks()
    {
        // ErrorOr<IEnumerable<BookDto>> result = await mediator.Send(new GetBooksQuery());
        // return result.Match(result => Ok(result), errors => Problem(errors));
        return Ok(new List<string> { "Book 1", "Book 2", "Book 3" });
    }

    // /// <summary>
    // /// Adds a book.
    // /// </summary>
    // /// <param name="command"></param>
    // /// <returns>The result of the HTTP request.</returns>
    // public async Task<IActionResult> AddBook(AddBookCommand command)
    // {
    //     ErrorOr<Unit> result = await mediator.Send(command);
    //     return result.Match(result => Ok(result), errors => Problem(errors));
    // }
    #endregion
}