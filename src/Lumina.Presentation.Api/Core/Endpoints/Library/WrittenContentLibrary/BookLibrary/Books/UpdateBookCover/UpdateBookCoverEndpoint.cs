#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/books/{bookId}/cover</c> route.
/// </summary>
public class UpdateBookCoverEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<UpdateBookCoverCommand, Result<UpdateBookCoverResponse>> _updateBookCoverCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCoverEndpoint"/> class.
    /// </summary>
    /// <param name="updateBookCoverCommandHandler">Injected service for handling update book cover commands.</param>
    public UpdateBookCoverEndpoint(ICommandHandler<UpdateBookCoverCommand, Result<UpdateBookCoverResponse>> updateBookCoverCommandHandler)
    {
        _updateBookCoverCommandHandler = updateBookCoverCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Books.UPDATE_BOOK_COVER);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Updates the cover image of the book with the image uploaded in the multipart form of the request.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take the identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? bookId = HttpContext.Request.RouteValues["bookId"]?.ToString();

        IFormFile? cover = null;
        if (HttpContext.Request.HasFormContentType)
        {
            try
            {
                // The cover is the first file of the multipart form; a multipart body without any file cannot be parsed as a form,
                // so a malformed upload is treated as a missing cover, which the command validator reports.
                cover = HttpContext.Request.Form.Files.Count > 0 ? HttpContext.Request.Form.Files[0] : null;
            }
            catch (InvalidDataException)
            {
                cover = null;
            }
        }

        // The cover stream must stay open while the handler stores the image, and is disposed as soon as the handler returns.
        UpdateBookCoverCommand command = new(libraryId, bookId, cover?.OpenReadStream(), cover?.FileName);
        try
        {
            Result<UpdateBookCoverResponse> result = await _updateBookCoverCommandHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            return result.Match(success => TypedResults.Ok(success), Problem);
        }
        finally
        {
            command.Cover?.Dispose();
        }
    }
}
