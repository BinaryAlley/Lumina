#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.Responses.Plugins;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="LibraryBookReaderConfigurationEntity"/>.
/// </summary>
public static class LibraryBookReaderConfigurationEntityMapping
{
    /// <summary>
    /// Converts <paramref name="configuration"/> to <see cref="LibraryBookReaderResponse"/>.
    /// </summary>
    /// <param name="configuration">The repository entity to be converted.</param>
    /// <param name="readerName">The display name of the book reader.</param>
    /// <param name="supportedExtensions">The file extensions supported by the book reader.</param>
    /// <returns>The converted response.</returns>
    public static LibraryBookReaderResponse ToResponse(this LibraryBookReaderConfigurationEntity configuration, string readerName, IReadOnlyList<string> supportedExtensions)
    {
        return new LibraryBookReaderResponse(
            configuration.PluginId,
            readerName,
            supportedExtensions,
            configuration.IsEnabled
        );
    }
}
