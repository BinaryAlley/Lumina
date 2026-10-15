#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetPlaybackManifest;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPlaybackManifestEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackManifestEndpointSummary : Summary<GetPlaybackManifestEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackManifestEndpointSummary"/> class.
    /// </summary>
    public GetPlaybackManifestEndpointSummary()
    {
        Summary = "Retrieves the playback manifest of a track.";
        Description = "Retrieves the playback manifest of a track, containing the format of its stored audio, the delivery the server decided on for the requesting player, and the path of its stream. The player advertises the container and codec pairs it can demux and decode directly in the optional capabilities query parameter, as a comma separated list of container/codec pairs; when the parameter is absent, empty or lists no valid pair, the default capabilities of the bundled player apply. The manifest is returned to an Admin, who can build the manifest of the tracks of all libraries, or to the owner of the library of the track.";

        ResponseParam<PlaybackManifestResponse>(r => r.TrackId, "The Id of the track the manifest describes.");
        ResponseParam<PlaybackManifestResponse>(r => r.DurationInSeconds, "The duration of the audio of the track, in seconds.");
        ResponseParam<PlaybackManifestResponse>(r => r.SourceContainer, "The container of the stored audio file of the track, for example MP3, FLAC, OGG, AAC, MP4 or WAV.");
        ResponseParam<PlaybackManifestResponse>(r => r.SourceCodec, "The codec of the stored audio of the track.");
        ResponseParam<PlaybackManifestResponse>(r => r.SampleRate, "The sample rate of the audio of the track, in Hz.");
        ResponseParam<PlaybackManifestResponse>(r => r.Channels, "The number of audio channels of the track.");
        ResponseParam<PlaybackManifestResponse>(r => r.BitDepth, "The bit depth of the audio of the track, if applicable.");
        ResponseParam<PlaybackManifestResponse>(r => r.ReplayGainTrackGain, "The ReplayGain track gain of the track, in decibels, if applicable.");
        ResponseParam<PlaybackManifestResponse>(r => r.ReplayGainTrackPeak, "The ReplayGain track peak of the track, if applicable.");
        ResponseParam<PlaybackManifestResponse>(r => r.ReplayGainAlbumGain, "The ReplayGain album gain of the track, in decibels, if applicable.");
        ResponseParam<PlaybackManifestResponse>(r => r.ReplayGainAlbumPeak, "The ReplayGain album peak of the track, if applicable.");
        ResponseParam<PlaybackManifestResponse>(r => r.DirectPlay, "Whether the player can play the stored audio as it is, without the server changing its container or its codec.");
        ResponseParam<PlaybackManifestResponse>(r => r.DeliveryContainer, "The container of the audio the server delivers, when the delivery is not a direct play.");
        ResponseParam<PlaybackManifestResponse>(r => r.DeliveryCodec, "The codec of the audio the server delivers, when the delivery is not a direct play.");
        ResponseParam<PlaybackManifestResponse>(r => r.DeliveryShape, "The shape the delivered audio is addressed and seeked by, when the delivery is not a direct play.");
        ResponseParam<PlaybackManifestResponse>(r => r.SegmentDurationSeconds, "The duration of each segment of the delivered audio, in seconds, when the delivery is addressed as time addressed segments.");
        ResponseParam<PlaybackManifestResponse>(r => r.Seekable, "Whether the delivered audio can be seeked by the player.");
        ResponseParam<PlaybackManifestResponse>(r => r.StreamPath, "The path of the track stream, relative to the API, without a host or a scheme.");

        Response(200, "The playback manifest of the track is returned.",
            example: new PlaybackManifestResponse(
                TrackId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                DurationInSeconds: 354,
                SourceContainer: "FLAC",
                SourceCodec: "FLAC",
                SampleRate: 44100,
                Channels: 2,
                BitDepth: 16,
                ReplayGainTrackGain: -7.5m,
                ReplayGainTrackPeak: 0.9877m,
                ReplayGainAlbumGain: -6.8m,
                ReplayGainAlbumPeak: 0.9999m,
                DirectPlay: false,
                DeliveryContainer: "MP3",
                DeliveryCodec: "MP3",
                DeliveryShape: PlaybackDeliveryShape.TimeAddressedSegments,
                SegmentDurationSeconds: 6,
                Seekable: true,
                StreamPath: "/libraries/3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a/artists/d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/tracks/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f/stream"
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
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/playback-manifest"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/playback-manifest"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/playback-manifest"
                }
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library, or because the server cannot produce any format the player advertises it supports.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Unauthorized",
                    status = 403,
                    detail = "NotAuthorized",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/playback-manifest",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Forbidden",
                    status = 403,
                    detail = "NoSupportedPlaybackFormat",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/playback-manifest",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                }
            }
        );

        Response(404, "The request failed because the track or its media library does not exist, or the track does not belong to the library, album and artist named by the route.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "TrackNotFound",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/playback-manifest",
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/playback-manifest",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty",
                            "ArtistIdCannotBeEmpty",
                            "AlbumIdCannotBeEmpty",
                            "TrackIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
