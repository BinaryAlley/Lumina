#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingSection;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/books/{bookId}/reading/sections/{locationRef}</c> route.
/// </summary>
public class GetReadingSectionEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetReadingSectionQuery, Result<ReadingSectionDto>> _getReadingSectionQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingSectionEndpoint"/> class.
    /// </summary>
    /// <param name="getReadingSectionQueryHandler">Injected service for handling get reading section queries.</param>
    public GetReadingSectionEndpoint(IQueryHandler<GetReadingSectionQuery, Result<ReadingSectionDto>> getReadingSectionQueryHandler)
    {
        _getReadingSectionQueryHandler = getReadingSectionQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Books.GET_BOOK_READING_SECTION);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the content of the reading section of the book identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? bookId = HttpContext.Request.RouteValues["bookId"]?.ToString();
        string? locationRef = HttpContext.Request.RouteValues["locationRef"]?.ToString();
        GetReadingSectionQuery query = new(libraryId, bookId, locationRef);
        Result<ReadingSectionDto> result = await _getReadingSectionQueryHandler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
