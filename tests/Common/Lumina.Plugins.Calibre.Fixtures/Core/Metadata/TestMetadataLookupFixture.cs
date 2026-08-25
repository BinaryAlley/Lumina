#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.Calibre.Fixtures.Core.Metadata;

/// <summary>
/// Fixture class for the <see cref="TestMetadataLookup"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class TestMetadataLookupFixture
{
    /// <summary>
    /// Creates a <see cref="TestMetadataLookup"/> that is not a book lookup.
    /// </summary>
    /// <param name="libraryId">The Id of the media library, or <see langword="null"/> to generate a random one.</param>
    /// <param name="path">The file system path of the media item.</param>
    /// <returns>The created <see cref="TestMetadataLookup"/>.</returns>
    public TestMetadataLookup Create(
        Guid? libraryId = null,
        string? path = null)
    {
        return new TestMetadataLookup(libraryId ?? Guid.NewGuid(), path ?? "/some/path");
    }

    /// <summary>
    /// Creates a list of <see cref="TestMetadataLookup"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<TestMetadataLookup> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
