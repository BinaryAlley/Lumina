#region ========================================================================= USING =====================================================================================
using Bogus;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Nemera.Domain.SharedKernel.Common.Enums.FileSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Domain.Fixtures.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;

/// <summary>
/// Fixture class for the <see cref="File"/> entity.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="File"/>.
    /// </summary>
    /// <param name="path">Optional. The file path. If not provided, a random path is generated.</param>
    /// <param name="name">Optional. The file name. If not provided, a random name is generated.</param>
    /// <param name="dateCreated">Optional. The file's creation date. If not provided, a random past date is generated.</param>
    /// <param name="dateModified">Optional. The file's modification date. If not provided, a recent date is generated.</param>
    /// <param name="size">Optional. The file size. If not provided, a random size is generated.</param>
    /// <param name="status">Optional. The file's status. If not provided, a random status is assigned.</param>
    /// <param name="isHidden">Optional. Whether the file is hidden. If not provided, a random value is generated.</param>
    /// <param name="includeDates">Whether the creation and modification dates should be included, or left unset.</param>
    /// <returns>The created <see cref="File"/>.</returns>
    public File Create(
        string? path = null,
        string? name = null,
        Optional<DateTime>? dateCreated = null,
        Optional<DateTime>? dateModified = null,
        long? size = null,
        FileSystemItemStatus? status = null,
        bool? isHidden = null,
        bool includeDates = true)
    {
        path ??= _faker.System.FilePath();
        name ??= _faker.System.FileName();
        dateCreated ??= includeDates ? Optional<DateTime>.Some(_faker.Date.Past()) : Optional<DateTime>.None();
        dateModified ??= includeDates ? Optional<DateTime>.Some(_faker.Date.Recent()) : Optional<DateTime>.None();
        size ??= _faker.Random.Long();
        status ??= _faker.PickRandom<FileSystemItemStatus>();
        isHidden ??= _faker.Random.Bool();

        Result<File> fileResult = File.Create(path, name, dateCreated.Value, dateModified.Value, size.Value, status.Value, isHidden.Value);

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
