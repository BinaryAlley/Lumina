#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.DeleteAlbum;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.DeleteAlbum;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/{artistId}/albums/{albumId}</c> route.
/// </summary>
public class DeleteAlbumEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<DeleteAlbumCommand, Result<Deleted>> _deleteAlbumCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteAlbumEndpoint"/> class.
    /// </summary>
    /// <param name="deleteAlbumCommandHandler">Injected service for handling delete album commands.</param>
    public DeleteAlbumEndpoint(ICommandHandler<DeleteAlbumCommand, Result<Deleted>> deleteAlbumCommandHandler)
    {
        _deleteAlbumCommandHandler = deleteAlbumCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.Albums.DELETE_ALBUM);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Deletes an album by Id.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        string? albumId = HttpContext.Request.RouteValues["albumId"]?.ToString();
        Result<Deleted> result = await _deleteAlbumCommandHandler
            .HandleAsync(new DeleteAlbumCommand(libraryId, artistId, albumId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
