#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;
using Lumina.Application.Core.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;
using Lumina.Contracts.Requests.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Api.Core.Controllers.Common;
using MapsterMapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Controllers.WrittenContentLibrary.BookLibrary.Books;

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
    /// <param name="mediator">Injected service for mediating commands and queries.</param>
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
        var result = await _mediator.Send(new GetBooksQuery()).ConfigureAwait(false);
        return result.Match(result => Ok(result), errors => Problem(errors));
    }

    /// <summary>
    /// Controller action for adding a book.
    /// </summary>
    /// <param name="request">The command to add a book.</param>
    [HttpPost()]
    public async Task<IActionResult> AddBook(AddBookRequest request)
    {
        var result = await _mediator.Send(_mapper.Map<AddBookCommand>(request)).ConfigureAwait(false);
        return result.Match(result => Created($"/api/v1/books/{result.Id}", result), errors => Problem(errors));
    }
    #endregion
}