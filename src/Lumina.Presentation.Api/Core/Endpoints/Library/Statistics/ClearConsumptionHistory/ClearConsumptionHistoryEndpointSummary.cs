#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.ClearConsumptionHistory;

/// <summary>
/// Class used for providing a textual description for the <see cref="ClearConsumptionHistoryEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearConsumptionHistoryEndpointSummary : Summary<ClearConsumptionHistoryEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClearConsumptionHistoryEndpointSummary"/> class.
    /// </summary>
    public ClearConsumptionHistoryEndpointSummary()
    {
        Summary = "Deletes every consumption record of the current user.";
        Description = "Deletes every recorded consumption session of the current user. The deletion is a hard delete, so the statistics and the history lists are empty as soon as it completes. Clearing an already empty history changes nothing and is not an error, so a retried clearance is safe; a concurrent write can still be rejected with a conflict. The resume points are not deleted: clearing the history and removing items from continue watching are separate user actions, and a user can keep their resume points after deleting their history. The history of another user is never deleted by this route.";

        Response(200, "The consumption history was successfully cleared.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = "/api/v1/users/me/consumption-history"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/consumption-history"
                }
            }
        );

        Response(403, "The request failed because the authenticated request does not carry the identity of a user.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/users/me/consumption-history",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(409, "The request failed because the deletion could not be stored, because the database is busy or locked, or because another request changed the same data after this request read it. Repeating the request is safe.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                title = "General.Conflict",
                status = 409,
                detail = "ConcurrencyConflict",
                instance = "/api/v1/users/me/consumption-history",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );
    }
}
