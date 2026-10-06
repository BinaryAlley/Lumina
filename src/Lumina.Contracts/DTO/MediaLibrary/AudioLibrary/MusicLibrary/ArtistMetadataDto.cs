#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for the metadata of a music artist.
/// </summary>
/// <param name="Name">The name of the artist.</param>
/// <param name="SortName">The sort name of the artist, a variant of the name used when sorting artists, if applicable.</param>
/// <param name="Disambiguation">The disambiguation comment of the artist, used to distinguish artists with the same name, if applicable.</param>
/// <param name="Type">The type of the artist, if applicable.</param>
/// <param name="Gender">The gender of the artist, applicable only to artists that are persons or characters, if applicable.</param>
/// <param name="Country">The ISO 3166-1 alpha-2 code of the country the artist is associated with, if applicable.</param>
/// <param name="Area">The area the artist is primarily identified with, if applicable.</param>
/// <param name="BeginArea">The area the artist began in, if applicable.</param>
/// <param name="EndArea">The area the artist ended in, if applicable.</param>
/// <param name="LifeSpanBegin">The date the artist started existing, if applicable.</param>
/// <param name="LifeSpanEnd">The date the artist stopped existing, if applicable.</param>
/// <param name="IsEnded">Whether the artist no longer exists.</param>
/// <param name="Website">The website of the artist, if applicable.</param>
/// <param name="MusicBrainzArtistId">The MusicBrainz identifier of the artist, if applicable.</param>
/// <param name="Ipis">The list of IPI (Interested Party Information) codes of the artist.</param>
/// <param name="Isnis">The list of ISNI (International Standard Name Identifier) codes of the artist.</param>
/// <param name="Aliases">The list of alternative names of the artist.</param>
/// <param name="Genres">The list of genres associated with the artist.</param>
/// <param name="Tags">The list of tags that further describe or categorize the artist.</param>
/// <param name="Ratings">The list of ratings for the artist.</param>
/// <param name="Contributors">The list of media contributors that make up the artist, each with the role they played.</param>
[DebuggerDisplay("Name: {Name}")]
public sealed record ArtistMetadataDto(
    string? Name,
    string? SortName,
    string? Disambiguation,
    MusicArtistType? Type,
    MusicArtistGender? Gender,
    string? Country,
    MusicAreaDto? Area,
    MusicAreaDto? BeginArea,
    MusicAreaDto? EndArea,
    DateOnly? LifeSpanBegin,
    DateOnly? LifeSpanEnd,
    bool IsEnded,
    string? Website,
    Guid? MusicBrainzArtistId,
    List<string>? Ipis,
    List<string>? Isnis,
    List<MusicArtistAliasDto>? Aliases,
    List<GenreDto>? Genres,
    List<TagDto>? Tags,
    List<AudioRatingDto>? Ratings,
    List<MediaContributorDto>? Contributors
) : MetadataDto;
