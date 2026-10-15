#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.UpdateUserSettings;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdateUserSettingsEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsEndpointSummary : Summary<UpdateUserSettingsEndpoint, UpdateUserSettingsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsEndpointSummary"/> class.
    /// </summary>
    public UpdateUserSettingsEndpointSummary()
    {
        Summary = "Updates the settings of the current user.";
        Description = "Updates the settings of the current user. When no settings are stored for the user yet, they are created with the provided values.";

        ExampleRequest = new UpdateUserSettingsRequest(
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
            StreamingQuality: PlaybackStreamingQuality.Auto
        );

        RequestParam(r => r.IsPaginationEnabled, "Whether pagination is enabled for the user, or not. Required.");
        RequestParam(r => r.ItemsPerPage, "The number of library items displayed per page when pagination is enabled. Required.");
        RequestParam(r => r.ShouldIgnoreThePrefixForAlphaPicker, "Whether the \"The\" prefix of library item titles is ignored by the alpha picker, or not. Required.");
        RequestParam(r => r.IsThemeCachingEnabled, "Whether the theme data served to this user is cached, or not. Required.");
        RequestParam(r => r.ShouldAggregateMetadataWhenMissing, "Whether the metadata of the media library items is aggregated from multiple providers, when fields are missing, or not. Required.");
        RequestParam(r => r.ShouldAggregateArtworkWhenMissing, "Whether the artwork of the media library items is aggregated from multiple providers, when it is missing, or not. Required.");
        RequestParam(r => r.ShouldRenderPdfAsImages, "Whether PDF books are rendered as page images for the user, or not. Required.");
        RequestParam(r => r.ShouldPreserveBookStyles, "Whether the styles of the book content are preserved when it is rendered for the user, or not. Required.");
        RequestParam(r => r.ShouldKeepDownloadingWhilePaused, "Whether the player keeps downloading a track while playback is paused, or not. Required.");
        RequestParam(r => r.ShouldAutoplay, "Whether the player continues playback at the end of an album or artist, or not. Required.");
        RequestParam(r => r.IsGaplessPlaybackEnabled, "Whether the player transitions between consecutive tracks without a gap, or not. Required.");
        RequestParam(r => r.ReplayGainMode, "The way ReplayGain normalization is applied to the tracks the user plays. Required.");
        RequestParam(r => r.StreamingQuality, "The quality cap applied to the audio the server normalizes for the user. Required.");

        Response(200, "The settings of the current user were successfully updated.");

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

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/users/me/settings",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "ItemsPerPageMustBeGreaterThanZero",
                            "ShouldKeepDownloadingWhilePausedCannotBeNull",
                            "ShouldAutoplayCannotBeNull",
                            "IsGaplessPlaybackEnabledCannotBeNull",
                            "ReplayGainModeCannotBeNull",
                            "StreamingQualityCannotBeNull",
                            "InvalidReplayGainMode",
                            "InvalidStreamingQuality"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
