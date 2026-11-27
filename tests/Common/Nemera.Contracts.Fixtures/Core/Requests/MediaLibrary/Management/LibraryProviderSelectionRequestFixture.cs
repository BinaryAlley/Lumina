#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="LibraryProviderSelectionRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryProviderSelectionRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="LibraryProviderSelectionRequest"/>.
    /// </summary>
    /// <param name="pluginId">Optional. The Id of the plugin providing the provider or reader.</param>
    /// <param name="isEnabled">Whether the provider or reader is enabled.</param>
    /// <param name="includeIsEnabled">Whether the choice carries an enabled state. When <see langword="false"/>, the enabled state is <see langword="null"/>.</param>
    /// <returns>The created <see cref="LibraryProviderSelectionRequest"/>.</returns>
    public LibraryProviderSelectionRequest Create(
        Guid? pluginId = null,
        bool isEnabled = true,
        bool includeIsEnabled = true)
    {
        return new LibraryProviderSelectionRequest(
            pluginId ?? Guid.NewGuid(),
            includeIsEnabled ? isEnabled : null);
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryProviderSelectionRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LibraryProviderSelectionRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
