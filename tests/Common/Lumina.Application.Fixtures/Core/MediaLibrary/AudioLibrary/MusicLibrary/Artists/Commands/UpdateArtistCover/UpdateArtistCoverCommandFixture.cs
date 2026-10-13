#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtistCover;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtistCover;

/// <summary>
/// Fixture class for the <see cref="UpdateArtistCoverCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistCoverCommandFixture
{
    /// <summary>
    /// Creates a random valid <see cref="UpdateArtistCoverCommand"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the artist belongs to.</param>
    /// <param name="artistId">Optional. The Id of the artist whose cover is updated.</param>
    /// <param name="cover">Optional. The stream of the uploaded cover image file.</param>
    /// <param name="fileName">Optional. The name of the uploaded cover image file.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCover">Whether the cover stream should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeFileName">Whether the file name should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="UpdateArtistCoverCommand"/>.</returns>
    public UpdateArtistCoverCommand Create(
        string? libraryId = null,
        string? artistId = null,
        Stream? cover = null,
        string? fileName = null,
        bool includeLibraryId = true,
        bool includeArtistId = true,
        bool includeCover = true,
        bool includeFileName = true)
    {
        return new UpdateArtistCoverCommand(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null,
            includeCover ? (cover ?? CreateCoverStream()) : null,
            includeFileName ? (fileName ?? "cover.jpg") : null);
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateArtistCoverCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UpdateArtistCoverCommand"/> instances.</returns>
    public List<UpdateArtistCoverCommand> CreateMany(int count = 3)
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
