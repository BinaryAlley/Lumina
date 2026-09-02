#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.SetLibraryBookReaderEnabled;

/// <summary>
/// Command for enabling or disabling a book reader for a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose book reader is enabled or disabled.</param>
/// <param name="PluginId">The unique identifier of the plugin providing the book reader.</param>
/// <param name="IsEnabled">Whether the book reader should be enabled for the media library.</param>
public record SetLibraryBookReaderEnabledCommand(
    Guid LibraryId,
    Guid PluginId,
    bool IsEnabled
) : ICommand;
