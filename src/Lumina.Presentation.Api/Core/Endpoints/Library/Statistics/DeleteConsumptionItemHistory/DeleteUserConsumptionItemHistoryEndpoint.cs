#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaConsumption.Commands.DeleteConsumptionItemHistory;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Statistics;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.DeleteConsumptionItemHistory;

/// <summary>
/// API endpoint for the <c>/users/{userId}/consumption-history/media-items/{mediaKind}/{mediaItemId}/{libraryId}</c> route.
/// </summary>
public class DeleteUserConsumptionItemHistoryEndpoint : BaseEndpoint<DeleteConsumptionItemHistoryRequest, IResult>
{
    private readonly ICommandHandler<DeleteConsumptionItemHistoryCommand, Result<Deleted>> _deleteConsumptionItemHistoryCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteUserConsumptionItemHistoryEndpoint"/> class.
    /// </summary>
    /// <param name="deleteConsumptionItemHistoryCommandHandler">Injected service for handling delete consumption item history commands.</param>
    public DeleteUserConsumptionItemHistoryEndpoint(ICommandHandler<DeleteConsumptionItemHistoryCommand, Result<Deleted>> deleteConsumptionItemHistoryCommandHandler)
    {
        _deleteConsumptionItemHistoryCommandHandler = deleteConsumptionItemHistoryCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.LibraryStatistics.DELETE_USER_CONSUMPTION_ITEM_HISTORY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Deletes every consumption record of a media item of a user.
    /// </summary>
    /// <param name="request">The request containing the media item and its library.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(DeleteConsumptionItemHistoryRequest request, CancellationToken cancellationToken)
    {
        // The consumption records belong to the target user, and only that user or an Admin may delete them, so no intermediary may store any
        // response of this route, including its problem responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers.Append("Vary", "Authorization");
        string? targetUserId = HttpContext.Request.RouteValues["userId"]?.ToString();
        Result<Deleted> result = await _deleteConsumptionItemHistoryCommandHandler
            .HandleAsync(new DeleteConsumptionItemHistoryCommand(targetUserId, request.MediaKind, request.MediaItemId, request.LibraryId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
