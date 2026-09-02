#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Commands.SetLibraryBookReaderEnabled;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Plugins.Commands.SetLibraryBookReaderEnabled;

/// <summary>
/// Fixture class for the <see cref="SetLibraryBookReaderEnabledCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryBookReaderEnabledCommandFixture
{
    /// <summary>
    /// Creates a random valid command to enable or disable a book reader for a media library.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose book reader is enabled or disabled.</param>
    /// <param name="pluginId">Optional. The unique identifier of the plugin providing the book reader.</param>
    /// <param name="isEnabled">Optional. Whether the book reader should be enabled for the media library.</param>
    /// <returns>The created <see cref="SetLibraryBookReaderEnabledCommand"/>.</returns>
    public SetLibraryBookReaderEnabledCommand Create(
        Guid? libraryId = null,
        Guid? pluginId = null,
        bool? isEnabled = null)
    {
        return new SetLibraryBookReaderEnabledCommand(libraryId ?? Guid.NewGuid(), pluginId ?? Guid.NewGuid(), isEnabled ?? true);
    }

    /// <summary>
    /// Creates a list of <see cref="SetLibraryBookReaderEnabledCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<SetLibraryBookReaderEnabledCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
