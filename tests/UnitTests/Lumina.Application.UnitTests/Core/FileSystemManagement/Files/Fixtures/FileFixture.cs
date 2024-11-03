#region ========================================================================= USING =====================================================================================
using Bogus;
using ErrorOr;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.Files.Fixtures;

/// <summary>
/// Fixture class for the <see cref="File"/> class.
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
    /// <returns>The created <see cref="File"/>.</returns>
    public File CreateFile(
       string? path = null,
       string? name = null,
       Optional<DateTime>? dateCreated = null,
       Optional<DateTime>? dateModified = null,
       long? size = null,
       FileSystemItemStatus? status = null)
    {
        path ??= _faker.System.FilePath();
        name ??= _faker.System.FileName();
        dateCreated ??= Optional<DateTime>.Some(_faker.Date.Past());
        dateModified ??= Optional<DateTime>.Some(_faker.Date.Recent());
        size ??= _faker.Random.Long();
        status ??= _faker.PickRandom<FileSystemItemStatus>();

        ErrorOr<File> fileResult = File.Create(path, name, dateCreated.Value, dateModified.Value, size.Value, status.Value);

        if (fileResult.IsError)
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
        return Enumerable.Range(0, count).Select(_ => CreateFile()).ToList();
    }

    /// <summary>
    /// Creates a random valid <see cref="File"/>, without creationg and modification dates.
    /// </summary>
    /// <returns></returns>
    public static File CreateFileWithoutDates()
    {
        return File.Create(
            "/path/to/file_without_dates.txt",
            "file_without_dates.txt",
            Optional<DateTime>.None(),
            Optional<DateTime>.None(),
            2048,
            FileSystemItemStatus.Accessible
        ).Value;
    }
}
