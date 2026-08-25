#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Plugins.Commands.ReorderLibraryArtworkProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Plugins.Commands.ReorderLibraryArtworkProviders;

/// <summary>
/// Fixture class for the <see cref="ReorderLibraryArtworkProvidersCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryArtworkProvidersCommandFixture
{
    /// <summary>
    /// Creates a random valid command to reorder the artwork providers of a media library.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose artwork providers are reordered.</param>
    /// <param name="pluginIds">Optional. The plugin Ids in the new order.</param>
    /// <returns>The created command.</returns>
    public ReorderLibraryArtworkProvidersCommand Create(
        Guid? libraryId = null,
        IReadOnlyList<Guid>? pluginIds = null)
    {
        return new Faker<ReorderLibraryArtworkProvidersCommand>()
            .CustomInstantiator(f => new ReorderLibraryArtworkProvidersCommand(
                default,
                default!))
            .RuleFor(x => x.LibraryId, libraryId ?? Guid.NewGuid())
            .RuleFor(x => x.PluginIds, f => pluginIds ?? [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()])
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="ReorderLibraryArtworkProvidersCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ReorderLibraryArtworkProvidersCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
