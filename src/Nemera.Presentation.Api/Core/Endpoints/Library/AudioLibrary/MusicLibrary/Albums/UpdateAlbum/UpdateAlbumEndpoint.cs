#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbum;
using Nemera.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Nemera.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.UpdateAlbum;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/{artistId}/albums/{albumId}</c> route.
/// </summary>
public class UpdateAlbumEndpoint : BaseEndpoint<UpdateAlbumRequest, IResult>
{
    private readonly ICommandHandler<UpdateAlbumCommand, Result<AlbumResponse>> _updateAlbumCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAlbumEndpoint"/> class.
    /// </summary>
    /// <param name="updateAlbumCommandHandler">Injected service for handling update album commands.</param>
    public UpdateAlbumEndpoint(ICommandHandler<UpdateAlbumCommand, Result<AlbumResponse>> updateAlbumCommandHandler)
    {
        _updateAlbumCommandHandler = updateAlbumCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Albums.UPDATE_ALBUM);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Updates an album stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the album to be updated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdateAlbumRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        string? albumId = HttpContext.Request.RouteValues["albumId"]?.ToString();
        Result<AlbumResponse> result = await _updateAlbumCommandHandler.HandleAsync(request.ToCommand(libraryId, artistId, albumId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
