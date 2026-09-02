#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingResource;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;

/// <summary>
/// API endpoint for the <c>/books/{bookId}/reading/resources/{resourceKey}</c> route.
/// </summary>
public class GetReadingResourceEndpoint : BaseEndpoint<GetReadingResourceRequest, IResult>
{
    private readonly IQueryHandler<GetReadingResourceQuery, Result<ReadingResourceDataDto>> _getReadingResourceQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingResourceEndpoint"/> class.
    /// </summary>
    /// <param name="getReadingResourceQueryHandler">Injected service for handling get reading resource queries.</param>
    public GetReadingResourceEndpoint(IQueryHandler<GetReadingResourceQuery, Result<ReadingResourceDataDto>> getReadingResourceQueryHandler)
    {
        _getReadingResourceQueryHandler = getReadingResourceQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Books.GET_BOOK_READING_RESOURCE);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the resource of the book identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the Id of the book and the resource key of the resource.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetReadingResourceRequest request, CancellationToken cancellationToken)
    {
        Result<ReadingResourceDataDto> result = await _getReadingResourceQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
            return Problem(result.Errors);
        // The media type of a resource is declared by the book itself, so it is not trusted: a resource whose declared media type could be
        // rendered as an active document (for example an SVG) is served as an opaque binary download instead, and every resource is served
        // with content sniffing disabled, so that the browser never executes book content as anything but what this endpoint declares.
        HttpContext.Response.Headers["X-Content-Type-Options"] = "nosniff";
        return TypedResults.Bytes(result.Value.Data, GetSafeContentType(result.Value.MimeType));
    }

    /// <summary>
    /// Gets the safe content type of a book resource, replacing the media types that could be rendered as active documents with an opaque binary type.
    /// </summary>
    /// <param name="mediaType">The media type declared by the book.</param>
    /// <returns>The safe content type the resource is served with.</returns>
    private static string GetSafeContentType(string mediaType)
    {
        // Images, audio, video, fonts, and stylesheets are inert content the reader can render; everything else (an SVG, an XML or HTML
        // document, a script) is served as an opaque download, so that opening a resource URL never executes active content.
        if (mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) && !string.Equals(mediaType, "image/svg+xml", StringComparison.OrdinalIgnoreCase))
            return mediaType;
        if (mediaType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) || mediaType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            return mediaType;
        if (mediaType.StartsWith("font/", StringComparison.OrdinalIgnoreCase)
            || string.Equals(mediaType, "application/font-woff", StringComparison.OrdinalIgnoreCase)
            || string.Equals(mediaType, "application/font-woff2", StringComparison.OrdinalIgnoreCase)
            || string.Equals(mediaType, "application/vnd.ms-opentype", StringComparison.OrdinalIgnoreCase)
            || string.Equals(mediaType, "application/x-font-ttf", StringComparison.OrdinalIgnoreCase)
            || string.Equals(mediaType, "application/x-font-opentype", StringComparison.OrdinalIgnoreCase)
            || string.Equals(mediaType, "text/css", StringComparison.OrdinalIgnoreCase))
            return mediaType;
        return "application/octet-stream";
    }
}
