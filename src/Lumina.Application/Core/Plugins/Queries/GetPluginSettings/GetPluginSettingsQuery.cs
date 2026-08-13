#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetPluginSettings;

/// <summary>
/// Query for getting the settings of a plugin and their schema.
/// </summary>
/// <param name="PluginId">The unique identifier of the plugin.</param>
public record GetPluginSettingsQuery(
    Guid PluginId
) : IQuery;
