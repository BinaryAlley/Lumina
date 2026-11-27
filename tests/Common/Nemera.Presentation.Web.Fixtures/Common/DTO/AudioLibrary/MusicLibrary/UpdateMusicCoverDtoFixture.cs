#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="UpdateMusicCoverDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateMusicCoverDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="UpdateMusicCoverDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="coverPath">Optional. The relative path of the stored cover image.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="UpdateMusicCoverDto"/> instance.</returns>
    public UpdateMusicCoverDto Create(
        string? coverPath = null,
        bool includeCoverPath = true)
    {
        return new UpdateMusicCoverDto
        {
            CoverPath = includeCoverPath ? (coverPath ?? _faker.System.FilePath()) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="UpdateMusicCoverDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UpdateMusicCoverDto"/> instances.</returns>
    public List<UpdateMusicCoverDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
