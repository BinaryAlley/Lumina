#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Fixture class for the <see cref="UpdateAlbumCoverResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateAlbumCoverResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UpdateAlbumCoverResponse"/>.
    /// </summary>
    /// <param name="coverPath">Optional. The relative path of the stored cover image.</param>
    /// <returns>The created <see cref="UpdateAlbumCoverResponse"/>.</returns>
    public UpdateAlbumCoverResponse Create(string? coverPath = null)
    {
        return new UpdateAlbumCoverResponse(
            coverPath ?? _faker.System.FilePath()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateAlbumCoverResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateAlbumCoverResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
