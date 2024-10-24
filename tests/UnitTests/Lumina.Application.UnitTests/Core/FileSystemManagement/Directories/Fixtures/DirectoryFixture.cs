#region ========================================================================= USING =====================================================================================
using Bogus;
using ErrorOr;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Directories.Fixtures;

/// <summary>
/// Fixture class for the <see cref="Directory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DirectoryFixture
{
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryFixture"/> class.
    /// </summary>
    public DirectoryFixture()
    {
        _faker = new Faker();
    }

    /// <summary>
    /// Creates a random valid <see cref="Directory"/>.
    /// </summary>
    /// <param name="path">Optional. The directory path. If not provided, a random path is generated.</param>
    /// <param name="name">Optional. The directory name. If not provided, a random name is generated.</param>
    /// <param name="dateCreated">Optional. The directory's creation date. If not provided, a random past date is generated.</param>
    /// <param name="dateModified">Optional. The directory's modification date. If not provided, a recent date is generated.</param>
    /// <param name="status">Optional. The directory's status. If not provided, a random status is assigned.</param>
    /// <param name="childItems">Optional. A list of child items to be added to the directory.</param>
    /// <returns>A newly created Directory instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Directory creation fails.</exception>
    public Directory CreateDirectory(
        string? path = null,
        string? name = null,
        Optional<DateTime>? dateCreated = null,
        Optional<DateTime>? dateModified = null,
        FileSystemItemStatus? status = null,
        List<FileSystemItem>? childItems = null)
    {
        path ??= _faker.System.FilePath();
        name ??= _faker.System.FileName();
        dateCreated ??= Optional<DateTime>.Some(_faker.Date.Past());
        dateModified ??= Optional<DateTime>.Some(_faker.Date.Recent());
        status ??= _faker.PickRandom<FileSystemItemStatus>();

        ErrorOr<Directory> directoryResult = Directory.Create(path, name, dateCreated.Value, dateModified.Value, status.Value);

        if (directoryResult.IsError)
            throw new InvalidOperationException("Failed to create Directory: " + string.Join(", ", directoryResult.Errors));
        Directory directory = directoryResult.Value;

        if (childItems != null)
            foreach (FileSystemItem item in childItems)
                directory.AddItem(item);
        return directory;
    }

    /// <summary>
    /// Creates a list of <see cref="Directory"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<Directory> CreateMany(int count = 3)
    {
        return Enumerable.Range(0, count).Select(_ => CreateDirectory()).ToList();
    }

    /// <summary>
    /// Creates a list of nested <see cref="Directory"/> objects with a hierarchical structure.
    /// </summary>
    /// <param name="count">The number of top-level directories to create. Default is 3.</param>
    /// <param name="maxDepth">The maximum depth of the directory hierarchy. Default is 3.</param>
    /// <param name="maxChildrenPerDirectory">The maximum number of child items (files or directories) per directory. Default is 3.</param>
    /// <returns>A list of <see cref="Directory"/> objects with nested structures.</returns>
    public List<Directory> CreateManyNested(int count = 3, int maxDepth = 3, int maxChildrenPerDirectory = 3)
    {
        return Enumerable.Range(0, count).Select(_ => CreateNestedDirectory(1, maxDepth, maxChildrenPerDirectory)).ToList();
    }

    /// <summary>
    /// Recursively creates a nested <see cref="Directory"/> structure.
    /// </summary>
    /// <param name="currentDepth">The current depth in the directory hierarchy.</param>
    /// <param name="maxDepth">The maximum allowed depth for the directory hierarchy.</param>
    /// <param name="maxChildrenPerDirectory">The maximum number of child items (files or directories) allowed per directory.</param>
    /// <returns>A <see cref="Directory"/> object that may contain nested directories and files.</returns>
    private Directory CreateNestedDirectory(int currentDepth, int maxDepth, int maxChildrenPerDirectory)
    {
        List<FileSystemItem> childItems = [];

        if (currentDepth < maxDepth)
        {
            int childCount = _faker.Random.Int(0, maxChildrenPerDirectory);
            for (int i = 0; i < childCount; i++)
               childItems.Add(CreateNestedDirectory(currentDepth + 1, maxDepth, maxChildrenPerDirectory));
        }
        return CreateDirectory(childItems: childItems);
    }
}
