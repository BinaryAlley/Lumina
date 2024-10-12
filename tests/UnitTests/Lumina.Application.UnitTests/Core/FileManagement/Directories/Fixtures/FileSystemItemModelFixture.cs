#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Models.FileManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Directories.Fixtures;

/// <summary>
/// Fixture class for the <see cref="FileSystemItemModel"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemItemModelFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="FileSystemItemModel"/>.
    /// </summary>
    /// <returns>The created <see cref="FileSystemItemModel"/>.</returns>
    public FileSystemItemModel Create()
    {
        return new FileSystemItemModel(
            _faker.System.FilePath(),
            _faker.System.FileName(),
            _faker.Date.Past(),
            _faker.Date.Recent()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="FileSystemItemModel"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<FileSystemItemModel> CreateMany(int count = 3)
    {
        return Enumerable.Range(0, count).Select(_ => Create()).ToList();
    }
}
