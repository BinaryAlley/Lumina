#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Reading;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingAvailability;

/// <summary>
/// API endpoint for the <c>/{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-availability</c> route.
/// </summary>
public class GetReadingAvailabilityEndpoint : BaseEndpoint<GetBookReadingAvailabilityRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingAvailabilityEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetReadingAvailabilityEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Books.GET_READING_AVAILABILITY);
        DontAutoTag();
        Options(options => options.WithTags("Books"));
    }

    /// <summary>
    /// Checks the reading availability of a book.
    /// </summary>
    /// <param name="request">The request containing the Id of the book whose reading availability is checked.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetBookReadingAvailabilityRequest request, CancellationToken cancellationToken)
    {
        ReadingAvailabilityDto response = await _apiHttpClient.GetAsync<ReadingAvailabilityDto>(ApiRoutes.Books.GET_BOOK_READING_AVAILABILITY.Replace("{bookId}", request.BookId.ToString()), cancellationToken).ConfigureAwait(false);
        return Results.Json(new { success = response.IsAvailable, errorCode = response.ErrorCode, libraryId = response.LibraryId });
    }
}
