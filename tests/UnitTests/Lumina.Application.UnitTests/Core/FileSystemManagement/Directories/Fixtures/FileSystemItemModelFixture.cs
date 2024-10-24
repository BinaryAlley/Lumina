#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Entities.FileSystemManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Fixtures;

/// <summary>
/// Fixture class for the <see cref="FileSystemItemEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemItemModelFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="FileSystemItemEntity"/>.
    /// </summary>
    /// <returns>The created <see cref="FileSystemItemEntity"/>.</returns>
    public FileSystemItemEntity Create()
    {
        return new FileSystemItemEntity(
            _faker.System.FilePath(),
            _faker.System.FileName(),
            _faker.Date.Past(),
            _faker.Date.Recent()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="FileSystemItemEntity"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<FileSystemItemEntity> CreateMany(int count = 3)
    {
        return Enumerable.Range(0, count).Select(_ => Create()).ToList();
    }
}
