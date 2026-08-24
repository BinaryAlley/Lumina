#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Common.Enums.FileSystem;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;

/// <summary>
/// Fixture class for generating <see cref="FileSystemTreeNodeDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemTreeNodeDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="FileSystemTreeNodeDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="path">Optional. The full path of the file or directory.</param>
    /// <param name="name">Optional. The name of the file or directory.</param>
    /// <param name="itemType">Optional. The type of the item, indicating whether it is a file, directory, or drive.</param>
    /// <returns>A configured <see cref="FileSystemTreeNodeDto"/> instance.</returns>
    public FileSystemTreeNodeDto Create(
        string? path = null, 
        string? name = null, 
        FileSystemItemType? itemType = null)
    {
        string generatedPath = _faker.System.FilePath();
        return new FileSystemTreeNodeDto
        {
            Path = path ?? generatedPath,
            Name = name ?? Path.GetFileName(generatedPath),
            ItemType = itemType ?? _faker.PickRandom<FileSystemItemType>(),
            IsExpanded = _faker.Random.Bool(),
            ChildrenLoaded = _faker.Random.Bool(),
            Children = []
        };
    }

    /// <summary>
    /// Creates multiple <see cref="FileSystemTreeNodeDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FileSystemTreeNodeDto"/> instances.</returns>
    public List<FileSystemTreeNodeDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
