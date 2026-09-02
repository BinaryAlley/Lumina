#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Fixture class for the <see cref="ReadingAvailabilityResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingAvailabilityResponseFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ReadingAvailabilityResponse"/>.
    /// </summary>
    /// <param name="bookId">Optional. The Id of the book whose reading availability is reported.</param>
    /// <param name="libraryId">Optional. The Id of the media library the book belongs to.</param>
    /// <param name="isAvailable">Optional. Whether the book can be opened for reading.</param>
    /// <param name="errorCode">Optional. The code of the error preventing the book from being read, when it cannot be read.</param>
    /// <returns>The created <see cref="ReadingAvailabilityResponse"/>.</returns>
    public ReadingAvailabilityResponse Create(
        Guid? bookId = null,
        Guid? libraryId = null,
        bool? isAvailable = null,
        string? errorCode = null)
    {
        return new ReadingAvailabilityResponse(
            bookId ?? Guid.NewGuid(),
            libraryId ?? Guid.NewGuid(),
            isAvailable ?? true,
            errorCode
        );
    }

    /// <summary>
    /// Creates a list of <see cref="ReadingAvailabilityResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ReadingAvailabilityResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
