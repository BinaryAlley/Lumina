#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.RestoreArtist;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.RestoreArtist;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/{artistId}/restore</c> route.
/// </summary>
public class RestoreArtistEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<RestoreArtistCommand, Result<Updated>> _restoreArtistCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreArtistEndpoint"/> class.
    /// </summary>
    /// <param name="restoreArtistCommandHandler">Injected service for handling restore artist commands.</param>
    public RestoreArtistEndpoint(ICommandHandler<RestoreArtistCommand, Result<Updated>> restoreArtistCommandHandler)
    {
        _restoreArtistCommandHandler = restoreArtistCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Artists.RESTORE_ARTIST);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Restores the deleted artist identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take the unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        Result<Updated> result = await _restoreArtistCommandHandler.HandleAsync(new RestoreArtistCommand(libraryId, artistId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
