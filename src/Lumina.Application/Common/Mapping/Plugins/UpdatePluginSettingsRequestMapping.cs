#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Commands.UpdatePluginSettings;
using Lumina.Contracts.Requests.Plugins;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="UpdatePluginSettingsRequest"/>.
/// </summary>
public static class UpdatePluginSettingsRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="UpdatePluginSettingsCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static UpdatePluginSettingsCommand ToCommand(this UpdatePluginSettingsRequest request)
    {
        return new UpdatePluginSettingsCommand(request.PluginId, request.Settings);
    }
}
