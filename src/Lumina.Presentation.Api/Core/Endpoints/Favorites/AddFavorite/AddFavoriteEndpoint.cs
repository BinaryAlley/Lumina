#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Favorites;
using Lumina.Application.Core.Favorites.Commands.AddFavorite;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Favorites;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Favorites.AddFavorite;

/// <summary>
/// API endpoint for the <c>/users/me/favorites</c> route, for adding a favorite.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class AddFavoriteEndpoint : BaseEndpoint<AddFavoriteRequest, IResult>
{
    private readonly ICommandHandler<AddFavoriteCommand, Result<FavoriteResponse>> _addFavoriteCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddFavoriteEndpoint"/> class.
    /// </summary>
    /// <param name="addFavoriteCommandHandler">Injected service for handling add favorite commands.</param>
    public AddFavoriteEndpoint(ICommandHandler<AddFavoriteCommand, Result<FavoriteResponse>> addFavoriteCommandHandler)
    {
        _addFavoriteCommandHandler = addFavoriteCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Favorites.ADD_FAVORITE);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Marks a media item as a favorite of the current user.
    /// </summary>
    /// <param name="request">The request containing the kind and the Id of the media item to favorite.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddFavoriteRequest request, CancellationToken cancellationToken)
    {
        // The favorites belong to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Authorization";
        Result<FavoriteResponse> result = await _addFavoriteCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
