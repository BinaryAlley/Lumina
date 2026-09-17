#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.DeleteAlbum;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.DeleteAlbum;

/// <summary>
/// API endpoint for the <c>/artists/{artistId}/albums/{albumId}</c> route.
/// </summary>
public class DeleteAlbumEndpoint : BaseEndpoint<DeleteAlbumRequest, IResult>
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
    /// <param name="request">The request containing the id of the album to be deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(DeleteAlbumRequest request, CancellationToken cancellationToken)
    {
        Result<Deleted> result = await _deleteAlbumCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
