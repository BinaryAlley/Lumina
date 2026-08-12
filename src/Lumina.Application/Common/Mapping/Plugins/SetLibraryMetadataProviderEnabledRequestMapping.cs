#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Commands.SetLibraryMetadataProviderEnabled;
using Lumina.Contracts.Requests.Plugins;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="SetLibraryMetadataProviderEnabledRequest"/>.
/// </summary>
public static class SetLibraryMetadataProviderEnabledRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="SetLibraryMetadataProviderEnabledCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static SetLibraryMetadataProviderEnabledCommand ToCommand(this SetLibraryMetadataProviderEnabledRequest request)
    {
        return new SetLibraryMetadataProviderEnabledCommand(request.LibraryId, request.PluginId, request.IsEnabled);
    }
}
