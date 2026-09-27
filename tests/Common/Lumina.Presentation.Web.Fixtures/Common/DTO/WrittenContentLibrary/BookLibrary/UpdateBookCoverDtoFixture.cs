#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Fixture class for generating <see cref="UpdateBookCoverDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCoverDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="UpdateBookCoverDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="coverPath">Optional. The relative path of the stored cover image.</param>
    /// <returns>A configured <see cref="UpdateBookCoverDto"/> instance.</returns>
    public UpdateBookCoverDto Create(string? coverPath = null)
    {
        return new UpdateBookCoverDto
        {
            CoverPath = coverPath ?? _faker.System.FilePath()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="UpdateBookCoverDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UpdateBookCoverDto"/> instances.</returns>
    public List<UpdateBookCoverDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
