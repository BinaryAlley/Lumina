#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;
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
/// API endpoint for the <c>/books/{id}/cover</c> route.
/// </summary>
public class UpdateBookCoverEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly Application.Common.CQRS.ICommandHandler<UpdateBookCoverCommand, Result<string>> _updateBookCoverCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCoverEndpoint"/> class.
    /// </summary>
    /// <param name="updateBookCoverCommandHandler">Injected service for handling update book cover commands.</param>
    public UpdateBookCoverEndpoint(Application.Common.CQRS.ICommandHandler<UpdateBookCoverCommand, Result<string>> updateBookCoverCommandHandler)
    {
        _updateBookCoverCommandHandler = updateBookCoverCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.PUT);
        Routes(ApiRoutes.Books.UPDATE_BOOK_COVER);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Updates the cover image of the book with the image uploaded in the multipart form of the request.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The endpoint takes no typed request, because its body is a multipart file, so the book id is read from the {id} route
        // value; an unparseable value becomes Guid.Empty, which the command validator reports as a missing book id.
        Guid bookId = Guid.TryParse(HttpContext.Request.RouteValues["id"]?.ToString(), out Guid parsedBookId) ? parsedBookId : Guid.Empty;

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
        UpdateBookCoverCommand command = new(bookId, cover?.OpenReadStream(), cover?.FileName);
        try
        {
            Result<string> result = await _updateBookCoverCommandHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            return result.Match(success => TypedResults.Ok(success), Problem);
        }
        finally
        {
            command.Cover?.Dispose();
        }
    }
}
