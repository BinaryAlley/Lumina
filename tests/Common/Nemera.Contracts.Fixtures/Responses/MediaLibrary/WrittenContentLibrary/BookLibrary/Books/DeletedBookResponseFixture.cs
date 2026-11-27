#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Fixture class for the <see cref="DeletedBookResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletedBookResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="DeletedBookResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the deleted book.</param>
    /// <param name="libraryId">Optional. The Id of the media library the deleted book belongs to.</param>
    /// <param name="title">Optional. The title of the deleted book.</param>
    /// <param name="originalTitle">Optional. The original title of the deleted book.</param>
    /// <param name="path">Optional. The file system path of the deleted book.</param>
    /// <param name="deletedOnUtc">Optional. The date and time when the book was deleted.</param>
    /// <param name="coverPath">Optional. The path of the cover image of the deleted book.</param>
    /// <param name="includeOriginalTitle">Whether the original title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="DeletedBookResponse"/>.</returns>
    public DeletedBookResponse Create(
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
        return new DeletedBookResponse(
            id ?? _faker.Random.Guid(),
            libraryId ?? _faker.Random.Guid(),
            title ?? _faker.Lorem.Sentence(3),
            includeOriginalTitle ? (originalTitle ?? _faker.Lorem.Sentence(3)) : null,
            path ?? _faker.System.FilePath(),
            deletedOnUtc ?? _faker.Date.Recent(),
            includeCoverPath ? (coverPath ?? $"/covers/{_faker.Random.Guid()}.jpg") : null
        );
    }

    /// <summary>
    /// Creates a list of <see cref="DeletedBookResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeletedBookResponse"/> instances.</returns>
    public List<DeletedBookResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
