#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;

/// <summary>
/// Fixture class for the <see cref="UpdateBookCoverCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCoverCommandFixture
{
    /// <summary>
    /// Creates a random valid <see cref="UpdateBookCoverCommand"/>.
    /// </summary>
    /// <param name="bookId">Optional. The Id of the book whose cover is updated.</param>
    /// <param name="cover">Optional. The stream of the uploaded cover image file.</param>
    /// <param name="fileName">Optional. The name of the uploaded cover image file.</param>
    /// <param name="includeCover">Whether the cover stream should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="UpdateBookCoverCommand"/>.</returns>
    public UpdateBookCoverCommand Create(
        Guid? bookId = null,
        Stream? cover = null,
        string? fileName = null,
        bool includeCover = true)
    {
        return new UpdateBookCoverCommand(
            bookId ?? Guid.NewGuid(),
            includeCover ? (cover ?? CreateCoverStream()) : null,
            fileName ?? "cover.jpg"
        );
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateBookCoverCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UpdateBookCoverCommand"/> instances.</returns>
    public List<UpdateBookCoverCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Creates an in-memory stream carrying a minimal file payload.
    /// </summary>
    /// <returns>A non-empty in-memory stream.</returns>
    private static Stream CreateCoverStream()
    {
        byte[] payload = Encoding.UTF8.GetBytes("fake cover payload");
        return new MemoryStream(payload);
    }
}
