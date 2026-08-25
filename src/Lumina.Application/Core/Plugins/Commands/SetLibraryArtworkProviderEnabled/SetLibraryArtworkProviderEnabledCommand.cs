#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.SetLibraryArtworkProviderEnabled;

/// <summary>
/// Command for enabling or disabling an artwork provider for a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose artwork provider is enabled or disabled.</param>
/// <param name="PluginId">The unique identifier of the plugin providing the artwork.</param>
/// <param name="IsEnabled">Whether the artwork provider should be enabled for the media library.</param>
public record SetLibraryArtworkProviderEnabledCommand(
    Guid LibraryId,
    Guid PluginId,
    bool IsEnabled
) : ICommand;
