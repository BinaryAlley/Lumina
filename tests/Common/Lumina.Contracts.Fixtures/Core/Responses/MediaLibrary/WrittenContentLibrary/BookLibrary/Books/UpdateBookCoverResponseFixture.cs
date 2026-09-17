#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Fixture class for the <see cref="UpdateBookCoverResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCoverResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UpdateBookCoverResponse"/>.
    /// </summary>
    /// <param name="coverPath">Optional. The relative path of the stored cover image.</param>
    /// <returns>The created <see cref="UpdateBookCoverResponse"/>.</returns>
    public UpdateBookCoverResponse Create(string? coverPath = null)
    {
        return new UpdateBookCoverResponse(
            coverPath ?? _faker.System.FilePath()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateBookCoverResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateBookCoverResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
