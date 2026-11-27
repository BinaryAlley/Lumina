#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.AddArtist;

/// <summary>
/// Command for adding an artist, together with the albums of the artist and the tracks of those albums.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library this artist belongs to, taken from the route.</param>
/// <param name="Metadata">The metadata of the artist.</param>
/// <param name="Website">The website of the artist, if applicable.</param>
/// <param name="MusicBrainzArtistId">The MusicBrainz identifier of the artist, if applicable.</param>
/// <param name="Ipis">The list of IPI codes of the artist.</param>
/// <param name="Isnis">The list of ISNI codes of the artist.</param>
/// <param name="Contributors">The list of media contributors that make up the artist.</param>
/// <param name="Ratings">The list of ratings for the artist.</param>
/// <param name="Albums">The list of albums of the artist, each with its own list of tracks.</param>
[DebuggerDisplay("Name: {Metadata.Name}")]
public record AddArtistCommand(
    string? LibraryId,
    MusicArtistMetadataDto? Metadata,
    string? Website,
    Guid? MusicBrainzArtistId,
    List<string>? Ipis,
    List<string>? Isnis,
    List<MediaContributorReferenceDto>? Contributors,
    List<AudioRatingDto>? Ratings,
    List<AddAlbumCommand>? Albums
) : ICommand;
