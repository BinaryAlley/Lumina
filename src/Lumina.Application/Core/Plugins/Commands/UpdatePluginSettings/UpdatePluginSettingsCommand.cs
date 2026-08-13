#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.UpdatePluginSettings;

/// <summary>
/// Command for updating the settings of a plugin.
/// </summary>
/// <param name="PluginId">The unique identifier of the plugin.</param>
/// <param name="Settings">The settings of the plugin.</param>
public record UpdatePluginSettingsCommand(
    Guid PluginId,
    IReadOnlyDictionary<string, string>? Settings
) : ICommand;
