#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Core.Themes;

/// <summary>
/// Builds the audio player model for the themed component template, resolving the localized strings and theme asset URLs server side.
/// </summary>
public sealed class ThemeAudioPlayerBuilder
{
    // The resource base name deliberately excludes the "Core.Resources" segment: the localization factory re-roots it with the configured ResourcesPath.
    private const string VIEW_RESOURCE_BASE_NAME = "Lumina.Presentation.Web.Views.Shared._Layout";
    private const string VIEW_RESOURCE_LOCATION = "Lumina.Presentation.Web";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStringLocalizer _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeAudioPlayerBuilder"/> class.
    /// </summary>
    /// <param name="stringLocalizerFactory">Injected factory used to create the localizer of the audio player resources.</param>
    /// <param name="httpContextAccessor">Injected accessor for the current HTTP context.</param>
    public ThemeAudioPlayerBuilder(IStringLocalizerFactory stringLocalizerFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _localizer = stringLocalizerFactory.Create(VIEW_RESOURCE_BASE_NAME, VIEW_RESOURCE_LOCATION);
    }

    /// <summary>
    /// Builds the model of the audio player for the given theme.
    /// </summary>
    /// <param name="assetBase">The base URL of the assets of the active theme.</param>
    /// <param name="queueItemTemplate">The raw template source of a queue item, or an empty string when the theme does not provide one.</param>
    /// <param name="playlistItemTemplate">The raw template source of a playlist item, or an empty string when the theme does not provide one.</param>
    /// <returns>The model available to the themed audio player template.</returns>
    public ThemeAudioPlayerDto Build(string assetBase, string queueItemTemplate, string playlistItemTemplate)
    {
        Dictionary<string, object?> strings = new()
        {
            ["toggleFullHeight"] = Localize("ToggleFullHeight"),
            ["toggleVisibility"] = Localize("ToggleVisibility"),
            ["previous"] = Localize("AudioPlayerPrevious"),
            ["next"] = Localize("AudioPlayerNext"),
            ["shuffle"] = Localize("AudioPlayerShuffle"),
            ["shuffleOff"] = Localize("AudioPlayerShuffleOff"),
            ["shuffleRelease"] = Localize("AudioPlayerShuffleRelease"),
            ["shuffleArtist"] = Localize("AudioPlayerShuffleArtist"),
            ["shuffleLibrary"] = Localize("AudioPlayerShuffleLibrary"),
            ["repeat"] = Localize("AudioPlayerRepeat"),
            ["repeatOff"] = Localize("AudioPlayerRepeatOff"),
            ["repeatAll"] = Localize("AudioPlayerRepeatAll"),
            ["repeatOne"] = Localize("AudioPlayerRepeatOne"),
            ["queue"] = Localize("AudioPlayerQueue")
        };

        return new ThemeAudioPlayerDto(
            AppBase: GetAppBaseUrl(),
            AssetBase: assetBase,
            QueueItemTemplate: queueItemTemplate,
            PlaylistItemTemplate: playlistItemTemplate,
            Strings: strings);
    }

    /// <summary>
    /// Gets the absolute base URL of the current Web application request, used to reference the application scripts from a static theme template.
    /// </summary>
    /// <returns>The absolute base URL with a trailing slash, or an empty string when the current HTTP context is unavailable.</returns>
    private string GetAppBaseUrl()
    {
        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
            return string.Empty;

        return $"{request.Scheme}://{request.Host}{request.PathBase}/";
    }

    /// <summary>
    /// Resolves a localized audio player string.
    /// </summary>
    /// <param name="key">The resource key of the string.</param>
    /// <returns>The localized string.</returns>
    private string Localize(string key)
    {
        return _localizer[key].Value;
    }
}
