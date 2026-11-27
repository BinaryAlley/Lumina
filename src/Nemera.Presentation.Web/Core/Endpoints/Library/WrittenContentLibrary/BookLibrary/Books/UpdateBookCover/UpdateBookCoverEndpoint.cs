#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;

/// <summary>
/// API endpoint for the <c>/{culture}/library/written-content-library/books-library/books/{bookId}/api-update-cover</c> route.
/// </summary>
public class UpdateBookCoverEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCoverEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public UpdateBookCoverEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.PUT);
        Routes(WebRoutes.Books.UPDATE_BOOK_COVER);
        DontAutoTag();
        Options(options => options.WithTags("Books"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Updates the cover image of the book with the image uploaded in the multipart form of the request.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The endpoint takes no typed request, because its body is a multipart file, so the book id is read from the {bookId} route
        // value and the owning library from the {libraryId} query string; an unparseable book id becomes Guid.Empty. A missing file
        // is forwarded as a multipart form without any file, so that the upstream API reports it with its own validation error.
        Guid bookId = Guid.TryParse(HttpContext.Request.RouteValues["bookId"]?.ToString(), out Guid parsedBookId) ? parsedBookId : Guid.Empty;
        string? libraryId = HttpContext.Request.Query["libraryId"].ToString();

        // The cover is the first file of the multipart form; a non-multipart body, or a multipart body without any file, is treated
        // as a missing cover and forwarded as a request without a file, so that the upstream API reports it with its own validation error.
        IFormFile? cover = null;
        if (HttpContext.Request.HasFormContentType)
        {
            try
            {
                cover = HttpContext.Request.Form.Files.Count > 0 ? HttpContext.Request.Form.Files[0] : null;
            }
            catch (InvalidDataException)
            {
                cover = null;
            }
        }

        // The cover stream must stay open while the file is streamed to the API, and is disposed when the request completes.
        Stream? coverStream = cover?.OpenReadStream();
        try
        {
            UpdateBookCoverDto response = await _apiHttpClient.PutMultipartAsync<UpdateBookCoverDto>(
                ApiRoutes.Books.UPDATE_BOOK_COVER.Replace("{libraryId}", libraryId).Replace("{bookId}", bookId.ToString()), coverStream, cover?.FileName, "cover", cancellationToken).ConfigureAwait(false);
            return JsonSuccess(response.CoverPath);
        }
        finally
        {
            coverStream?.Dispose();
        }
    }
}
