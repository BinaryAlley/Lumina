#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Plugins;

/// <summary>
/// Fixture class for generating <see cref="SetLibraryMetadataProviderEnabledRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryMetadataProviderEnabledRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="SetLibraryMetadataProviderEnabledRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="libraryId">Optional identifier of the media library.</param>
    /// <param name="pluginId">Optional identifier of the plugin.</param>
    /// <param name="isEnabled">Optional value indicating whether the metadata provider is enabled.</param>
    /// <returns>A configured <see cref="SetLibraryMetadataProviderEnabledRequest"/> instance.</returns>
    public SetLibraryMetadataProviderEnabledRequest Create(
        Guid? libraryId = null,
        Guid? pluginId = null,
        bool? isEnabled = null)
    {
        return new SetLibraryMetadataProviderEnabledRequest
        {
            LibraryId = libraryId ?? Guid.NewGuid(),
            PluginId = pluginId ?? Guid.NewGuid(),
            IsEnabled = isEnabled ?? _faker.Random.Bool()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="SetLibraryMetadataProviderEnabledRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="SetLibraryMetadataProviderEnabledRequest"/> instances.</returns>
    public List<SetLibraryMetadataProviderEnabledRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
