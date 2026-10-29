#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.StartConsumption;

/// <summary>
/// Class used for providing a textual description for the <see cref="StartConsumptionEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartConsumptionEndpointSummary : Summary<StartConsumptionEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartConsumptionEndpointSummary"/> class.
    /// </summary>
    public StartConsumptionEndpointSummary()
    {
        Summary = "Starts a media consumption session of the current user.";
        Description = "Starts a consumption record for the media item named by the route, in the library named by the route. The supported kinds are MusicTrack and Book. A music track example is /libraries/{libraryId}/media-items/MusicTrack/{trackId}/consumption-records, and a book example is /libraries/{libraryId}/media-items/Book/{bookId}/consumption-records. When the library tracks progress and the kind can resume, the new record starts from the stored resume point; the consumed duration always starts at zero. A missing media item and a media item that belongs to another library are reported with the same not found error, so the route cannot be used to probe another library. The record belongs to the current user, and it never stores a queue, a current index, a repeat or shuffle state, or a playback session.";

        Params["libraryId"] = "The Id of the media library that contains the media item.";
        Params["mediaKind"] = "The kind of media that is consumed. One of MusicTrack or Book. Only the member names are accepted; numeric values are rejected.";
        Params["mediaItemId"] = "The Id of the consumed media item.";

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

        Response(statusCode: 200, description: "The consumption record is started and returned.", contentType: "application/json",
            example: new ConsumptionRecordResponse(
                Id: Guid.Parse("2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f"),
                LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                MediaItemId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                MediaKind: MediaKind.MusicTrack,
                Status: ConsumptionStatus.InProgress,
                StartedOnUtc: new DateTime(2026, 10, 24, 12, 0, 0, DateTimeKind.Utc),
                LastActivityOnUtc: new DateTime(2026, 10, 24, 12, 0, 0, DateTimeKind.Utc),
                EndedOnUtc: null,
                ConsumedDurationInSeconds: 0,
                LastPosition: null,
                LastProgressFraction: 0,
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
                    instance = "/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/media-items/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/consumption-records"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/media-items/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/consumption-records"
                }
            }
        );

        Response(403, "The request failed because the authenticated request does not carry the identity of a user, or because the current user cannot access the library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/media-items/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/consumption-records",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the library does not exist, or because the media item does not exist, or does not belong to the library named by the route.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "LibraryNotFound",
                    instance = "/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/media-items/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/consumption-records",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "TrackNotFound",
                    instance = "/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/media-items/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/consumption-records",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "BookNotFound",
                    instance = "/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/media-items/Book/7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d/consumption-records",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                }
            }
        );

        Response(409, "The request failed because the consumption record could not be stored, because the database is busy or locked. Repeating the request is safe.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                title = "General.Conflict",
                status = 409,
                detail = "ConcurrencyConflict",
                instance = "/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/media-items/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/consumption-records",
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
                instance = "/api/v1/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/media-items/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/consumption-records",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty",
                            "MediaItemIdCannotBeEmpty",
                            "UnsupportedMediaKind"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
