#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Reading;

/// <summary>
/// Fixture class for generating <see cref="ReadingAvailabilityDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingAvailabilityDtoFixture
{
    /// <summary>
    /// Creates a new <see cref="ReadingAvailabilityDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="bookId">Optional identifier of the book whose reading availability is reported.</param>
    /// <param name="libraryId">Optional identifier of the media library the book belongs to.</param>
    /// <param name="isAvailable">Optional value indicating whether the book can be opened for reading.</param>
    /// <param name="errorCode">Optional code of the error preventing the book from being read, when it cannot be read.</param>
    /// <returns>A configured <see cref="ReadingAvailabilityDto"/> instance.</returns>
    public ReadingAvailabilityDto Create(
        Guid? bookId = null,
        Guid? libraryId = null,
        bool? isAvailable = null,
        string? errorCode = null)
    {
        return new ReadingAvailabilityDto
        {
            BookId = bookId ?? Guid.NewGuid(),
            LibraryId = libraryId ?? Guid.NewGuid(),
            IsAvailable = isAvailable ?? true,
            ErrorCode = errorCode
        };
    }

    /// <summary>
    /// Creates multiple <see cref="ReadingAvailabilityDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ReadingAvailabilityDto"/> instances.</returns>
    public List<ReadingAvailabilityDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
