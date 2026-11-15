#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.RestoreAlbum;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.RestoreAlbum;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/restore</c> route.
/// </summary>
public class RestoreAlbumEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<RestoreAlbumCommand, Result<Updated>> _restoreAlbumCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreAlbumEndpoint"/> class.
    /// </summary>
    /// <param name="restoreAlbumCommandHandler">Injected service for handling restore album commands.</param>
    public RestoreAlbumEndpoint(ICommandHandler<RestoreAlbumCommand, Result<Updated>> restoreAlbumCommandHandler)
    {
        _restoreAlbumCommandHandler = restoreAlbumCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Albums.RESTORE_ALBUM);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Restores the deleted album identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take the unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        string? albumId = HttpContext.Request.RouteValues["albumId"]?.ToString();
        Result<Updated> result = await _restoreAlbumCommandHandler.HandleAsync(new RestoreAlbumCommand(libraryId, artistId, albumId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
