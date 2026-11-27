#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.DeleteConsumptionItemHistory;

/// <summary>
/// Class used for providing a textual description for the <see cref="DeleteUserConsumptionItemHistoryEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteUserConsumptionItemHistoryEndpointSummary : Summary<DeleteUserConsumptionItemHistoryEndpoint, DeleteConsumptionItemHistoryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteUserConsumptionItemHistoryEndpointSummary"/> class.
    /// </summary>
    public DeleteUserConsumptionItemHistoryEndpointSummary()
    {
        Summary = "Deletes every consumption record of a media item of a user.";
        Description = "Deletes every recorded consumption session of one media item of a user, in the library named by the route. The deletion covers every session of the item, independent of any date range, so the item disappears from the history consistently with clearing the whole history. A user can delete only their own history, while an Admin can select any user with the userId route parameter; a regular user targeting another user receives the same not authorized result as a route that names no accessible user. Deleting an item that has no recorded session changes nothing and is not an error, so a retried deletion is safe; a concurrent write can still be rejected with a conflict. The resume point of the media item is not deleted: removing an item from the history and removing it from continue watching are separate user actions.";

        Params["userId"] = "The Id of the user whose history is deleted. An Admin can name any user; a regular user can only name themselves.";
        Params["mediaKind"] = "The kind of the consumed media item.";
        Params["mediaItemId"] = "The Id of the consumed media item.";
        Params["libraryId"] = "The Id of the media library that contains the media item.";

        Response(200, "The consumption records of the media item were successfully deleted.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = "/api/v1/users/1a2b3c4d-5e6f-4a1b-8c2d-3e4f5a6b7c8d/consumption-history/media-items/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/1a2b3c4d-5e6f-4a1b-8c2d-3e4f5a6b7c8d/consumption-history/media-items/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"
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
                instance = "/api/v1/users/1a2b3c4d-5e6f-4a1b-8c2d-3e4f5a6b7c8d/consumption-history/media-items/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
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
                instance = "/api/v1/users/1a2b3c4d-5e6f-4a1b-8c2d-3e4f5a6b7c8d/consumption-history/media-items/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
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
                instance = "/api/v1/users/1a2b3c4d-5e6f-4a1b-8c2d-3e4f5a6b7c8d/consumption-history/media-items/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "TargetUserIdIsInvalid",
                            "UnsupportedMediaKind",
                            "MediaItemIdCannotBeEmpty",
                            "LibraryIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
