#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingAvailability;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/books/{bookId}/reading/availability</c> route.
/// </summary>
public class GetReadingAvailabilityEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetReadingAvailabilityQuery, Result<ReadingAvailabilityResponse>> _getReadingAvailabilityQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingAvailabilityEndpoint"/> class.
    /// </summary>
    /// <param name="getReadingAvailabilityQueryHandler">Injected service for handling get reading availability queries.</param>
    public GetReadingAvailabilityEndpoint(IQueryHandler<GetReadingAvailabilityQuery, Result<ReadingAvailabilityResponse>> getReadingAvailabilityQueryHandler)
    {
        _getReadingAvailabilityQueryHandler = getReadingAvailabilityQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Books.GET_BOOK_READING_AVAILABILITY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Checks the reading availability of the book identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? bookId = HttpContext.Request.RouteValues["bookId"]?.ToString();
        GetReadingAvailabilityQuery query = new(libraryId, bookId);
        Result<ReadingAvailabilityResponse> result = await _getReadingAvailabilityQueryHandler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
