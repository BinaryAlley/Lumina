#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.AddArtist;
using Nemera.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Nemera.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.AddArtist;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists</c> route.
/// </summary>
public class AddArtistEndpoint : BaseEndpoint<AddArtistRequest, IResult>
{
    private readonly ICommandHandler<AddArtistCommand, Result<ArtistResponse>> _addArtistCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddArtistEndpoint"/> class.
    /// </summary>
    /// <param name="addArtistCommandHandler">Injected service for handling add artist commands.</param>
    public AddArtistEndpoint(ICommandHandler<AddArtistCommand, Result<ArtistResponse>> addArtistCommandHandler)
    {
        _addArtistCommandHandler = addArtistCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Artists.ADD_ARTIST);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Adds an artist stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the artist to be added.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddArtistRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        Result<ArtistResponse> result = await _addArtistCommandHandler.HandleAsync(request.ToCommand(libraryId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Created($"{BaseURL}api/v1/libraries/{libraryId}/artists/{result.Value.Id}", result.Value), Problem);
    }
}
