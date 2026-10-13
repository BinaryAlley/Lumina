#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtistCover;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.UpdateArtistCover;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/{artistId}/cover</c> route.
/// </summary>
public class UpdateArtistCoverEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<UpdateArtistCoverCommand, Result<UpdateArtistCoverResponse>> _updateArtistCoverCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateArtistCoverEndpoint"/> class.
    /// </summary>
    /// <param name="updateArtistCoverCommandHandler">Injected service for handling update artist cover commands.</param>
    public UpdateArtistCoverEndpoint(ICommandHandler<UpdateArtistCoverCommand, Result<UpdateArtistCoverResponse>> updateArtistCoverCommandHandler)
    {
        _updateArtistCoverCommandHandler = updateArtistCoverCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Artists.UPDATE_ARTIST_COVER);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Updates the cover image of the artist with the image uploaded in the multipart form of the request.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take the identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();

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
        UpdateArtistCoverCommand command = new(libraryId, artistId, cover?.OpenReadStream(), cover?.FileName);
        try
        {
            Result<UpdateArtistCoverResponse> result = await _updateArtistCoverCommandHandler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            return result.Match(success => TypedResults.Ok(success), Problem);
        }
        finally
        {
            command.Cover?.Dispose();
        }
    }
}
