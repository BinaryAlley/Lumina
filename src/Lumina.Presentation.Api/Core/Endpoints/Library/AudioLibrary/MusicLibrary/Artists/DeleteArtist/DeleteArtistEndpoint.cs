#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.DeleteArtist;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.DeleteArtist;

/// <summary>
/// API endpoint for the <c>/{libraryId}/artists/{artistId}</c> route.
/// </summary>
public class DeleteArtistEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<DeleteArtistCommand, Result<Deleted>> _deleteArtistCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteArtistEndpoint"/> class.
    /// </summary>
    /// <param name="deleteArtistCommandHandler">Injected service for handling delete artist commands.</param>
    public DeleteArtistEndpoint(ICommandHandler<DeleteArtistCommand, Result<Deleted>> deleteArtistCommandHandler)
    {
        _deleteArtistCommandHandler = deleteArtistCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.Artists.DELETE_ARTIST);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Deletes an artist by Id.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        Result<Deleted> result = await _deleteArtistCommandHandler.HandleAsync(new DeleteArtistCommand(libraryId, artistId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
