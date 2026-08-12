#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Plugins;

/// <summary>
/// Represents a detected plugin.
/// </summary>
/// <param name="Id">The unique identifier of the plugin.</param>
/// <param name="Name">The display name of the plugin.</param>
/// <param name="Author">The author of the plugin.</param>
/// <param name="Version">The version of the plugin.</param>
/// <param name="Description">The description of the plugin.</param>
/// <param name="LoadStatus">The load status of the plugin.</param>
/// <param name="LoadError">The error message when the plugin failed to load, if applicable.</param>
/// <param name="Settings">The settings of the plugin.</param>
[DebuggerDisplay("Name: {Name}")]
public sealed record PluginResponse(
    Guid Id,
    string Name,
    string Author,
    string Version,
    string Description,
    PluginLoadStatus LoadStatus,
    string? LoadError,
    IReadOnlyDictionary<string, string>? Settings
);
