#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;

/// <summary>
/// Fixture class for the <see cref="LibraryProviderSelectionDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryProviderSelectionDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryProviderSelectionDto"/>.
    /// </summary>
    /// <param name="pluginId">Optional. The Id of the plugin providing the provider or reader.</param>
    /// <param name="isEnabled">Optional. Whether the provider or reader is enabled.</param>
    /// <param name="includeIsEnabled">Whether the choice carries an enabled state. When <see langword="false"/>, the enabled state is <see langword="null"/>.</param>
    /// <returns>The created <see cref="LibraryProviderSelectionDto"/>.</returns>
    public LibraryProviderSelectionDto Create(
        Guid? pluginId = null,
        bool? isEnabled = null,
        bool includeIsEnabled = true)
    {
        return new LibraryProviderSelectionDto
        {
            PluginId = pluginId ?? Guid.NewGuid(),
            IsEnabled = includeIsEnabled ? isEnabled ?? _faker.Random.Bool() : null
        };
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryProviderSelectionDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LibraryProviderSelectionDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
