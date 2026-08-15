#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;

/// <summary>
/// Fixture class for the <see cref="File"/> entity.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileFixture
{
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileFixture"/> class.
    /// </summary>
    public FileFixture()
    {
        _faker = new Faker();
    }

    /// <summary>
    /// Creates a random valid <see cref="File"/>.
    /// </summary>
    /// <param name="path">Optional. The file path. If not provided, a random path is generated.</param>
    /// <param name="name">Optional. The file name. If not provided, a random name is generated.</param>
    /// <param name="dateCreated">Optional. The file's creation date. If not provided, a random past date is generated.</param>
    /// <param name="dateModified">Optional. The file's modification date. If not provided, a recent date is generated.</param>
    /// <param name="size">Optional. The file size. If not provided, a random size is generated.</param>
    /// <param name="status">Optional. The file's status. If not provided, a random status is assigned.</param>
    /// <param name="includeDates">Whether the file should include creation and modification dates.</param>
    /// <returns>The created <see cref="File"/>.</returns>
    public File Create(
        string? path = null,
        string? name = null,
        Optional<DateTime>? dateCreated = null,
        Optional<DateTime>? dateModified = null,
        long? size = null,
        FileSystemItemStatus? status = null,
        bool includeDates = true)
    {
        path ??= _faker.System.FilePath();
        name ??= _faker.System.FileName();
        dateCreated ??= includeDates ? Optional<DateTime>.Some(_faker.Date.Past()) : Optional<DateTime>.None();
        dateModified ??= includeDates ? Optional<DateTime>.Some(_faker.Date.Recent()) : Optional<DateTime>.None();
        size ??= _faker.Random.Long();
        status ??= _faker.PickRandom<FileSystemItemStatus>();

        Result<File> fileResult = File.Create(path, name, dateCreated.Value, dateModified.Value, size.Value, status.Value);

        if (fileResult.IsFailure)
            throw new InvalidOperationException("Failed to create File: " + string.Join(", ", fileResult.Errors));
        return fileResult.Value;
    }

    /// <summary>
    /// Creates a list of <see cref="File"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<File> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
