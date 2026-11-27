#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.GetResumePoint;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetResumePointEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetResumePointEndpointSummary : Summary<GetResumePointEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetResumePointEndpointSummary"/> class.
    /// </summary>
    public GetResumePointEndpointSummary()
    {
        Summary = "Retrieves the resume point of the current user for a media item.";
        Description = "Retrieves the point at which the current user can resume the media item named by the route. A book example is /users/me/media-resume-points/Book/{bookId}, and a music track example is /users/me/media-resume-points/MusicTrack/{trackId}. A music track resume point carries a TimeOffset position, while a book resume point carries a Location with an opaque locator. Only the supported MusicTrack and Book kinds can resume. A planned kind whose catalogue does not exist yet is rejected with UnsupportedMediaKind, while a photo has no position representation and is rejected with MediaKindCannotResume. A resume point of another user is reported with the same not found error as a missing resume point. When the media item no longer exists, or no longer belongs to the recorded library, the orphan resume point is deleted and the item is reported as not found.";

        Params["mediaKind"] = "The kind of media to resume. One of MusicTrack or Book. A Photo cannot resume. Only the member names are accepted; numeric values are rejected.";
        Params["mediaItemId"] = "The Id of the media item to resume.";

        ResponseParam<ResumePointResponse>(resumePoint => resumePoint.Id, "The Id of the resume point.");
        ResponseParam<ResumePointResponse>(resumePoint => resumePoint.LibraryId, "The Id of the media library that contains the media item.");
        ResponseParam<ResumePointResponse>(resumePoint => resumePoint.MediaItemId, "The Id of the media item to resume.");
        ResponseParam<ResumePointResponse>(resumePoint => resumePoint.MediaKind, "The kind of media that is resumed.");
        ResponseParam<ResumePointResponse>(resumePoint => resumePoint.LastPosition, "The position at which the media item can be resumed. It carries the position Kind, and either TimeOffsetSeconds or Location.");
        ResponseParam<ResumePointResponse>(resumePoint => resumePoint.LastProgressFraction, "The progress fraction at which the media item can be resumed.");
        ResponseParam<ResumePointResponse>(resumePoint => resumePoint.LastConsumedOnUtc, "The date and time when the media item was last consumed.");

        Response<ResumePointResponse>(statusCode: 200, description: "The resume point is returned.", contentType: "application/json",
            example: new ResumePointResponse(
                Id: Guid.Parse("4d5e6f70-8192-4a3b-9c4d-5e6f708192a3"),
                LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                MediaItemId: Guid.Parse("7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d"),
                MediaKind: MediaKind.Book,
                LastPosition: new MediaPositionResponse(MediaPositionKind.Location, null, "epubcfi(/6/4[chapter-3]!/4/2/2)"),
                LastProgressFraction: 0.42,
                LastConsumedOnUtc: new DateTime(2026, 10, 24, 12, 12, 0, DateTimeKind.Utc)
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
                    instance = "/api/v1/users/me/media-resume-points/Book/7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/media-resume-points/Book/7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d"
                }
            }
        );

        Response(403, "The request failed because the authenticated request does not carry the identity of a user, or because the kind of media cannot be resumed.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Unauthorized",
                    status = 403,
                    detail = "NotAuthorized",
                    instance = "/api/v1/users/me/media-resume-points/Photo/7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Forbidden",
                    status = 403,
                    detail = "MediaKindCannotResume",
                    instance = "/api/v1/users/me/media-resume-points/Photo/7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                }
            }
        );

        Response(404, "The request failed because the resume point does not exist, because the media item does not exist, or because the media item no longer belongs to the recorded library.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "MediaResumePointNotFound",
                    instance = "/api/v1/users/me/media-resume-points/Book/7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "TrackNotFound",
                    instance = "/api/v1/users/me/media-resume-points/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "BookNotFound",
                    instance = "/api/v1/users/me/media-resume-points/Book/7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
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
                instance = "/api/v1/users/me/media-resume-points/Book/7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
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
