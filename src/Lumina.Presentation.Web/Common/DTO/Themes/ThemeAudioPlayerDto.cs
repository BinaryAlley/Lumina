#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Themes;

/// <summary>
/// Data transfer object for the model available to the themed audio player template.
/// </summary>
/// <param name="AppBase">The absolute base URL of the Web application, used to reference application scripts from the theme template.</param>
/// <param name="AssetBase">The base URL of the theme assets, used by the theme template to reference its own assets.</param>
/// <param name="QueueItemTemplate">The raw template source of a queue item, rendered client side for each track of the play context.</param>
/// <param name="PlaylistItemTemplate">The raw template source of a playlist item, rendered client side for each entry of a saved playlist.</param>
/// <param name="Strings">The localized strings of the audio player, keyed by resource name.</param>
[DebuggerDisplay("AssetBase: {AssetBase}")]
public sealed record ThemeAudioPlayerDto(
    string AppBase,
    string AssetBase,
    string QueueItemTemplate,
    string PlaylistItemTemplate,
    IReadOnlyDictionary<string, object?> Strings
);
