#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.DeleteConsumptionRecord;

/// <summary>
/// Class used for providing a textual description for the <see cref="DeleteConsumptionRecordEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteConsumptionRecordEndpointSummary : Summary<DeleteConsumptionRecordEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConsumptionRecordEndpointSummary"/> class.
    /// </summary>
    public DeleteConsumptionRecordEndpointSummary()
    {
        Summary = "Deletes one consumption record of the current user.";
        Description = "Deletes one recorded consumption session of the current user, named by the route. The deletion is a hard delete, so the session stops contributing to the statistics and to the history lists immediately. Deleting an absent record, or a record that was already deleted in another tab, changes nothing and is not an error, so a retried deletion is safe; a concurrent write can still be rejected with a conflict. The resume point of the media item is not deleted: removing a record from the history and removing an item from continue watching are separate user actions. The record of another user is never deleted by this route.";

        Params["recordId"] = "The Id of the consumption record to delete.";

        Response(200, "The consumption record was successfully deleted.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = "/api/v1/users/me/consumption-history/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/consumption-history/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f"
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
                instance = "/api/v1/users/me/consumption-history/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
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
                instance = "/api/v1/users/me/consumption-history/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
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
                instance = "/api/v1/users/me/consumption-history/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "ConsumptionRecordIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
