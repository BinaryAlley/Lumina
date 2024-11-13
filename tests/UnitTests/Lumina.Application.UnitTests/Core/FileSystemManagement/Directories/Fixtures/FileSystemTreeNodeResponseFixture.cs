#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Fixtures;

/// <summary>
/// Fixture class for the <see cref="FileSystemTreeNodeResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemTreeNodeResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a single <see cref="FileSystemTreeNodeResponse"/> with a nested structure.
    /// </summary>
    /// <param name="currentDepth">The current depth in the tree structure. Default is 0 (root level).</param>
    /// <param name="maxDepth">The maximum depth of the tree structure. Default is 2.</param>
    /// <param name="maxChildren">The maximum number of children for each node. Default is 3.</param>
    /// <returns>A <see cref="FileSystemTreeNodeResponse"/> with a nested structure.</returns>
    public FileSystemTreeNodeResponse Create(int currentDepth = 0, int maxDepth = 2, int maxChildren = 3)
    {
        FileSystemTreeNodeResponse response = new()
        {
            Path = _faker.System.FilePath(),
            Name = _faker.System.FileName(),
            ItemType = FileSystemItemType.Directory,
            IsExpanded = _faker.Random.Bool(),
            ChildrenLoaded = true,
            Children = []
        };
        if (currentDepth < maxDepth)
        {
            int childCount = _faker.Random.Int(1, maxChildren);
            for (int i = 0; i < childCount; i++)
                response.Children.Add(Create(currentDepth + 1, maxDepth, maxChildren));
        }
        return response;
    }

    /// <summary>
    /// Creates a list of <see cref="FileSystemTreeNodeResponse"/> objects, each with a nested structure.
    /// </summary>
    /// <param name="count">The number of root-level nodes to create. Default is 2.</param>
    /// <param name="maxDepth">The maximum depth of the tree structure for each root node. Default is 2.</param>
    /// <param name="maxChildren">The maximum number of children for each node. Default is 3.</param>
    /// <returns>A list of <see cref="FileSystemTreeNodeResponse"/> objects with nested structures.</returns>
    public List<FileSystemTreeNodeResponse> CreateMany(int count = 2, int maxDepth = 2, int maxChildren = 3)
    {
        return Enumerable.Range(0, count).Select(_ => Create(0, maxDepth, maxChildren)).ToList();
    }
}
