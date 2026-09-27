#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.AddAlbum;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/{artistId}/albums</c> route.
/// </summary>
public class AddAlbumEndpoint : BaseEndpoint<AddAlbumRequest, IResult>
{
    private readonly ICommandHandler<AddAlbumCommand, Result<AlbumResponse>> _addAlbumCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddAlbumEndpoint"/> class.
    /// </summary>
    /// <param name="addAlbumCommandHandler">Injected service for handling add album commands.</param>
    public AddAlbumEndpoint(ICommandHandler<AddAlbumCommand, Result<AlbumResponse>> addAlbumCommandHandler)
    {
        _addAlbumCommandHandler = addAlbumCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Albums.ADD_ALBUM);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Adds an album stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the album to be added.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddAlbumRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        Result<AlbumResponse> result = await _addAlbumCommandHandler.HandleAsync(request.ToCommand(libraryId, artistId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Created($"{BaseURL}api/v1/libraries/{libraryId}/artists/{artistId}/albums/{result.Value.Id}", result.Value), Problem);
    }
}
