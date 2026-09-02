#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;

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
            .Replace("{bookId}", request.BookId.ToString())
            .Replace("{resourceKey}", Uri.EscapeDataString(request.ResourceKey)), cancellationToken).ConfigureAwait(false);
        // The resource is served without content sniffing, so that the browser never renders book content as anything but the media type the API declared.
        HttpContext.Response.Headers["X-Content-Type-Options"] = "nosniff";
        return Results.Bytes(response.Data, response.ContentType);
    }
}
