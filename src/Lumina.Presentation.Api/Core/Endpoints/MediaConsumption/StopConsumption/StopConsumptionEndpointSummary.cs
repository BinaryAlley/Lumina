#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.StopConsumption;

/// <summary>
/// Class used for providing a textual description for the <see cref="StopConsumptionEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopConsumptionEndpointSummary : Summary<StopConsumptionEndpoint, StopConsumptionRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StopConsumptionEndpointSummary"/> class.
    /// </summary>
    public StopConsumptionEndpointSummary()
    {
        Summary = "Stops a consumption session of the current user.";
        Description = "Stops an in-progress consumption session of the current user, storing its final position, progress fraction, and active consumed duration. When the library tracks progress and the final progress reaches the library completion threshold, the session is recorded as completed instead of stopped and its resume point is deleted. Otherwise the session is recorded as stopped, and when the library tracks progress, the kind can resume, and a final position is known, its resume point is saved for the next time. A music track reports its final position as a TimeOffset with TimeOffsetSeconds, while a book reports it as a Location with an opaque locator. A record of another user is reported with the same not found error as a missing record.";

        ExampleRequest = new StopConsumptionRequest(
            Position: new MediaPositionRequest("TimeOffset", 60, null),
            ProgressFraction: 0.6,
            ConsumedDurationInSeconds: 60
        );
        RequestExamples.Add(new RequestExample(new StopConsumptionRequest(
            Position: new MediaPositionRequest("Location", null, "epubcfi(/6/6[chapter-2]!/4/2/2)"),
            ProgressFraction: 0.18,
            ConsumedDurationInSeconds: 300
        )));
        Params["recordId"] = "The Id of the consumption record.";

        RequestParam(request => request.Position, "The final position reported for the media item, if the client knows it. It carries the position Kind, and either TimeOffsetSeconds or Location. Optional.");
        RequestParam(request => request.ProgressFraction, "The final fraction of the media item that the client reached, between 0 and 1. Required.");
        RequestParam(request => request.ConsumedDurationInSeconds, "The active duration consumed by the session, in seconds. Required.");

        ResponseParam<ConsumptionRecordResponse>(record => record.Id, "The Id of the consumption record.");
        ResponseParam<ConsumptionRecordResponse>(record => record.LibraryId, "The Id of the media library that contains the consumed media item.");
        ResponseParam<ConsumptionRecordResponse>(record => record.MediaItemId, "The Id of the consumed media item.");
        ResponseParam<ConsumptionRecordResponse>(record => record.MediaKind, "The kind of media that was consumed.");
        ResponseParam<ConsumptionRecordResponse>(record => record.Status, "The state of the consumption session.");
        ResponseParam<ConsumptionRecordResponse>(record => record.StartedOnUtc, "The date and time when the session started.");
        ResponseParam<ConsumptionRecordResponse>(record => record.LastActivityOnUtc, "The date and time when the session last received activity.");
        ResponseParam<ConsumptionRecordResponse>(record => record.EndedOnUtc, "The date and time when the session completed or stopped.");
        ResponseParam<ConsumptionRecordResponse>(record => record.ConsumedDurationInSeconds, "The active duration consumed by the session, in seconds.");
        ResponseParam<ConsumptionRecordResponse>(record => record.LastPosition, "The final known position for the media item, if any. It carries the position Kind, and either TimeOffsetSeconds or Location.");
        ResponseParam<ConsumptionRecordResponse>(record => record.LastProgressFraction, "The final reported progress fraction of the media item.");
        ResponseParam<ConsumptionRecordResponse>(record => record.ConsecutivePagesRead, "The number of consecutive pages read in the session.");

        Response(statusCode: 200, description: "The consumption session is stopped, or completed when it reached the library threshold, and the updated record is returned.", contentType: "application/json",
            example: new ConsumptionRecordResponse(
                Id: Guid.Parse("2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f"),
                LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                MediaItemId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                MediaKind: MediaKind.MusicTrack,
                Status: ConsumptionStatus.Stopped,
                StartedOnUtc: new DateTime(2026, 10, 24, 12, 0, 0, DateTimeKind.Utc),
                LastActivityOnUtc: new DateTime(2026, 10, 24, 12, 1, 0, DateTimeKind.Utc),
                EndedOnUtc: new DateTime(2026, 10, 24, 12, 1, 0, DateTimeKind.Utc),
                ConsumedDurationInSeconds: 60,
                LastPosition: new MediaPositionResponse(MediaPositionKind.TimeOffset, 60, null),
                LastProgressFraction: 0.6,
                ConsecutivePagesRead: 0
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
                    instance = "/api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f/stop"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f/stop"
                }
            }
        );

        Response(403, "The request failed because the authenticated request does not carry the identity of a user, or because the reported consumed duration is lower than the duration already stored for the session.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Unauthorized",
                    status = 403,
                    detail = "NotAuthorized",
                    instance = "/api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f/stop",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Forbidden",
                    status = 403,
                    detail = "ConsumedDurationCannotDecrease",
                    instance = "/api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f/stop",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                }
            }
        );

        Response(404, "The request failed because the consumption record does not exist, or it does not belong to the current user.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "ConsumptionRecordNotFound",
                instance = "/api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f/stop",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );

        Response(409, "The request failed because the session is already terminal, or because another request changed it after this request read it.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "ConsumptionIsAlreadyTerminal",
                    instance = "/api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f/stop",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "ConcurrencyConflict",
                    instance = "/api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f/stop",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                }
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/users/me/consumption-records/2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f/stop",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "ConsumptionRecordIdCannotBeEmpty",
                            "InvalidMediaPosition",
                            "InvalidProgressFraction",
                            "ConsumedDurationMustBeNonNegative",
                            "ConsumedDurationIsOutOfRange"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
