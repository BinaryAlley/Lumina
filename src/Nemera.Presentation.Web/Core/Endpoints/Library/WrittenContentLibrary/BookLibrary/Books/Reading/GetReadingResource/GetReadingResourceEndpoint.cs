#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.DTO.FileSystemManagement;
using Nemera.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;

/// <summary>
/// API endpoint for the <c>/{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-resource</c> route.
/// </summary>
public class GetReadingResourceEndpoint : BaseEndpoint<GetBookReadingResourceRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingResourceEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetReadingResourceEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Books.GET_READING_RESOURCE);
        DontAutoTag();
        Options(options => options.WithTags("Books"));
    }

    /// <summary>
    /// Retrieves a resource of a book, for reading.
    /// </summary>
    /// <param name="request">The request containing the Id of the book and the resource key of the resource.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetBookReadingResourceRequest request, CancellationToken cancellationToken)
    {
        BlobDataDto response = await _apiHttpClient.GetBlobAsync(ApiRoutes.Books.GET_BOOK_READING_RESOURCE
            .Replace("{libraryId}", request.LibraryId.ToString())
            .Replace("{bookId}", request.BookId.ToString())
            .Replace("{resourceKey}", Uri.EscapeDataString(request.ResourceKey)), cancellationToken).ConfigureAwait(false);
        // The resource is served without content sniffing, so that the browser never renders book content as anything but the media type the API declared.
        HttpContext.Response.Headers["X-Content-Type-Options"] = "nosniff";
        return Results.Bytes(response.Data, response.ContentType);
    }
}
