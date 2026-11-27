#region ========================================================================= USING =====================================================================================
using Bogus;
using Nemera.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Fixture class for the <see cref="DeletedBookDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletedBookDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="DeletedBookDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The Id of the deleted book.</param>
    /// <param name="libraryId">Optional. The Id of the media library the deleted book belongs to.</param>
    /// <param name="title">Optional. The title of the deleted book.</param>
    /// <param name="originalTitle">Optional. The original title of the deleted book.</param>
    /// <param name="path">Optional. The file system path of the deleted book.</param>
    /// <param name="deletedOnUtc">Optional. The date and time when the book was deleted.</param>
    /// <param name="coverPath">Optional. The relative path of the stored cover image of the deleted book.</param>
    /// <param name="includeOriginalTitle">Whether the original title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="DeletedBookDto"/>.</returns>
    public DeletedBookDto Create(
        Guid? id = null,
        Guid? libraryId = null,
        string? title = null,
        string? originalTitle = null,
        string? path = null,
        DateTime? deletedOnUtc = null,
        string? coverPath = null,
        bool includeOriginalTitle = true,
        bool includeCoverPath = true)
    {
        return new DeletedBookDto(
            Id: id ?? _faker.Random.Guid(),
            LibraryId: libraryId ?? _faker.Random.Guid(),
            Title: title ?? _faker.Lorem.Sentence(),
            OriginalTitle: includeOriginalTitle ? (originalTitle ?? _faker.Lorem.Sentence()) : null,
            Path: path ?? _faker.System.FilePath(),
            DeletedOnUtc: deletedOnUtc ?? _faker.Date.Recent(),
            CoverPath: includeCoverPath ? (coverPath ?? _faker.System.FilePath()) : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="DeletedBookDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeletedBookDto"/> instances.</returns>
    public List<DeletedBookDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
