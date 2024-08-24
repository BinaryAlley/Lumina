#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.WrittenContentLibrary.BooksLibrary.Books.Commands;
using Lumina.Presentation.Api.Common.Contracts.Books;
using Lumina.Presentation.Api.Controllers.Common;
using MapsterMapper;
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
    private readonly ISender _mediator;
    private readonly IMapper _mapper;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="BooksController"/> class.
    /// </summary>
    /// <param name="mediator">Injecting service for mediating commands and queries.</param>
    /// <param name="mapper">Injected service for mapping objects.</param>
    public BooksController(ISender mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Controller action for getting the list of all books.
    /// </summary>
    [HttpGet()]
    public async Task<IActionResult> GetBooks()
    {
        // ErrorOr<IEnumerable<BookDto>> result = await mediator.Send(new GetBooksQuery());
        // return result.Match(result => Ok(result), errors => Problem(errors));
        return Ok(new List<string> { "Book 1", "Book 2", "Book 3" });
    }

    /// <summary>
    /// Controller action for adding a book.
    /// </summary>
    /// <param name="command">The command to add a book.</param>
    [HttpPost()]
    public async Task<IActionResult> AddBook(AddBookRequest request)
    {
        var result = await _mediator.Send(_mapper.Map<AddBookCommand>(request));
        return result.Match(result => Ok(result), errors => Problem(errors));
    }
    #endregion
}