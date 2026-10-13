#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Fixture class for the <see cref="UpdateArtistCoverResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistCoverResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UpdateArtistCoverResponse"/>.
    /// </summary>
    /// <param name="coverPath">Optional. The relative path of the stored cover image.</param>
    /// <returns>The created <see cref="UpdateArtistCoverResponse"/>.</returns>
    public UpdateArtistCoverResponse Create(string? coverPath = null)
    {
        return new UpdateArtistCoverResponse(
            coverPath ?? _faker.System.FilePath()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateArtistCoverResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateArtistCoverResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
