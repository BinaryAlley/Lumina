#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.RestoreArtist;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.RestoreArtist;

/// <summary>
/// Fixture class for the <see cref="RestoreArtistCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreArtistCommandFixture
{
    /// <summary>
    /// Creates a random valid <see cref="RestoreArtistCommand"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the artist belongs to, taken from the route.</param>
    /// <param name="artistId">Optional. The Id of the deleted artist to restore, taken from the route.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="RestoreArtistCommand"/>.</returns>
    public RestoreArtistCommand Create(
        string? libraryId = null,
        string? artistId = null,
        bool includeLibraryId = true,
        bool includeArtistId = true)
    {
        return new RestoreArtistCommand(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="RestoreArtistCommand"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RestoreArtistCommand"/> instances.</returns>
    public List<RestoreArtistCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
