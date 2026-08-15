#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Plugins.Commands.ReorderLibraryMetadataProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Plugins.Commands.ReorderLibraryMetadataProviders;

/// <summary>
/// Fixture class for the <see cref="ReorderLibraryMetadataProvidersCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryMetadataProvidersCommandFixture
{
    /// <summary>
    /// Creates a random valid command to reorder the metadata providers of a media library.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose metadata providers are reordered.</param>
    /// <param name="pluginIds">Optional. The plugin Ids in the new order.</param>
    /// <returns>The created command.</returns>
    public ReorderLibraryMetadataProvidersCommand Create(Guid? libraryId = null, IReadOnlyList<Guid>? pluginIds = null)
    {
        return new Faker<ReorderLibraryMetadataProvidersCommand>()
            .CustomInstantiator(f => new ReorderLibraryMetadataProvidersCommand(
                default,
                default!))
            .RuleFor(x => x.LibraryId, libraryId ?? Guid.NewGuid())
            .RuleFor(x => x.PluginIds, f => pluginIds ?? [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()])
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="ReorderLibraryMetadataProvidersCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ReorderLibraryMetadataProvidersCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
