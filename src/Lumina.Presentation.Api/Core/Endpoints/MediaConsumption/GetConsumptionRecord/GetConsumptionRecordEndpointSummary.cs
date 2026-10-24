#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.GetConsumptionRecord;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetConsumptionRecordEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetConsumptionRecordEndpointSummary : Summary<GetConsumptionRecordEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetConsumptionRecordEndpointSummary"/> class.
    /// </summary>
    public GetConsumptionRecordEndpointSummary()
    {
        Summary = "Retrieves a consumption record of the current user.";
        Description = "Retrieves the consumption record identified by the route, when it belongs to the current user. The record carries the kind of media, the last reported position, the progress fraction, the consumed duration, the consecutive pages read for page based media, and whether the session is in progress, completed or stopped. It never carries a queue, a current index, a repeat or shuffle state, or a playback session. A record of another user is reported with the same not found error as a missing record. The example is a book record, whose position is an opaque location; a music track record uses a time offset instead.";

        Params["recordId"] = "The Id of the consumption record.";

        ResponseParam<ConsumptionRecordResponse>(record => record.Id, "The Id of the consumption record.");
        ResponseParam<ConsumptionRecordResponse>(record => record.LibraryId, "The Id of the media library that contains the consumed media item.");
        ResponseParam<ConsumptionRecordResponse>(record => record.MediaItemId, "The Id of the consumed media item.");
        ResponseParam<ConsumptionRecordResponse>(record => record.MediaKind, "The kind of media that was consumed.");
        ResponseParam<ConsumptionRecordResponse>(record => record.Status, "The state of the consumption session.");
        ResponseParam<ConsumptionRecordResponse>(record => record.StartedOnUtc, "The date and time when the session started.");
        ResponseParam<ConsumptionRecordResponse>(record => record.LastActivityOnUtc, "The date and time when the session last received activity.");
        ResponseParam<ConsumptionRecordResponse>(record => record.EndedOnUtc, "The date and time when the session completed or stopped, if it has ended.");
        ResponseParam<ConsumptionRecordResponse>(record => record.ConsumedDurationInSeconds, "The active duration consumed by the session, in seconds.");
        ResponseParam<ConsumptionRecordResponse>(record => record.LastPosition, "The last known position for the media item, if any. It carries the position Kind, and either TimeOffsetSeconds or Location.");
        ResponseParam<ConsumptionRecordResponse>(record => record.LastProgressFraction, "The latest reported progress fraction of the media item.");
        ResponseParam<ConsumptionRecordResponse>(record => record.ConsecutivePagesRead, "The number of consecutive pages read in the session.");

        Response<ConsumptionRecordResponse>(statusCode: 200, description: "The consumption record is returned.", contentType: "application/json",
            example: new ConsumptionRecordResponse(
                Id: Guid.Parse("2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f"),
                LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                MediaItemId: Guid.Parse("7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d"),
                MediaKind: MediaKind.Book,
                Status: ConsumptionStatus.Stopped,
                StartedOnUtc: new DateTime(2026, 10, 24, 12, 0, 0, DateTimeKind.Utc),
                LastActivityOnUtc: new DateTime(2026, 10, 24, 12, 12, 0, DateTimeKind.Utc),
                EndedOnUtc: new DateTime(2026, 10, 24, 12, 12, 0, DateTimeKind.Utc),
                ConsumedDurationInSeconds: 720,
                LastPosition: new MediaPositionResponse(MediaPositionKind.Location, null, "epubcfi(/6/4[chapter-3]!/4/2/2)"),
                LastProgressFraction: 0.42,
                ConsecutivePagesRead: 6
            )
        );

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = "/api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f"
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
                instance = "/api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the consumption record does not exist, or it does not belong to the current user.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "ConsumptionRecordNotFound",
                instance = "/api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f",
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
