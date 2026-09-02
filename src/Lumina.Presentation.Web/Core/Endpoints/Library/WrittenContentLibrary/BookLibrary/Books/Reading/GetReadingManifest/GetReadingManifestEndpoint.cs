#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Reading;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingManifest;

/// <summary>
/// API endpoint for the <c>/{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-manifest</c> route.
/// </summary>
public class GetReadingManifestEndpoint : BaseEndpoint<GetBookReadingManifestRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingManifestEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetReadingManifestEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Books.GET_READING_MANIFEST);
        DontAutoTag();
        Options(options => options.WithTags("Books"));
    }

    /// <summary>
    /// Retrieves the reading manifest of a book.
    /// </summary>
    /// <param name="request">The request containing the Id of the book whose reading manifest is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetBookReadingManifestRequest request, CancellationToken cancellationToken)
    {
        try
        {
            ReadingManifestDto response = await _apiHttpClient.GetAsync<ReadingManifestDto>(ApiRoutes.Books.GET_BOOK_READING_MANIFEST.Replace("{bookId}", request.BookId.ToString()), cancellationToken).ConfigureAwait(false);
            return JsonSuccess(response);
        }
        catch (ApiException apiException)
        {
            // When the book has no available reader or its reader is disabled, the manifest cannot be produced at all; the reader view
            // distinguishes this case from a generic failure and warns the user, so it is reported with a distinct error code instead of
            // being flattened to a generic "NotFound" by the exception handling middleware.
            if (apiException.ProblemDetails?.Detail is "NoReaderAvailable" or "ReaderDisabled")
                return Results.Json(new { success = false, errorCode = apiException.ProblemDetails.Detail });
            throw;
        }
    }
}
