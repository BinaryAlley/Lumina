#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.ClearConsumptionHistory;

/// <summary>
/// Class used for providing a textual description for the <see cref="ClearUserConsumptionHistoryEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearUserConsumptionHistoryEndpointSummary : Summary<ClearUserConsumptionHistoryEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClearUserConsumptionHistoryEndpointSummary"/> class.
    /// </summary>
    public ClearUserConsumptionHistoryEndpointSummary()
    {
        Summary = "Deletes every consumption record of a user.";
        Description = "Deletes every recorded consumption session of a user. A user can clear only their own history, while an Admin can select any user with the userId route parameter; a regular user targeting another user receives the same not authorized result as a route that names no accessible user. The deletion is a hard delete, so the statistics and the history lists are empty as soon as it completes. Clearing an already empty history changes nothing and is not an error, so a retried clearance is safe; a concurrent write can still be rejected with a conflict. The resume points are not deleted: clearing the history and removing items from continue watching are separate user actions, and a user can keep their resume points after deleting their history.";

        Params["userId"] = "The Id of the user whose history is cleared. An Admin can name any user; a regular user can only name themselves.";

        Response(200, "The consumption history of the user was successfully cleared.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = "/api/v1/users/{userId}/consumption-history"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/{userId}/consumption-history"
                }
            }
        );

        Response(403, "The request failed because the authenticated request does not carry the identity of a user, or because the current user is neither the target user nor an Admin.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/users/{userId}/consumption-history",
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
                instance = "/api/v1/users/{userId}/consumption-history",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/users/{userId}/consumption-history",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "TargetUserIdIsInvalid"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
