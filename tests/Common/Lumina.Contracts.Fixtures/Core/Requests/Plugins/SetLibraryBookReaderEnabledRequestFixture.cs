#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Plugins;

/// <summary>
/// Fixture class for the <see cref="SetLibraryBookReaderEnabledRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryBookReaderEnabledRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="SetLibraryBookReaderEnabledRequest"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose book reader is enabled or disabled.</param>
    /// <param name="pluginId">Optional. The unique identifier of the plugin providing the book reader.</param>
    /// <param name="isEnabled">Optional. Whether the book reader should be enabled for the media library.</param>
    /// <returns>The created <see cref="SetLibraryBookReaderEnabledRequest"/>.</returns>
    public SetLibraryBookReaderEnabledRequest Create(
        Guid? libraryId = null,
        Guid? pluginId = null,
        bool? isEnabled = null)
    {
        return new SetLibraryBookReaderEnabledRequest(libraryId ?? Guid.NewGuid(), pluginId ?? Guid.NewGuid(), isEnabled ?? true);
    }

    /// <summary>
    /// Creates a list of <see cref="SetLibraryBookReaderEnabledRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<SetLibraryBookReaderEnabledRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
