#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Favorites;
using Lumina.Application.Core.Favorites.Queries.GetFavoriteStatus;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Favorites;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Favorites.GetFavoriteStatus;

/// <summary>
/// API endpoint for the <c>/users/me/favorites/status</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class GetFavoriteStatusEndpoint : BaseEndpoint<GetFavoriteStatusRequest, IResult>
{
    private readonly IQueryHandler<GetFavoriteStatusQuery, Result<FavoriteStatusResponse>> _getFavoriteStatusQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoriteStatusEndpoint"/> class.
    /// </summary>
    /// <param name="getFavoriteStatusQueryHandler">Injected service for handling get favorite status queries.</param>
    public GetFavoriteStatusEndpoint(IQueryHandler<GetFavoriteStatusQuery, Result<FavoriteStatusResponse>> getFavoriteStatusQueryHandler)
    {
        _getFavoriteStatusQueryHandler = getFavoriteStatusQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Favorites.GET_FAVORITE_STATUS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Finds out which media items of one kind are favorites of the current user.
    /// </summary>
    /// <param name="request">The request containing the media kind and the Ids of the media items to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetFavoriteStatusRequest request, CancellationToken cancellationToken)
    {
        // The favorites belong to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Authorization";
        Result<FavoriteStatusResponse> result = await _getFavoriteStatusQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
