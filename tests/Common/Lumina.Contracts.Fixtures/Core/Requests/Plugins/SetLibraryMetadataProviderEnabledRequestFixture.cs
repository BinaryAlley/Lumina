#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Plugins;

/// <summary>
/// Fixture class for the <see cref="SetLibraryMetadataProviderEnabledRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryMetadataProviderEnabledRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="SetLibraryMetadataProviderEnabledRequest"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose metadata provider is enabled or disabled.</param>
    /// <param name="pluginId">Optional. The Id of the plugin providing the metadata.</param>
    /// <param name="isEnabled">Optional. Whether the metadata provider should be enabled.</param>
    /// <returns>The created <see cref="SetLibraryMetadataProviderEnabledRequest"/>.</returns>
    public SetLibraryMetadataProviderEnabledRequest Create(
        Guid? libraryId = null,
        Guid? pluginId = null,
        bool? isEnabled = null)
    {
        return new SetLibraryMetadataProviderEnabledRequest(
            libraryId ?? _faker.Random.Guid(),
            pluginId ?? _faker.Random.Guid(),
            isEnabled ?? _faker.Random.Bool()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="SetLibraryMetadataProviderEnabledRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<SetLibraryMetadataProviderEnabledRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
