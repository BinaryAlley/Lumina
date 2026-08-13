#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;

/// <summary>
/// Command for enabling or disabling a metadata provider for a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose metadata provider is enabled or disabled.</param>
/// <param name="PluginId">The unique identifier of the plugin providing the metadata.</param>
/// <param name="IsEnabled">Whether the metadata provider should be enabled for the media library.</param>
public record SetLibraryMetadataProviderEnabledCommand(
    Guid LibraryId,
    Guid PluginId,
    bool IsEnabled
) : ICommand;
