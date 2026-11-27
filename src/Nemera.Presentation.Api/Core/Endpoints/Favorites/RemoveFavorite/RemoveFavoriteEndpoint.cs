#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Favorites.Commands.RemoveFavorite;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Favorites;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Favorites.RemoveFavorite;

/// <summary>
/// API endpoint for the <c>/users/me/favorites/{mediaKind}/{mediaItemId}</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class RemoveFavoriteEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<RemoveFavoriteCommand, Result<Deleted>> _removeFavoriteCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveFavoriteEndpoint"/> class.
    /// </summary>
    /// <param name="removeFavoriteCommandHandler">Injected service for handling remove favorite commands.</param>
    public RemoveFavoriteEndpoint(ICommandHandler<RemoveFavoriteCommand, Result<Deleted>> removeFavoriteCommandHandler)
    {
        _removeFavoriteCommandHandler = removeFavoriteCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.Favorites.REMOVE_FAVORITE);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Removes a media item from the favorites of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // The favorites belong to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Authorization";
        // Take the kind and the unique identifier of the media item from the route, as raw strings so the validator reports a clean error for an unparseable value.
        string? mediaKind = HttpContext.Request.RouteValues["mediaKind"]?.ToString();
        string? mediaItemId = HttpContext.Request.RouteValues["mediaItemId"]?.ToString();
        Result<Deleted> result = await _removeFavoriteCommandHandler.HandleAsync(new RemoveFavoriteCommand(mediaKind, mediaItemId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
