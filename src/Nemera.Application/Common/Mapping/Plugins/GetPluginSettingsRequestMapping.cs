#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Plugins.Queries.GetPluginSettings;
using Nemera.Contracts.Requests.Plugins;
#endregion

namespace Nemera.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="GetPluginSettingsRequest"/>.
/// </summary>
public static class GetPluginSettingsRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetPluginSettingsQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetPluginSettingsQuery ToQuery(this GetPluginSettingsRequest request)
    {
        return new GetPluginSettingsQuery(request.PluginId);
    }
}
