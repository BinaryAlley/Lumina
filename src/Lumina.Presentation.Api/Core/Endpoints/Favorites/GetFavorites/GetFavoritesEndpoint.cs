#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Favorites;
using Lumina.Application.Core.Favorites.Queries.GetFavorites;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Favorites;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Favorites.GetFavorites;

/// <summary>
/// API endpoint for the <c>/users/me/favorites</c> route, for getting the favorites.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class GetFavoritesEndpoint : BaseEndpoint<GetFavoritesRequest, IResult>
{
    private readonly IQueryHandler<GetFavoritesQuery, Result<PaginatedResponse<FavoriteItemResponse>>> _getFavoritesQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoritesEndpoint"/> class.
    /// </summary>
    /// <param name="getFavoritesQueryHandler">Injected service for handling get favorites queries.</param>
    public GetFavoritesEndpoint(IQueryHandler<GetFavoritesQuery, Result<PaginatedResponse<FavoriteItemResponse>>> getFavoritesQueryHandler)
    {
        _getFavoritesQueryHandler = getFavoritesQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Favorites.GET_FAVORITES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets a page of the favorites of the current user.
    /// </summary>
    /// <param name="request">The request containing the optional media kind filter and the page bounds.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetFavoritesRequest request, CancellationToken cancellationToken)
    {
        // The favorites belong to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Authorization";
        Result<PaginatedResponse<FavoriteItemResponse>> result = await _getFavoritesQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
