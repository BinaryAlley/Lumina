#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="LibraryContentLocationEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryContentLocationEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryContentLocationEntity"/>.
    /// </summary>
    /// <param name="path">Optional. The path of the media library content location.</param>
    /// <returns>The created <see cref="LibraryContentLocationEntity"/>.</returns>
    public LibraryContentLocationEntity Create(
        string? path = null)
    {
        return new LibraryContentLocationEntity
        {
            Path = path ?? _faker.System.DirectoryPath()
        };
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryContentLocationEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryContentLocationEntity"/> instances.</returns>
    public List<LibraryContentLocationEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
