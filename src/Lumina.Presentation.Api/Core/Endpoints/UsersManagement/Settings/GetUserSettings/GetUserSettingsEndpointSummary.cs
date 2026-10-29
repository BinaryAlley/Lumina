#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.GetUserSettings;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetUserSettingsEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserSettingsEndpointSummary : Summary<GetUserSettingsEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserSettingsEndpointSummary"/> class.
    /// </summary>
    public GetUserSettingsEndpointSummary()
    {
        Summary = "Retrieves the settings of the current user.";
        Description = "Retrieves the settings of the current user. When no settings are stored for the user yet, the default settings are returned.";

        ResponseParam<UserSettingsResponse>(r => r.UserId, "The unique identifier of the user that owns these settings.");
        ResponseParam<UserSettingsResponse>(r => r.IsPaginationEnabled, "Whether pagination is enabled for the user, or not.");
        ResponseParam<UserSettingsResponse>(r => r.ItemsPerPage, "The number of library items displayed per page when pagination is enabled.");
        ResponseParam<UserSettingsResponse>(r => r.ShouldIgnoreThePrefixForAlphaPicker, "Whether the \"The\" prefix of library item titles is ignored by the alpha picker, or not.");
        ResponseParam<UserSettingsResponse>(r => r.IsThemeCachingEnabled, "Whether the theme data served to this user is cached, or not.");
        ResponseParam<UserSettingsResponse>(r => r.ShouldAggregateMetadataWhenMissing, "Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing, or not.");
        ResponseParam<UserSettingsResponse>(r => r.ShouldAggregateArtworkWhenMissing, "Whether the artwork of the media library items is aggregated from multiple providers, when it is missing, or not.");
        ResponseParam<UserSettingsResponse>(r => r.ShouldRenderPdfAsImages, "Whether PDF books are rendered as page images for the user, or not.");
        ResponseParam<UserSettingsResponse>(r => r.ShouldPreserveBookStyles, "Whether the styles of the book content are preserved when it is rendered for the user, or not.");
        ResponseParam<UserSettingsResponse>(r => r.ShouldKeepDownloadingWhilePaused, "Whether the player keeps downloading a track while playback is paused, or not.");
        ResponseParam<UserSettingsResponse>(r => r.ShouldAutoplay, "Whether the player continues playback at the end of an album or artist, or not.");
        ResponseParam<UserSettingsResponse>(r => r.IsGaplessPlaybackEnabled, "Whether the player transitions between consecutive tracks without a gap, or not.");
        ResponseParam<UserSettingsResponse>(r => r.ReplayGainMode, "The way ReplayGain normalization is applied to the tracks the user plays.");
        ResponseParam<UserSettingsResponse>(r => r.StreamingQuality, "The quality cap applied to the audio the server normalizes for the user.");
        ResponseParam<UserSettingsResponse>(r => r.VuMeterOrientation, "The direction in which each channel of the VU meter of the audio player grows.");
        ResponseParam<UserSettingsResponse>(r => r.VuMeterSegmentCount, "The number of segments each channel of the VU meter is divided into.");
        ResponseParam<UserSettingsResponse>(r => r.VuMeterSegmentGap, "The gap, in pixels, between the segments and the channels of the VU meter.");
        ResponseParam<UserSettingsResponse>(r => r.VuMeterMediumThreshold, "The level, between 0 and 1, above which a segment of the VU meter is drawn with its medium state.");
        ResponseParam<UserSettingsResponse>(r => r.VuMeterHighThreshold, "The level, between 0 and 1, above which a segment of the VU meter is drawn with its high state.");
        ResponseParam<UserSettingsResponse>(r => r.VuMeterShowPeak, "Whether the VU meter holds the peak level of each channel after it falls, or not.");
        ResponseParam<UserSettingsResponse>(r => r.VuMeterPeakHoldDurationInMilliseconds, "The time, in milliseconds, a peak of the VU meter is held before it starts falling.");
        ResponseParam<UserSettingsResponse>(r => r.VuMeterReleasePerSecond, "The number of level units the display of the VU meter falls per second when the input level drops.");
        ResponseParam<UserSettingsResponse>(r => r.VuMeterPeakReleasePerSecond, "The number of level units the peak of the VU meter falls per second after its hold time elapsed.");

        Response(200, "The settings of the current user are returned.",
            example: new UserSettingsResponse(
                UserId: Guid.NewGuid(),
                IsPaginationEnabled: true,
                ItemsPerPage: 48,
                ShouldIgnoreThePrefixForAlphaPicker: false,
                IsThemeCachingEnabled: true,
                ShouldAggregateMetadataWhenMissing: false,
                ShouldAggregateArtworkWhenMissing: false,
                ShouldRenderPdfAsImages: false,
                ShouldPreserveBookStyles: true,
                ShouldKeepDownloadingWhilePaused: true,
                ShouldAutoplay: true,
                IsGaplessPlaybackEnabled: true,
                ReplayGainMode: ReplayGainMode.Off,
                StreamingQuality: PlaybackStreamingQuality.Auto,
                VuMeterOrientation: VuMeterOrientation.Vertical,
                VuMeterSegmentCount: 12,
                VuMeterSegmentGap: 1,
                VuMeterMediumThreshold: 0.6,
                VuMeterHighThreshold: 0.8,
                VuMeterShowPeak: true,
                VuMeterPeakHoldDurationInMilliseconds: 650,
                VuMeterReleasePerSecond: 3.0,
                VuMeterPeakReleasePerSecond: 0.65
            ));

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = "/api/v1/users/me/settings"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/settings"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/users/me/settings"
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
                instance = "/api/v1/users/me/settings",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );
    }
}
