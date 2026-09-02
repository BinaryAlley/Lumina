#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Commands.SetLibraryBookReaderEnabled;
using Lumina.Contracts.Requests.Plugins;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="SetLibraryBookReaderEnabledRequest"/>.
/// </summary>
public static class SetLibraryBookReaderEnabledRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="SetLibraryBookReaderEnabledCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static SetLibraryBookReaderEnabledCommand ToCommand(this SetLibraryBookReaderEnabledRequest request)
    {
        return new SetLibraryBookReaderEnabledCommand(
            request.LibraryId, 
            request.PluginId, 
            request.IsEnabled
        );
    }
}
