#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetDeletedAlbums;
using Nemera.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetDeletedAlbums;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/albums/deleted-items</c> route.
/// </summary>
public class GetDeletedAlbumsEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetDeletedAlbumsQuery, Result<IReadOnlyList<DeletedAlbumResponse>>> _getDeletedAlbumsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedAlbumsEndpoint"/> class.
    /// </summary>
    /// <param name="getDeletedAlbumsQueryHandler">Injected service for handling get deleted albums queries.</param>
    public GetDeletedAlbumsEndpoint(IQueryHandler<GetDeletedAlbumsQuery, Result<IReadOnlyList<DeletedAlbumResponse>>> getDeletedAlbumsQueryHandler)
    {
        _getDeletedAlbumsQueryHandler = getDeletedAlbumsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Albums.GET_DELETED_ALBUMS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the deleted albums of the library identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take the unique identifier of the library from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        Result<IReadOnlyList<DeletedAlbumResponse>> result = await _getDeletedAlbumsQueryHandler.HandleAsync(new GetDeletedAlbumsQuery(libraryId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
