#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Queries.GetLibraryTypeBookReaders;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Plugins.Queries.GetLibraryTypeBookReaders;

/// <summary>
/// Fixture class for the <see cref="GetLibraryTypeBookReadersQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeBookReadersQueryFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetLibraryTypeBookReadersQuery"/>.
    /// </summary>
    /// <param name="libraryType">Optional. The media library type whose available book readers are retrieved.</param>
    /// <returns>The created <see cref="GetLibraryTypeBookReadersQuery"/>.</returns>
    public GetLibraryTypeBookReadersQuery Create(string? libraryType = null)
    {
        return new GetLibraryTypeBookReadersQuery(libraryType ?? "Book");
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryTypeBookReadersQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryTypeBookReadersQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
