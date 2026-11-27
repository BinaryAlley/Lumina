#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="FileSystemPathId"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemPathIdFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="FileSystemPathId"/>.
    /// </summary>
    /// <param name="path">Optional. The path of the file system path element.</param>
    /// <returns>The created <see cref="FileSystemPathId"/>.</returns>
    public FileSystemPathId Create(string? path = null)
    {
        path ??= _faker.System.FilePath();

        Result<FileSystemPathId> fileSystemPathIdResult = FileSystemPathId.Create(path);

        if (fileSystemPathIdResult.IsFailure)
            throw new InvalidOperationException("Failed to create FileSystemPathId: " + string.Join(", ", fileSystemPathIdResult.Errors));
        return fileSystemPathIdResult.Value;
    }

    /// <summary>
    /// Creates a list of <see cref="FileSystemPathId"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<FileSystemPathId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
