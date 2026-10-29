#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrackSegment;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetTrackSegmentEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTrackSegmentEndpointSummary : Summary<GetTrackSegmentEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackSegmentEndpointSummary"/> class.
    /// </summary>
    public GetTrackSegmentEndpointSummary()
    {
        Summary = "Streams one segment of the normalized audio of a track.";
        Description = "Streams a single fixed duration segment of the normalized audio of the track identified by the route. A track that is not a direct play is delivered as fixed duration, time addressed segments, so a player seeks by segment index and never maps a byte range to a time. The delivery handle is the opaque value the playback manifest reports in its deliveryId, and the segment index is zero based; together they name a segment of exactly the delivery the manifest declared, including its quality cap and the source file version it was declared for. Library ownership is re-evaluated on every request, while track membership and content locations are those captured at manifest time, so a deleted track row whose file remains can stay streamable until the handle expires. The first request for a track may wait for the whole normalized audio to be produced and cached. The segment is streamed to an Admin, who can stream the tracks of all libraries, or to the owner of the library of the track.";

        Response(200, "The requested segment is streamed, with the media type of the delivered container, or application/octet-stream when the container is not an audio type, with the length of the segment as its content length, and with the segment count and its exact start time on the track timeline as the X-Lumina-Segment-Count and X-Segment-Start-Seconds headers.", "*/*");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/stream/deliveries/8f14e45fceea167a5a36dedd4bea2543/segments/0"
                }
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/stream/deliveries/8f14e45fceea167a5a36dedd4bea2543/segments/0",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the delivery handle is unknown, expired, or was issued to another user or track, the track or its media library does not exist, the track does not belong to the library, album and artist named by the route, the audio file of the track is outside the content locations of its library, missing, or unreadable, or the requested segment does not exist. Only the NormalizedTrackSegmentNotFound detail means the end of the normalized audio; every other not found detail is a playback failure.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "TrackNotFound",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/stream/deliveries/8f14e45fceea167a5a36dedd4bea2543/segments/0",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "PlaybackDeliveryNotFound",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/stream/deliveries/8f14e45fceea167a5a36dedd4bea2543/segments/0",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "NormalizedTrackSegmentNotFound",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/stream/deliveries/8f14e45fceea167a5a36dedd4bea2543/segments/999",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "NormalizedTrackCacheEntryNotFound",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/stream/deliveries/8f14e45fceea167a5a36dedd4bea2543/segments/999",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                }
            }
        );

        Response(409, "The delivery handle does not describe time addressed segments, or the source file changed since the manifest declared the delivery. The player must request the playback manifest again.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                title = "General.Conflict",
                status = 409,
                detail = "PlaybackDeliveryChanged",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/stream/deliveries/8f14e45fceea167a5a36dedd4bea2543/segments/0",
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/stream/deliveries/not-a-handle/segments/-1",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty",
                            "ArtistIdCannotBeEmpty",
                            "AlbumIdCannotBeEmpty",
                            "TrackIdCannotBeEmpty",
                            "DeliveryIdCannotBeEmpty",
                            "DeliveryIdIsInvalid",
                            "SegmentIndexMustBeValid"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );

        Response(503, "Every normalization job slot, globally or for this user, is in use. The condition is transient and the request can be retried.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.6.4",
                title = "General.ResourceUnavailable",
                status = 503,
                detail = "NormalizationUnavailable",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/stream/deliveries/8f14e45fceea167a5a36dedd4bea2543/segments/0",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );
    }
}
