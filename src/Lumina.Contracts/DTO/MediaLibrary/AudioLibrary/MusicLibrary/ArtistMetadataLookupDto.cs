#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for a music artist metadata lookup.
/// </summary>
/// <param name="LibraryId">The Id of the media library the artist belongs to.</param>
/// <param name="Path">The file system path of a track of the artist.</param>
/// <param name="MusicBrainzArtistId">The MusicBrainz identifier of the artist, if applicable.</param>
/// <param name="Name">The name of the artist, if applicable.</param>
[DebuggerDisplay("Name: {Name}")]
public sealed record ArtistMetadataLookupDto(
    Guid LibraryId,
    string Path,
    Guid? MusicBrainzArtistId = null,
    string? Name = null
) : MetadataLookupDto;
