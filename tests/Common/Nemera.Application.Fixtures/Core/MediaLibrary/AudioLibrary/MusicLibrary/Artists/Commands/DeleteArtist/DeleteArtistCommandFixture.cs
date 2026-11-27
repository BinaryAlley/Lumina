#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.DeleteArtist;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.DeleteArtist;

/// <summary>
/// Fixture class for the <see cref="DeleteArtistCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteArtistCommandFixture
{
    /// <summary>
    /// Creates a random valid <see cref="DeleteArtistCommand"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the artist belongs to, taken from the route.</param>
    /// <param name="artistId">Optional. The Id of the artist to delete, taken from the route.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="DeleteArtistCommand"/>.</returns>
    public DeleteArtistCommand Create(
        string? libraryId = null,
        string? artistId = null,
        bool includeLibraryId = true,
        bool includeArtistId = true)
    {
        return new DeleteArtistCommand(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="DeleteArtistCommand"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeleteArtistCommand"/> instances.</returns>
    public List<DeleteArtistCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
