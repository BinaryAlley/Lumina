#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
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
/// API endpoint for the <c>/books/{bookId}/reading/availability</c> route.
/// </summary>
public class GetReadingAvailabilityEndpoint : BaseEndpoint<GetReadingAvailabilityRequest, IResult>
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
        Verbs(Http.GET);
        Routes(ApiRoutes.Books.GET_BOOK_READING_AVAILABILITY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Checks the reading availability of the book identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the Id of the book whose reading availability is checked.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetReadingAvailabilityRequest request, CancellationToken cancellationToken)
    {
        Result<ReadingAvailabilityResponse> result = await _getReadingAvailabilityQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
