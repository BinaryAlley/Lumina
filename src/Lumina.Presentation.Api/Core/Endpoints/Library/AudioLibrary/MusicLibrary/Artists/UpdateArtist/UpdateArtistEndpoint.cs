#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.UpdateArtist;

/// <summary>
/// API endpoint for the <c>/{libraryId}/artists/{artistId}</c> route.
/// </summary>
public class UpdateArtistEndpoint : BaseEndpoint<UpdateArtistRequest, IResult>
{
    private readonly ICommandHandler<UpdateArtistCommand, Result<ArtistResponse>> _updateArtistCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateArtistEndpoint"/> class.
    /// </summary>
    /// <param name="updateArtistCommandHandler">Injected service for handling update artist commands.</param>
    public UpdateArtistEndpoint(ICommandHandler<UpdateArtistCommand, Result<ArtistResponse>> updateArtistCommandHandler)
    {
        _updateArtistCommandHandler = updateArtistCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Artists.UPDATE_ARTIST);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Updates the artist described in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the edited details of the artist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdateArtistRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        Result<ArtistResponse> result = await _updateArtistCommandHandler.HandleAsync(request.ToCommand(libraryId, artistId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
