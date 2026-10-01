#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Plugins.MusicBrainz.Common.Models.Contracts.Responses;
using Lumina.Plugins.MusicBrainz.Core.Mapping;
using Lumina.Plugins.MusicBrainz.Fixtures.Common.Models.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Core.Mapping;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzMapper"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzMapperTests
{
    private readonly MusicBrainzArtistResponseFixture _musicBrainzArtistResponseFixture = new();
    private readonly MusicBrainzAreaResponseFixture _musicBrainzAreaResponseFixture = new();
    private readonly MusicBrainzAliasResponseFixture _musicBrainzAliasResponseFixture = new();
    private readonly MusicBrainzRelationResponseFixture _musicBrainzRelationResponseFixture = new();
    private readonly MusicBrainzTagResponseFixture _musicBrainzTagResponseFixture = new();
    private readonly MusicBrainzLifeSpanResponseFixture _musicBrainzLifeSpanResponseFixture = new();
    private readonly MusicBrainzRatingResponseFixture _musicBrainzRatingResponseFixture = new();
    private readonly MusicBrainzReleaseGroupResponseFixture _musicBrainzReleaseGroupResponseFixture = new();
    private readonly MusicBrainzReleaseResponseFixture _musicBrainzReleaseResponseFixture = new();
    private readonly MusicBrainzArtistCreditResponseFixture _musicBrainzArtistCreditResponseFixture = new();
    private readonly MusicBrainzLabelInfoResponseFixture _musicBrainzLabelInfoResponseFixture = new();
    private readonly MusicBrainzMediumResponseFixture _musicBrainzMediumResponseFixture = new();
    private readonly MusicBrainzRecordingResponseFixture _musicBrainzRecordingResponseFixture = new();
    private readonly MusicBrainzWorkResponseFixture _musicBrainzWorkResponseFixture = new();
    private readonly MusicBrainzUrlResponseFixture _musicBrainzUrlResponseFixture = new();
    private readonly MusicBrainzTextRepresentationResponseFixture _musicBrainzTextRepresentationResponseFixture = new();
    private readonly MusicBrainzLabelResponseFixture _musicBrainzLabelResponseFixture = new();

    [Fact]
    public void MapArtist_WhenEveryFieldIsPresent_ShouldMapEveryField()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid areaId = Guid.NewGuid();
        Guid beginAreaId = Guid.NewGuid();
        Guid endAreaId = Guid.NewGuid();
        MusicBrainzAreaResponse area = _musicBrainzAreaResponseFixture.Create(id: areaId.ToString(), name: "United States", sortName: "United States", disambiguation: "the country", type: "Country", iso3166Part1Codes: ["US"], iso3166Part2Codes: ["US-NY"]);
        MusicBrainzAreaResponse beginArea = _musicBrainzAreaResponseFixture.Create(id: beginAreaId.ToString(), name: "New York", iso3166Part1Codes: ["US"], iso3166Part2Codes: []);
        MusicBrainzAreaResponse endArea = _musicBrainzAreaResponseFixture.Create(id: endAreaId.ToString(), name: "Los Angeles", iso3166Part1Codes: ["US"], iso3166Part2Codes: []);
        MusicBrainzAliasResponse alias = _musicBrainzAliasResponseFixture.Create(name: "The Alias", sortName: "Alias, The", type: "Artist name", locale: "en", isPrimary: true, begin: "1990-01-01", end: "2000-01-01", isEnded: false);
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            id: artistId.ToString(),
            name: "  The Artist  ",
            sortName: "Artist, The",
            disambiguation: "UK band",
            type: "Group",
            gender: "Female",
            country: "US",
            area: area,
            includeArea: true,
            beginArea: beginArea,
            includeBeginArea: true,
            endArea: endArea,
            includeEndArea: true,
            lifeSpan: _musicBrainzLifeSpanResponseFixture.Create(begin: "1950-01-01", end: "2000-12-31", isEnded: true),
            includeLifeSpan: true,
            isnis: ["ISNI1", "", "  "],
            ipis: ["IPI1", "", "  "],
            aliases: [alias],
            tags: [_musicBrainzTagResponseFixture.Create(name: "Rock"), _musicBrainzTagResponseFixture.Create(name: "rock"), _musicBrainzTagResponseFixture.Create(name: "   ")],
            genres: [_musicBrainzTagResponseFixture.Create(name: "Pop"), _musicBrainzTagResponseFixture.Create(name: "POP")],
            relations:
            [
                _musicBrainzRelationResponseFixture.Create(type: "official homepage", targetType: "url", includeUrl: true, url: _musicBrainzUrlResponseFixture.Create(resource: "https://artist.example")),
                _musicBrainzRelationResponseFixture.Create(type: "official homepage", targetType: "url", includeUrl: true, url: _musicBrainzUrlResponseFixture.Create(resource: "https://second.example")),
                _musicBrainzRelationResponseFixture.Create(type: "wikidata", targetType: "url", includeUrl: true, url: _musicBrainzUrlResponseFixture.Create(resource: "https://wikidata.example")),
                _musicBrainzRelationResponseFixture.Create(type: "member of band", targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "Member")),
                _musicBrainzRelationResponseFixture.Create(type: "producer", targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "The Producer")),
                _musicBrainzRelationResponseFixture.Create(type: "producer", targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "The Producer")),
                _musicBrainzRelationResponseFixture.Create(type: "producer", targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "   ")),
                _musicBrainzRelationResponseFixture.Create(type: "composer", targetType: "work")
            ],
            rating: _musicBrainzRatingResponseFixture.Create(value: 4.5m, votesCount: 12),
            includeRating: true);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        Assert.Equal("The Artist", result.Name);
        Assert.Equal("Artist, The", result.SortName);
        Assert.Equal("UK band", result.Disambiguation);
        Assert.Equal(MusicArtistType.Group, result.Type);
        Assert.Equal(MusicArtistGender.Female, result.Gender);
        Assert.Equal("US", result.Country);
        Assert.NotNull(result.Area);
        Assert.Equal(areaId, result.Area.MusicBrainzAreaId);
        Assert.Equal("United States", result.Area.Name);
        Assert.Equal("United States", result.Area.SortName);
        Assert.Equal("the country", result.Area.Disambiguation);
        Assert.Equal("Country", result.Area.Type);
        Assert.Equal("US", result.Area.Iso3166Code);
        Assert.NotNull(result.BeginArea);
        Assert.Equal(beginAreaId, result.BeginArea.MusicBrainzAreaId);
        Assert.NotNull(result.EndArea);
        Assert.Equal(endAreaId, result.EndArea.MusicBrainzAreaId);
        Assert.Equal(new DateOnly(1950, 1, 1), result.LifeSpanBegin);
        Assert.Equal(new DateOnly(2000, 12, 31), result.LifeSpanEnd);
        Assert.True(result.IsEnded);
        Assert.Equal("https://artist.example", result.Website);
        Assert.Equal(artistId, result.MusicBrainzArtistId);
        Assert.Equal(["IPI1"], result.Ipis);
        Assert.Equal(["ISNI1"], result.Isnis);
        MusicArtistAliasDto mappedAlias = Assert.Single(result.Aliases!);
        Assert.Equal("The Alias", mappedAlias.Name);
        Assert.Equal("Alias, The", mappedAlias.SortName);
        Assert.Equal("Artist name", mappedAlias.Type);
        Assert.Equal("en", mappedAlias.Locale);
        Assert.True(mappedAlias.IsPrimary);
        Assert.Equal(new DateOnly(1990, 1, 1), mappedAlias.BeginDate);
        Assert.Equal(new DateOnly(2000, 1, 1), mappedAlias.EndDate);
        Assert.False(mappedAlias.IsEnded);
        Assert.Equal(["Rock"], result.Tags!.Select(tag => tag.Name));
        Assert.Equal(["Pop"], result.Genres!.Select(genre => genre.Name));
        Assert.Single(result.Ratings!);
        Assert.Equal(4.5m, result.Ratings![0].Value);
        Assert.Equal(5m, result.Ratings[0].MaxValue);
        Assert.Equal(AudioRatingSource.MusicBrainz, result.Ratings[0].Source);
        Assert.Equal(12, result.Ratings[0].VoteCount);
        Assert.Equal(2, result.Contributors!.Count);
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "Member" && contributor.Role == MediaContributorRole.Performer);
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "The Producer" && contributor.Role == MediaContributorRole.Producer);
    }

    [Fact]
    public void MapArtist_WhenOptionalFieldsAreMissing_ShouldReturnNullAndEmptyCollections()
    {
        // Arrange
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            id: "not-a-guid",
            name: "   ",
            includeSortName: false,
            includeDisambiguation: false,
            includeType: false,
            includeGender: false,
            includeCountry: false,
            includeArea: false,
            includeBeginArea: false,
            includeEndArea: false,
            includeLifeSpan: false,
            isnis: [],
            ipis: [],
            aliases: [],
            tags: [],
            genres: [],
            relations: [],
            includeRating: false);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        Assert.Null(result.Name);
        Assert.Null(result.SortName);
        Assert.Null(result.Disambiguation);
        Assert.Null(result.Type);
        Assert.Null(result.Gender);
        Assert.Null(result.Country);
        Assert.Null(result.Area);
        Assert.Null(result.BeginArea);
        Assert.Null(result.EndArea);
        Assert.Null(result.LifeSpanBegin);
        Assert.Null(result.LifeSpanEnd);
        Assert.False(result.IsEnded);
        Assert.Null(result.Website);
        Assert.Null(result.MusicBrainzArtistId);
        Assert.Empty(result.Ipis!);
        Assert.Empty(result.Isnis!);
        Assert.Empty(result.Aliases!);
        Assert.Empty(result.Tags!);
        Assert.Empty(result.Genres!);
        Assert.Empty(result.Ratings!);
        Assert.Empty(result.Contributors!);
    }

    [Theory]
    [InlineData("person", MusicArtistType.Person)] // an individual
    [InlineData("group", MusicArtistType.Group)] // an ensemble
    [InlineData("orchestra", MusicArtistType.Orchestra)] // an instrumental ensemble
    [InlineData("choir", MusicArtistType.Choir)] // a vocal ensemble
    [InlineData("character", MusicArtistType.Character)] // a fictional character
    [InlineData("other", MusicArtistType.Other)] // a known but uncategorized type
    [InlineData("PERSON", MusicArtistType.Person)] // the value is normalized
    public void MapArtist_WhenTheTypeIsKnown_ShouldMapIt(string type, MusicArtistType expected)
    {
        // Arrange
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(type: type, relations: [], aliases: [], tags: [], genres: []);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        Assert.Equal(expected, result.Type);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("made-up")]
    public void MapArtist_WhenTheTypeIsUnknown_ShouldReturnNullType(string? type)
    {
        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(_musicBrainzArtistResponseFixture.Create(type: type, includeType: type is not null, relations: [], aliases: [], tags: [], genres: []));

        // Assert
        Assert.Null(result.Type);
    }

    [Theory]
    [InlineData("male", MusicArtistGender.Male)]
    [InlineData("female", MusicArtistGender.Female)]
    [InlineData("other", MusicArtistGender.Other)]
    [InlineData("not applicable", MusicArtistGender.NotApplicable)]
    [InlineData("FEMALE", MusicArtistGender.Female)] // the value is normalized
    public void MapArtist_WhenTheGenderIsKnown_ShouldMapIt(string gender, MusicArtistGender expected)
    {
        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(_musicBrainzArtistResponseFixture.Create(gender: gender, relations: [], aliases: [], tags: [], genres: []));

        // Assert
        Assert.Equal(expected, result.Gender);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("made-up")]
    public void MapArtist_WhenTheGenderIsUnknown_ShouldReturnNullGender(string? gender)
    {
        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(_musicBrainzArtistResponseFixture.Create(gender: gender, includeGender: gender is not null, relations: [], aliases: [], tags: [], genres: []));

        // Assert
        Assert.Null(result.Gender);
    }

    [Theory]
    [InlineData("executive producer", MediaContributorRole.ExecutiveProducer)] // more specific than producer
    [InlineData("producer", MediaContributorRole.Producer)]
    [InlineData("remixer", MediaContributorRole.Remixer)]
    [InlineData("mix", MediaContributorRole.Mixer)]
    [InlineData("engineer", MediaContributorRole.Engineer)]
    [InlineData("mastering", MediaContributorRole.Engineer)]
    [InlineData("composer", MediaContributorRole.Composer)]
    [InlineData("writer", MediaContributorRole.Author)]
    [InlineData("lyricist", MediaContributorRole.Lyricist)]
    [InlineData("librettist", MediaContributorRole.Lyricist)]
    [InlineData("arranger", MediaContributorRole.Arranger)]
    [InlineData("conductor", MediaContributorRole.Conductor)]
    [InlineData("orchestrator", MediaContributorRole.Orchestrator)]
    [InlineData("backing vocals", MediaContributorRole.BackingVocals)]
    [InlineData("vocal", MediaContributorRole.Vocals)]
    [InlineData("member of band", MediaContributorRole.Performer)]
    [InlineData("performer", MediaContributorRole.Performer)]
    [InlineData("orchestra", MediaContributorRole.Performer)]
    [InlineData("choir", MediaContributorRole.Performer)]
    [InlineData("made-up role", MediaContributorRole.Other)]
    public void MapArtist_WhenAnArtistRelationHasAKnownType_ShouldMapTheRole(string relationType, MediaContributorRole expected)
    {
        // Arrange
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            relations: [_musicBrainzRelationResponseFixture.Create(type: relationType, targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "The Person"))],
            aliases: [],
            tags: [],
            genres: []);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        MediaContributorDto contributor = Assert.Single(result.Contributors!);
        Assert.Equal(expected, contributor.Role);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MapArtist_WhenAnArtistRelationHasNoType_ShouldMapTheRoleAsOther(string? relationType)
    {
        // Arrange
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            relations: [_musicBrainzRelationResponseFixture.Create(type: relationType, includeType: relationType is not null, targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "The Person"))],
            aliases: [],
            tags: [],
            genres: []);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        Assert.Equal(MediaContributorRole.Other, Assert.Single(result.Contributors!).Role);
    }

    [Theory]
    [InlineData("backing vocal", MediaContributorRole.BackingVocals)]
    [InlineData("vocals", MediaContributorRole.Vocals)]
    [InlineData("bass guitar", MediaContributorRole.BassGuitar)]
    [InlineData("guitar", MediaContributorRole.Guitar)]
    [InlineData("drums", MediaContributorRole.Drums)]
    [InlineData("percussion", MediaContributorRole.Percussion)]
    [InlineData("keyboard", MediaContributorRole.Keyboards)]
    [InlineData("piano", MediaContributorRole.Piano)]
    [InlineData("synthesizer", MediaContributorRole.Synthesizer)]
    [InlineData("synth", MediaContributorRole.Synthesizer)]
    [InlineData("strings", MediaContributorRole.Strings)]
    [InlineData("violin", MediaContributorRole.Strings)]
    [InlineData("cello", MediaContributorRole.Strings)]
    [InlineData("viola", MediaContributorRole.Strings)]
    [InlineData("brass", MediaContributorRole.Brass)]
    [InlineData("trumpet", MediaContributorRole.Brass)]
    [InlineData("trombone", MediaContributorRole.Brass)]
    [InlineData("horn", MediaContributorRole.Brass)]
    [InlineData("woodwind", MediaContributorRole.Woodwinds)]
    [InlineData("flute", MediaContributorRole.Woodwinds)]
    [InlineData("saxophone", MediaContributorRole.Woodwinds)]
    [InlineData("clarinet", MediaContributorRole.Woodwinds)]
    public void MapArtist_WhenAnArtistRelationHasAnInstrumentAttribute_ShouldPreferTheInstrument(string attribute, MediaContributorRole expected)
    {
        // Arrange
        // The relation type would map to a different role, so the instrument attribute must take precedence.
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            relations: [_musicBrainzRelationResponseFixture.Create(type: "producer", targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "The Person"), attributes: [attribute])],
            aliases: [],
            tags: [],
            genres: []);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        Assert.Equal(expected, Assert.Single(result.Contributors!).Role);
    }

    [Fact]
    public void MapArtist_WhenAnArtistRelationHasAnUnknownInstrumentAttribute_ShouldFallBackToTheRelationType()
    {
        // Arrange
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            relations: [_musicBrainzRelationResponseFixture.Create(type: "producer", targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "The Person"), attributes: ["made-up instrument"])],
            aliases: [],
            tags: [],
            genres: []);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        Assert.Equal(MediaContributorRole.Producer, Assert.Single(result.Contributors!).Role);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void MapArtist_WhenAnArtistRelationHasABlankInstrumentAttribute_ShouldFallBackToTheRelationType(string attribute)
    {
        // Arrange
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            relations: [_musicBrainzRelationResponseFixture.Create(type: "composer", targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "The Person"), attributes: [attribute])],
            aliases: [],
            tags: [],
            genres: []);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        Assert.Equal(MediaContributorRole.Composer, Assert.Single(result.Contributors!).Role);
    }

    [Fact]
    public void MapArtist_WhenARelationTargetsAnotherType_ShouldIgnoreIt()
    {
        // Arrange
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            relations:
            [
                _musicBrainzRelationResponseFixture.Create(type: "composer", targetType: "work"),
                _musicBrainzRelationResponseFixture.Create(type: "producer", targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "The Producer"))
            ],
            aliases: [],
            tags: [],
            genres: []);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        MediaContributorDto contributor = Assert.Single(result.Contributors!);
        Assert.Equal("The Producer", contributor.Name!.DisplayName);
    }

    [Fact]
    public void MapArtist_WhenAnArtistRelationHasNoName_ShouldSkipIt()
    {
        // Arrange
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            relations: [_musicBrainzRelationResponseFixture.Create(type: "producer", targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "   "))],
            aliases: [],
            tags: [],
            genres: []);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        Assert.Empty(result.Contributors!);
    }

    [Fact]
    public void MapArtist_WhenAUrlRelationIsNotAnOfficialHomepage_ShouldNotSetTheWebsite()
    {
        // Arrange
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            relations: [_musicBrainzRelationResponseFixture.Create(type: "wikidata", targetType: "url", includeUrl: true, url: _musicBrainzUrlResponseFixture.Create(resource: "https://wikidata.example"))],
            aliases: [],
            tags: [],
            genres: []);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        Assert.Null(result.Website);
    }

    [Fact]
    public void MapArea_WhenTheAreaIsNull_ShouldReturnNull()
    {
        // Act
        MusicAreaDto? result = MusicBrainzMapper.MapArea(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MapArea_WhenThePrimaryIsoCodeIsMissing_ShouldUseTheSecondaryIsoCode()
    {
        // Arrange
        MusicBrainzAreaResponse area = _musicBrainzAreaResponseFixture.Create(id: Guid.NewGuid().ToString(), name: "Some Area", iso3166Part1Codes: [], iso3166Part2Codes: ["GB-ENG"]);

        // Act
        MusicAreaDto? result = MusicBrainzMapper.MapArea(area);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("GB-ENG", result.Iso3166Code);
    }

    [Fact]
    public void MapArea_WhenEveryOptionalFieldIsBlank_ShouldReturnNullFields()
    {
        // Arrange
        MusicBrainzAreaResponse area = _musicBrainzAreaResponseFixture.Create(id: "not-a-guid", name: "  ", includeSortName: false, includeDisambiguation: false, includeType: false, iso3166Part1Codes: [], iso3166Part2Codes: []);

        // Act
        MusicAreaDto? result = MusicBrainzMapper.MapArea(area);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.MusicBrainzAreaId);
        Assert.Null(result.Name);
        Assert.Null(result.SortName);
        Assert.Null(result.Disambiguation);
        Assert.Null(result.Type);
        Assert.Null(result.Iso3166Code);
    }

    [Fact]
    public void MapAlbum_WhenEveryFieldIsPresent_ShouldMapEveryField()
    {
        // Arrange
        Guid releaseGroupId = Guid.NewGuid();
        Guid releaseId = Guid.NewGuid();
        Guid releaseArtistId = Guid.NewGuid();
        MusicBrainzArtistCreditResponse credit = _musicBrainzArtistCreditResponseFixture.Create(name: "The Album Artist", artist: _musicBrainzArtistResponseFixture.Create(id: releaseArtistId.ToString(), name: "The Album Artist"), includeJoinPhrase: false);
        MusicBrainzMediumResponse medium = _musicBrainzMediumResponseFixture.Create(position: 1, format: "CD", tracks: [], trackCount: 12);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(
            id: releaseId.ToString(),
            title: "The Release",
            disambiguation: "remastered",
            status: "Official",
            date: "1995-11-21",
            country: "US",
            barcode: "1234567890123",
            packaging: "Jewel Case",
            textRepresentation: _musicBrainzTextRepresentationResponseFixture.Create(language: "eng", script: "Latn"),
            artistCredit: [credit],
            includeReleaseGroup: false,
            labelInfo:
            [
                _musicBrainzLabelInfoResponseFixture.Create(catalogNumber: "CAT-1", label: _musicBrainzLabelResponseFixture.Create(name: "The Label")),
                _musicBrainzLabelInfoResponseFixture.Create(catalogNumber: "CAT-2", label: _musicBrainzLabelResponseFixture.Create(name: "Other Label")),
                _musicBrainzLabelInfoResponseFixture.Create(catalogNumber: "cat-1", label: _musicBrainzLabelResponseFixture.Create(name: "Other Label")),
                _musicBrainzLabelInfoResponseFixture.Create(catalogNumber: "   ", label: _musicBrainzLabelResponseFixture.Create(name: "Other Label"))
            ],
            media: [medium, _musicBrainzMediumResponseFixture.Create(position: 2, format: "Vinyl", tracks: [], trackCount: 3)],
            asin: "B000000001",
            tags: [_musicBrainzTagResponseFixture.Create(name: "Rock")],
            genres: [_musicBrainzTagResponseFixture.Create(name: "Pop")],
            relations: [_musicBrainzRelationResponseFixture.Create(type: "producer", targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "The Release Producer"))]);
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(
            id: releaseGroupId.ToString(),
            title: "The Album",
            disambiguation: "the album",
            primaryType: "Album",
            secondaryTypes: ["Live"],
            firstReleaseDate: "1975-06-01",
            artistCredit: [credit],
            tags: [_musicBrainzTagResponseFixture.Create(name: "Rock"), _musicBrainzTagResponseFixture.Create(name: "rock")],
            genres: [_musicBrainzTagResponseFixture.Create(name: "Pop")],
            rating: _musicBrainzRatingResponseFixture.Create(value: 4m, votesCount: 8),
            includeRating: true);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.Equal("The Album", result.Title);
        Assert.Null(result.OriginalTitle);
        Assert.Null(result.Description);
        Assert.Equal("the album", result.Disambiguation);
        Assert.NotNull(result.ReleaseInfo);
        Assert.Equal(new DateOnly(1975, 6, 1), result.ReleaseInfo.OriginalReleaseDate);
        Assert.Equal(1975, result.ReleaseInfo.OriginalReleaseYear);
        Assert.Equal(new DateOnly(1995, 11, 21), result.ReleaseInfo.ReReleaseDate);
        Assert.Equal(1995, result.ReleaseInfo.ReReleaseYear);
        Assert.Equal(ReleaseCountry.US, result.ReleaseInfo.ReleaseCountry);
        Assert.Equal("remastered", result.ReleaseInfo.ReleaseVersion);
        Assert.NotNull(result.Language);
        Assert.Equal("en", result.Language.LanguageCode);
        Assert.Null(result.OriginalLanguage);
        Assert.Equal(["Rock"], result.Tags!.Select(tag => tag.Name));
        Assert.Equal(["Pop"], result.Genres!.Select(genre => genre.Name));
        Assert.Equal("Latn", result.Script);
        Assert.Equal([MusicReleaseType.Album, MusicReleaseType.Live], result.ReleaseTypes);
        Assert.Equal(MusicReleaseStatus.Official, result.ReleaseStatus);
        Assert.Equal(MusicMediaFormat.CD, result.MediaFormat);
        Assert.Equal(MusicReleasePackaging.JewelCase, result.Packaging);
        Assert.Equal(2, result.TotalDiscs);
        Assert.Equal(15, result.TotalTracks);
        Assert.Equal("1234567890123", result.Barcode);
        Assert.Equal(["CAT-1", "CAT-2"], result.CatalogNumbers);
        Assert.Equal("The Label", result.Label);
        Assert.Equal("B000000001", result.ASIN);
        Assert.Equal(releaseId, result.MusicBrainzReleaseId);
        Assert.Equal(releaseGroupId, result.MusicBrainzReleaseGroupId);
        Assert.Equal(releaseArtistId, result.MusicBrainzReleaseArtistId);
        Assert.Equal("The Release", result.ReleaseTitle);
        Assert.Single(result.Ratings!);
        Assert.Equal(4m, result.Ratings![0].Value);
        Assert.Equal(2, result.Contributors!.Count);
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "The Album Artist" && contributor.Role == MediaContributorRole.Performer);
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "The Release Producer" && contributor.Role == MediaContributorRole.Producer);
    }

    [Fact]
    public void MapAlbum_WhenThereIsNoRelease_ShouldReturnNullReleaseLevelFields()
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(
            id: Guid.NewGuid().ToString(),
            title: "The Album",
            includeDisambiguation: false,
            includePrimaryType: false,
            secondaryTypes: [],
            includeFirstReleaseDate: false,
            artistCredit: [],
            tags: [],
            genres: [],
            includeRating: false);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, null);

        // Assert
        Assert.Equal("The Album", result.Title);
        Assert.Null(result.Disambiguation);
        Assert.NotNull(result.ReleaseInfo);
        Assert.Null(result.ReleaseInfo.OriginalReleaseDate);
        Assert.Null(result.ReleaseInfo.OriginalReleaseYear);
        Assert.Null(result.ReleaseInfo.ReReleaseDate);
        Assert.Null(result.ReleaseInfo.ReReleaseYear);
        Assert.Null(result.ReleaseInfo.ReleaseCountry);
        Assert.Null(result.ReleaseInfo.ReleaseVersion);
        Assert.Null(result.Language);
        Assert.Empty(result.Tags!);
        Assert.Empty(result.Genres!);
        Assert.Null(result.Script);
        Assert.Empty(result.ReleaseTypes!);
        Assert.Null(result.ReleaseStatus);
        Assert.Null(result.MediaFormat);
        Assert.Null(result.Packaging);
        Assert.Null(result.TotalDiscs);
        Assert.Null(result.TotalTracks);
        Assert.Null(result.Barcode);
        Assert.Empty(result.CatalogNumbers!);
        Assert.Null(result.Label);
        Assert.Null(result.ASIN);
        Assert.Null(result.MusicBrainzReleaseId);
        Assert.Equal(releaseGroup.Id, result.MusicBrainzReleaseGroupId.ToString());
        Assert.Null(result.MusicBrainzReleaseArtistId);
        Assert.Empty(result.Contributors!);
        Assert.Empty(result.Ratings!);
        Assert.Null(result.ReleaseTitle);
    }

    [Fact]
    public void MapAlbum_WhenTheReleaseGroupTitleIsBlank_ShouldFallBackToTheReleaseTitle()
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(title: "   ", artistCredit: [], tags: [], genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(title: "The Release Title", includeReleaseGroup: false, includeTextRepresentation: false, media: [], labelInfo: [], artistCredit: [], tags: [], genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.Equal("The Release Title", result.Title);
        Assert.Equal("The Release Title", result.ReleaseTitle);
    }

    [Fact]
    public void MapAlbum_WhenTheReleaseGroupCreditIsMissing_ShouldFallBackToTheReleaseCredit()
    {
        // Arrange
        Guid releaseArtistId = Guid.NewGuid();
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(artistCredit: [], tags: [], genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(
            includeReleaseGroup: false,
            includeTextRepresentation: false,
            media: [],
            labelInfo: [],
            artistCredit: [_musicBrainzArtistCreditResponseFixture.Create(name: "Release Artist", artist: _musicBrainzArtistResponseFixture.Create(id: releaseArtistId.ToString(), name: "Release Artist"))],
            tags: [],
            genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.Equal(releaseArtistId, result.MusicBrainzReleaseArtistId);
        Assert.Equal("Release Artist", Assert.Single(result.Contributors!).Name!.DisplayName);
    }

    [Fact]
    public void MapAlbum_WhenTheReleaseGroupCreditHasNoArtistId_ShouldFallBackToTheReleaseCreditId()
    {
        // Arrange
        Guid releaseArtistId = Guid.NewGuid();
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(
            artistCredit: [_musicBrainzArtistCreditResponseFixture.Create(name: "Group Artist", artist: _musicBrainzArtistResponseFixture.Create(id: null, includeId: false, name: "Group Artist"))],
            tags: [],
            genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(
            includeReleaseGroup: false,
            includeTextRepresentation: false,
            media: [],
            labelInfo: [],
            artistCredit: [_musicBrainzArtistCreditResponseFixture.Create(name: "Release Artist", artist: _musicBrainzArtistResponseFixture.Create(id: releaseArtistId.ToString(), name: "Release Artist"))],
            tags: [],
            genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.Equal(releaseArtistId, result.MusicBrainzReleaseArtistId);
    }

    [Fact]
    public void MapAlbum_WhenTheReReleaseIsInTheSameYear_ShouldCollapseTheReRelease()
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(firstReleaseDate: "1975-06-01", artistCredit: [], tags: [], genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(date: "1975-11-21", includeReleaseGroup: false, includeTextRepresentation: false, media: [], labelInfo: [], artistCredit: [], tags: [], genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.NotNull(result.ReleaseInfo);
        Assert.Equal(new DateOnly(1975, 6, 1), result.ReleaseInfo.OriginalReleaseDate);
        Assert.Null(result.ReleaseInfo.ReReleaseDate);
        Assert.Null(result.ReleaseInfo.ReReleaseYear);
    }

    [Theory]
    [InlineData("1975-11-21", 1975, 11, 21, 1975)] // an exact date is parsed fully
    [InlineData("1975", null, 0, 0, 1975)] // a bare year yields only the year
    [InlineData("Spring 1975", null, 0, 0, 1975)] // an embedded year is extracted when the value is not a date
    [InlineData("not a date", null, 0, 0, null)] // an unparsable value yields nothing
    [InlineData("2200", null, 0, 0, null)] // a year outside the accepted window yields nothing
    [InlineData("2100", null, 0, 0, 2100)] // the upper boundary is accepted
    [InlineData(" 1975-11-21 ", 1975, 11, 21, 1975)] // surrounding white space is trimmed
    public void MapAlbum_WhenTheFirstReleaseDateHasVariousShapes_ShouldParseIt(string firstReleaseDate, int? expectedYear, int expectedMonth, int expectedDay, int? expectedParsedYear)
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(firstReleaseDate: firstReleaseDate, artistCredit: [], tags: [], genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, null);

        // Assert
        Assert.NotNull(result.ReleaseInfo);
        if (expectedMonth == 0)
            Assert.Null(result.ReleaseInfo.OriginalReleaseDate);
        else
            Assert.Equal(new DateOnly(expectedYear!.Value, expectedMonth, expectedDay), result.ReleaseInfo.OriginalReleaseDate);
        Assert.Equal(expectedParsedYear, result.ReleaseInfo.OriginalReleaseYear);
    }

    [Theory]
    [InlineData("US", ReleaseCountry.US)]
    [InlineData("us", ReleaseCountry.US)] // the code is matched case insensitively
    [InlineData("XE", ReleaseCountry.XE)] // the Europe special code
    [InlineData("XW", ReleaseCountry.XW)] // the Worldwide special code
    [InlineData("AN", ReleaseCountry.AN)] // the Netherlands Antilles special code
    public void MapAlbum_WhenTheReleaseCountryIsKnown_ShouldMapIt(string country, ReleaseCountry expected)
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(artistCredit: [], tags: [], genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(country: country, includeReleaseGroup: false, includeTextRepresentation: false, media: [], labelInfo: [], artistCredit: [], tags: [], genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.Equal(expected, result.ReleaseInfo!.ReleaseCountry);
    }

    [Theory]
    [InlineData("ZZ")] // a valid shape but not a known code
    [InlineData("")] // an empty value
    [InlineData("   ")] // only white space
    public void MapAlbum_WhenTheReleaseCountryIsUnknown_ShouldReturnNullCountry(string country)
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(artistCredit: [], tags: [], genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(country: country, includeReleaseGroup: false, includeTextRepresentation: false, media: [], labelInfo: [], artistCredit: [], tags: [], genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.Null(result.ReleaseInfo!.ReleaseCountry);
    }

    [Theory]
    [InlineData("album", MusicReleaseType.Album)]
    [InlineData("single", MusicReleaseType.Single)]
    [InlineData("ep", MusicReleaseType.Ep)]
    [InlineData("broadcast", MusicReleaseType.Broadcast)]
    [InlineData("other", MusicReleaseType.Other)]
    [InlineData("compilation", MusicReleaseType.Compilation)]
    [InlineData("live", MusicReleaseType.Live)]
    [InlineData("soundtrack", MusicReleaseType.Soundtrack)]
    [InlineData("spokenword", MusicReleaseType.SpokenWord)]
    [InlineData("audio drama", MusicReleaseType.AudioDrama)]
    [InlineData("audiobook", MusicReleaseType.AudioBook)]
    [InlineData("demo", MusicReleaseType.Demo)]
    [InlineData("dj-mix", MusicReleaseType.DjMix)]
    [InlineData("field recording", MusicReleaseType.FieldRecording)]
    [InlineData("interview", MusicReleaseType.Interview)]
    [InlineData("mixtape/street", MusicReleaseType.Mixtape)]
    [InlineData("remix", MusicReleaseType.Remix)]
    public void MapAlbum_WhenThePrimaryReleaseTypeIsKnown_ShouldMapIt(string primaryType, MusicReleaseType expected)
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(primaryType: primaryType, secondaryTypes: [], artistCredit: [], tags: [], genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, null);

        // Assert
        Assert.Equal([expected], result.ReleaseTypes);
    }

    [Fact]
    public void MapAlbum_WhenTheReleaseTypesContainDuplicatesAndUnknowns_ShouldKeepOnlyTheDistinctKnownOnes()
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(primaryType: "Album", secondaryTypes: ["Live", "live", "made-up", "Album"], artistCredit: [], tags: [], genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, null);

        // Assert
        Assert.Equal([MusicReleaseType.Album, MusicReleaseType.Live], result.ReleaseTypes);
    }

    [Theory]
    [InlineData("official", MusicReleaseStatus.Official)]
    [InlineData("promotion", MusicReleaseStatus.Promotion)]
    [InlineData("bootleg", MusicReleaseStatus.Bootleg)]
    [InlineData("pseudo-release", MusicReleaseStatus.PseudoRelease)]
    [InlineData("withdrawn", MusicReleaseStatus.Withdrawn)]
    [InlineData("cancelled", MusicReleaseStatus.Cancelled)]
    [InlineData("expunged", MusicReleaseStatus.Expunged)]
    public void MapAlbum_WhenTheReleaseStatusIsKnown_ShouldMapIt(string status, MusicReleaseStatus expected)
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(artistCredit: [], tags: [], genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(status: status, includeReleaseGroup: false, includeTextRepresentation: false, media: [], labelInfo: [], artistCredit: [], tags: [], genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.Equal(expected, result.ReleaseStatus);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("made-up")]
    public void MapAlbum_WhenTheReleaseStatusIsUnknown_ShouldReturnNullStatus(string? status)
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(artistCredit: [], tags: [], genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(status: status, includeStatus: status is not null, includeReleaseGroup: false, includeTextRepresentation: false, media: [], labelInfo: [], artistCredit: [], tags: [], genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.Null(result.ReleaseStatus);
    }

    [Theory]
    [InlineData("cd", MusicMediaFormat.CD)]
    [InlineData("vinyl", MusicMediaFormat.Vinyl)]
    [InlineData("12\" vinyl", MusicMediaFormat.Inch12Vinyl)]
    [InlineData("10\" vinyl", MusicMediaFormat.Inch10Vinyl)]
    [InlineData("7\" vinyl", MusicMediaFormat.Inch7Vinyl)]
    [InlineData("digital media", MusicMediaFormat.DigitalMedia)]
    [InlineData("cassette", MusicMediaFormat.Cassette)]
    [InlineData("dvd", MusicMediaFormat.DVD)]
    [InlineData("dvd-video", MusicMediaFormat.DVDVideo)]
    [InlineData("dvd-audio", MusicMediaFormat.DVDAudio)]
    [InlineData("sacd", MusicMediaFormat.SACD)]
    [InlineData("hybrid sacd", MusicMediaFormat.HybridSACD)]
    [InlineData("blu-ray", MusicMediaFormat.BluRay)]
    [InlineData("minidisc", MusicMediaFormat.MiniDisc)]
    [InlineData("8cm cd", MusicMediaFormat.Cm8CD)]
    [InlineData("dat", MusicMediaFormat.DAT)]
    [InlineData("other", MusicMediaFormat.Other)]
    [InlineData("made-up", MusicMediaFormat.Other)] // an unknown format still maps to Other
    public void MapAlbum_WhenTheMediaFormatIsKnownOrUnknownButPresent_ShouldMapIt(string format, MusicMediaFormat expected)
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(artistCredit: [], tags: [], genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(
            media: [_musicBrainzMediumResponseFixture.Create(position: 1, format: format, tracks: [], trackCount: 0)],
            includeReleaseGroup: false,
            includeTextRepresentation: false,
            labelInfo: [],
            artistCredit: [],
            tags: [],
            genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.Equal(expected, result.MediaFormat);
    }

    [Theory]
    [InlineData("book", MusicReleasePackaging.Book)]
    [InlineData("box", MusicReleasePackaging.Box)]
    [InlineData("cardboard/paper sleeve", MusicReleasePackaging.CardboardPaperSleeve)]
    [InlineData("cassette case", MusicReleasePackaging.CassetteCase)]
    [InlineData("clamshell case", MusicReleasePackaging.ClamshellCase)]
    [InlineData("digibook", MusicReleasePackaging.Digibook)]
    [InlineData("digifile", MusicReleasePackaging.Digifile)]
    [InlineData("digipak", MusicReleasePackaging.Digipak)]
    [InlineData("discbox slider", MusicReleasePackaging.DiscboxSlider)]
    [InlineData("fatbox", MusicReleasePackaging.Fatbox)]
    [InlineData("gatefold cover", MusicReleasePackaging.GatefoldCover)]
    [InlineData("jewel case", MusicReleasePackaging.JewelCase)]
    [InlineData("keep case", MusicReleasePackaging.KeepCase)]
    [InlineData("longbox", MusicReleasePackaging.Longbox)]
    [InlineData("metal tin", MusicReleasePackaging.MetalTin)]
    [InlineData("plastic sleeve", MusicReleasePackaging.PlasticSleeve)]
    [InlineData("slidepack", MusicReleasePackaging.Slidepack)]
    [InlineData("slim jewel case", MusicReleasePackaging.SlimJewelCase)]
    [InlineData("slipcase", MusicReleasePackaging.Slipcase)]
    [InlineData("snap case", MusicReleasePackaging.SnapCase)]
    [InlineData("snappack", MusicReleasePackaging.SnapPack)]
    [InlineData("super jewel box", MusicReleasePackaging.SuperJewelBox)]
    [InlineData("none", MusicReleasePackaging.None)]
    [InlineData("other", MusicReleasePackaging.Other)]
    [InlineData("made-up", MusicReleasePackaging.Other)] // an unknown packaging still maps to Other
    public void MapAlbum_WhenThePackagingIsKnownOrUnknownButPresent_ShouldMapIt(string packaging, MusicReleasePackaging expected)
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(artistCredit: [], tags: [], genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(packaging: packaging, includeReleaseGroup: false, includeTextRepresentation: false, media: [], labelInfo: [], artistCredit: [], tags: [], genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.Equal(expected, result.Packaging);
    }

    [Fact]
    public void MapAlbum_WhenTheMediaAreMissing_ShouldReturnNullDiscAndTrackCounts()
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(artistCredit: [], tags: [], genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(media: [], includeReleaseGroup: false, includeTextRepresentation: false, labelInfo: [], artistCredit: [], tags: [], genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.Null(result.TotalDiscs);
        Assert.Null(result.TotalTracks);
    }

    [Fact]
    public void MapAlbum_WhenTheMediaFormatIsBlank_ShouldReturnNullMediaFormat()
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(artistCredit: [], tags: [], genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(
            media: [_musicBrainzMediumResponseFixture.Create(position: 1, format: "   ", tracks: [], trackCount: 0)],
            includeReleaseGroup: false,
            includeTextRepresentation: false,
            labelInfo: [],
            artistCredit: [],
            tags: [],
            genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.Null(result.MediaFormat);
    }

    [Fact]
    public void MapAlbum_WhenThePackagingIsBlank_ShouldReturnNullPackaging()
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(artistCredit: [], tags: [], genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(packaging: "   ", includeReleaseGroup: false, includeTextRepresentation: false, media: [], labelInfo: [], artistCredit: [], tags: [], genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.Null(result.Packaging);
    }

    [Fact]
    public void MapTrack_WhenEveryFieldIsPresent_ShouldMapEveryField()
    {
        // Arrange
        Guid recordingId = Guid.NewGuid();
        Guid workId = Guid.NewGuid();
        MusicBrainzWorkResponse work = _musicBrainzWorkResponseFixture.Create(
            id: workId.ToString(),
            title: "The Work",
            type: "Song",
            disambiguation: "the work",
            languages: ["eng"],
            iswcs: ["T-010.154.482-4"],
            relations: [_musicBrainzRelationResponseFixture.Create(type: "composer", targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "The Work Composer"))],
            tags: [],
            genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(
            date: "1975-11-21",
            textRepresentation: _musicBrainzTextRepresentationResponseFixture.Create(language: "eng", script: "Latn"),
            includeReleaseGroup: false,
            media: [],
            labelInfo: [],
            artistCredit: [],
            tags: [],
            genres: []);
        MusicBrainzRecordingResponse recording = _musicBrainzRecordingResponseFixture.Create(
            id: recordingId.ToString(),
            title: "  The Track  ",
            disambiguation: "the track",
            length: 215_400,
            isVideo: true,
            firstReleaseDate: "1975-10-31",
            artistCredit: [_musicBrainzArtistCreditResponseFixture.Create(name: "The Track Artist", includeJoinPhrase: false)],
            isrcs: ["US1A2B3C4D5E", "", "   "],
            releases: [],
            tags: [_musicBrainzTagResponseFixture.Create(name: "Rock")],
            genres: [_musicBrainzTagResponseFixture.Create(name: "Pop")],
            relations: [_musicBrainzRelationResponseFixture.Create(type: "producer", targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "The Producer"))],
            rating: _musicBrainzRatingResponseFixture.Create(value: 3m, votesCount: 4),
            includeRating: true);

        // Act
        AudioMetadataDto result = MusicBrainzMapper.MapTrack(recording, work, release);

        // Assert
        Assert.Equal("The Track", result.Title);
        Assert.Null(result.OriginalTitle);
        Assert.Null(result.Description);
        Assert.Equal("the track", result.Disambiguation);
        Assert.NotNull(result.ReleaseInfo);
        Assert.Equal(new DateOnly(1975, 10, 31), result.ReleaseInfo.OriginalReleaseDate);
        Assert.Equal(1975, result.ReleaseInfo.OriginalReleaseYear);
        Assert.Null(result.ReleaseInfo.ReReleaseDate);
        Assert.Null(result.ReleaseInfo.ReReleaseYear);
        Assert.Null(result.ReleaseInfo.ReleaseCountry);
        Assert.Null(result.ReleaseInfo.ReleaseVersion);
        Assert.NotNull(result.Language);
        Assert.Equal("en", result.Language.LanguageCode);
        Assert.Null(result.OriginalLanguage);
        Assert.Equal(["Rock"], result.Tags!.Select(tag => tag.Name));
        Assert.Equal(["Pop"], result.Genres!.Select(genre => genre.Name));
        Assert.Equal("Latn", result.Script);
        Assert.Null(result.Key);
        Assert.Null(result.Bpm);
        Assert.True(result.IsVideo);
        Assert.NotNull(result.Work);
        Assert.Equal(workId, result.Work.MusicBrainzWorkId);
        Assert.Equal("The Work", result.Work.Title);
        Assert.Equal("Song", result.Work.Type);
        Assert.Equal("en", Assert.Single(result.Work.Languages!).LanguageCode);
        Assert.Equal("T-010.154.482-4", Assert.Single(result.Work.Iswcs!));
        Assert.Equal("US1A2B3C4D5E", Assert.Single(result.Isrcs!).Value);
        Assert.Null(result.Moods);
        Assert.Equal(215, result.DurationInSeconds);
        Assert.Null(result.SampleRate);
        Assert.Null(result.Channels);
        Assert.Null(result.BitDepth);
        Assert.Null(result.AudioCodec);
        Assert.Null(result.Bitrate);
        Assert.Equal(recordingId, result.MusicBrainzRecordingId);
        Assert.Null(result.MusicBrainzTrackId);
        Assert.Single(result.Ratings!);
        Assert.Equal(3m, result.Ratings![0].Value);
        Assert.Equal(4, result.Ratings[0].VoteCount);
        Assert.Equal(3, result.Contributors!.Count);
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "The Track Artist" && contributor.Role == MediaContributorRole.Performer);
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "The Producer" && contributor.Role == MediaContributorRole.Producer);
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "The Work Composer" && contributor.Role == MediaContributorRole.Composer);
    }

    [Fact]
    public void MapTrack_WhenTheRecordingHasNoFirstReleaseDate_ShouldFallBackToTheReleaseDate()
    {
        // Arrange
        MusicBrainzRecordingResponse recording = _musicBrainzRecordingResponseFixture.Create(
            id: Guid.NewGuid().ToString(),
            includeFirstReleaseDate: false,
            artistCredit: [],
            isrcs: [],
            tags: [],
            genres: [],
            relations: [],
            includeRating: false);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(date: "1975-10-31", includeReleaseGroup: false, includeTextRepresentation: false, media: [], labelInfo: [], artistCredit: [], tags: [], genres: []);

        // Act
        AudioMetadataDto result = MusicBrainzMapper.MapTrack(recording, null, release);

        // Assert
        Assert.Equal(new DateOnly(1975, 10, 31), result.ReleaseInfo!.OriginalReleaseDate);
        Assert.Equal(1975, result.ReleaseInfo.OriginalReleaseYear);
    }

    [Fact]
    public void MapTrack_WhenTheRecordingHasAFirstReleaseDate_ShouldPreferItOverTheReleaseDate()
    {
        // Arrange
        MusicBrainzRecordingResponse recording = _musicBrainzRecordingResponseFixture.Create(
            id: Guid.NewGuid().ToString(),
            firstReleaseDate: "1975-11-21",
            artistCredit: [],
            isrcs: [],
            tags: [],
            genres: [],
            relations: [],
            includeRating: false);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(date: "2011-03-14", includeReleaseGroup: false, includeTextRepresentation: false, media: [], labelInfo: [], artistCredit: [], tags: [], genres: []);

        // Act
        AudioMetadataDto result = MusicBrainzMapper.MapTrack(recording, null, release);

        // Assert
        Assert.Equal(new DateOnly(1975, 11, 21), result.ReleaseInfo!.OriginalReleaseDate);
        Assert.Equal(1975, result.ReleaseInfo.OriginalReleaseYear);
    }

    [Theory]
    [InlineData(null, null)] // no length at all
    [InlineData(0L, null)] // a zero length is treated as unknown
    [InlineData(-100L, null)] // a negative length is treated as unknown
    [InlineData(1500L, 1)] // milliseconds are truncated to whole seconds
    [InlineData(215_400L, 215)]
    public void MapTrack_WhenTheRecordingHasALength_ShouldMapTheDurationInSeconds(long? length, int? expectedSeconds)
    {
        // Arrange
        MusicBrainzRecordingResponse recording = _musicBrainzRecordingResponseFixture.Create(
            id: Guid.NewGuid().ToString(),
            length: length,
            includeLength: length is not null,
            artistCredit: [],
            isrcs: [],
            tags: [],
            genres: [],
            relations: [],
            includeRating: false);

        // Act
        AudioMetadataDto result = MusicBrainzMapper.MapTrack(recording, null, null);

        // Assert
        Assert.Equal(expectedSeconds, result.DurationInSeconds);
    }

    [Fact]
    public void MapTrack_WhenTheVideoFlagIsMissing_ShouldReturnFalse()
    {
        // Arrange
        MusicBrainzRecordingResponse recording = _musicBrainzRecordingResponseFixture.Create(
            id: Guid.NewGuid().ToString(),
            includeVideo: false,
            artistCredit: [],
            isrcs: [],
            tags: [],
            genres: [],
            relations: [],
            includeRating: false);

        // Act
        AudioMetadataDto result = MusicBrainzMapper.MapTrack(recording, null, null);

        // Assert
        Assert.False(result.IsVideo);
    }

    [Fact]
    public void MapTrack_WhenTheOptionalCollectionsAreEmpty_ShouldReturnNullWorkAndEmptyCollections()
    {
        // Arrange
        MusicBrainzRecordingResponse recording = _musicBrainzRecordingResponseFixture.Create(
            id: "not-a-guid",
            title: "   ",
            includeDisambiguation: false,
            includeLength: false,
            includeVideo: false,
            includeFirstReleaseDate: false,
            artistCredit: [],
            isrcs: [],
            releases: [],
            tags: [],
            genres: [],
            relations: [],
            includeRating: false);

        // Act
        AudioMetadataDto result = MusicBrainzMapper.MapTrack(recording, null, null);

        // Assert
        Assert.Null(result.Title);
        Assert.Null(result.Disambiguation);
        Assert.NotNull(result.ReleaseInfo);
        Assert.Null(result.ReleaseInfo.OriginalReleaseDate);
        Assert.Null(result.ReleaseInfo.OriginalReleaseYear);
        Assert.Null(result.Language);
        Assert.Empty(result.Tags!);
        Assert.Empty(result.Genres!);
        Assert.Null(result.Script);
        Assert.Null(result.Work);
        Assert.Empty(result.Isrcs!);
        Assert.Null(result.DurationInSeconds);
        Assert.Null(result.MusicBrainzRecordingId);
        Assert.Empty(result.Contributors!);
        Assert.Empty(result.Ratings!);
    }

    [Theory]
    [InlineData(1.0)] // a value only
    [InlineData(2.5)]
    public void MapArtist_WhenOnlyTheRatingValueIsPresent_ShouldMapItWithANullVoteCount(decimal value)
    {
        // Arrange
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            relations: [],
            aliases: [],
            tags: [],
            genres: [],
            rating: _musicBrainzRatingResponseFixture.Create(value: value, includeVotesCount: false),
            includeRating: true);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        AudioRatingDto rating = Assert.Single(result.Ratings!);
        Assert.Equal(value, rating.Value);
        Assert.Null(rating.VoteCount);
    }

    [Fact]
    public void MapArtist_WhenOnlyTheRatingVoteCountIsPresent_ShouldMapItWithANullValue()
    {
        // Arrange
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            relations: [],
            aliases: [],
            tags: [],
            genres: [],
            rating: _musicBrainzRatingResponseFixture.Create(includeValue: false, votesCount: 7),
            includeRating: true);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        AudioRatingDto rating = Assert.Single(result.Ratings!);
        Assert.Null(rating.Value);
        Assert.Equal(7, rating.VoteCount);
    }

    [Fact]
    public void MapArtist_WhenTheRatingHasNoValueNorVotes_ShouldReturnAnEmptyRatingList()
    {
        // Arrange
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            relations: [],
            aliases: [],
            tags: [],
            genres: [],
            rating: _musicBrainzRatingResponseFixture.Create(includeValue: false, includeVotesCount: false),
            includeRating: true);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        Assert.Empty(result.Ratings!);
    }

    [Theory]
    [InlineData("eng", "en", "English")] // a three letter code maps to the neutral culture
    [InlineData("deu", "de", "German")]
    [InlineData("en", "en", "English")] // a two letter code maps too
    [InlineData("zxx", "zxx", "No linguistic content")] // a special MusicBrainz code
    [InlineData("mul", "mul", "Multiple languages")]
    [InlineData("und", "und", "Undetermined")]
    [InlineData("eng/ara", "en", "English")] // only the first code of a multi language value is used
    [InlineData("/", "/", "/")] // a value with no usable part falls back to the raw code
    [InlineData("  eng  ", "en", "English")] // surrounding white space is trimmed
    [InlineData("qqq", "qqq", "qqq")] // an unknown code falls back to the raw code
    public void MapTrack_WhenTheLanguageCodeIsPresent_ShouldMapIt(string language, string expectedCode, string expectedName)
    {
        // Arrange
        MusicBrainzRecordingResponse recording = _musicBrainzRecordingResponseFixture.Create(
            id: Guid.NewGuid().ToString(),
            artistCredit: [],
            isrcs: [],
            tags: [],
            genres: [],
            relations: [],
            includeRating: false);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(
            textRepresentation: _musicBrainzTextRepresentationResponseFixture.Create(language: language, includeScript: false),
            includeReleaseGroup: false,
            media: [],
            labelInfo: [],
            artistCredit: [],
            tags: [],
            genres: []);

        // Act
        AudioMetadataDto result = MusicBrainzMapper.MapTrack(recording, null, release);

        // Assert
        Assert.NotNull(result.Language);
        Assert.Equal(expectedCode, result.Language.LanguageCode);
        Assert.Equal(expectedName, result.Language.LanguageName);
    }

    [Fact]
    public void MapWork_WhenEveryFieldIsPresent_ShouldMapEveryField()
    {
        // Arrange
        Guid workId = Guid.NewGuid();
        MusicBrainzWorkResponse work = _musicBrainzWorkResponseFixture.Create(
            id: workId.ToString(),
            title: "  The Work  ",
            type: "Song",
            languages: ["eng", "zxx", ""],
            iswcs: ["T-1", "   ", "T-2"]);

        // Act
        MusicWorkDto result = MusicBrainzMapper.MapWork(work);

        // Assert
        Assert.Equal(workId, result.MusicBrainzWorkId);
        Assert.Equal("The Work", result.Title);
        Assert.Equal("Song", result.Type);
        Assert.Equal(2, result.Languages!.Count);
        Assert.Equal("en", result.Languages[0].LanguageCode);
        Assert.Equal("zxx", result.Languages[1].LanguageCode);
        Assert.Equal(["T-1", "T-2"], result.Iswcs);
    }

    [Fact]
    public void MapWork_WhenTheIdentifierIsNotAGuid_ShouldReturnNullIdentifier()
    {
        // Arrange
        MusicBrainzWorkResponse work = _musicBrainzWorkResponseFixture.Create(id: "not-a-guid", includeTitle: false, includeType: false, languages: [], iswcs: []);

        // Act
        MusicWorkDto result = MusicBrainzMapper.MapWork(work);

        // Assert
        Assert.Null(result.MusicBrainzWorkId);
        Assert.Null(result.Title);
        Assert.Null(result.Type);
        Assert.Empty(result.Languages!);
        Assert.Empty(result.Iswcs!);
    }

    [Fact]
    public void MapAlbum_WhenTheReleaseGroupCreditsHaveNoUsableName_ShouldFallBackToTheReleaseCredits()
    {
        // Arrange
        Guid releaseArtistId = Guid.NewGuid();
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(
            artistCredit: [_musicBrainzArtistCreditResponseFixture.Create(name: null, includeName: false, includeArtist: false)],
            tags: [],
            genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(
            includeReleaseGroup: false,
            includeTextRepresentation: false,
            media: [],
            labelInfo: [],
            artistCredit: [_musicBrainzArtistCreditResponseFixture.Create(name: "Release Artist", artist: _musicBrainzArtistResponseFixture.Create(id: releaseArtistId.ToString(), name: "Release Artist"))],
            tags: [],
            genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.Equal(releaseArtistId, result.MusicBrainzReleaseArtistId);
        Assert.Equal("Release Artist", Assert.Single(result.Contributors!).Name!.DisplayName);
    }

    [Fact]
    public void MapAlbum_WhenTheReleaseGroupCreditHasNoNameButItsArtistDoes_ShouldFallBackToTheArtistName()
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(
            artistCredit: [_musicBrainzArtistCreditResponseFixture.Create(name: null, includeName: false, artist: _musicBrainzArtistResponseFixture.Create(name: "The Artist"))],
            tags: [],
            genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, null);

        // Assert
        Assert.Equal("The Artist", Assert.Single(result.Contributors!).Name!.DisplayName);
    }

    [Fact]
    public void MapAlbum_WhenAReleaseRelationTargetsAnArtistWithoutAName_ShouldSkipIt()
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(artistCredit: [], tags: [], genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(
            includeReleaseGroup: false,
            includeTextRepresentation: false,
            media: [],
            labelInfo: [],
            artistCredit: [_musicBrainzArtistCreditResponseFixture.Create(name: "The Artist", includeJoinPhrase: false)],
            tags: [],
            genres: [],
            relations: [_musicBrainzRelationResponseFixture.Create(type: "producer", targetType: "artist", includeArtist: false)]);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        MediaContributorDto contributor = Assert.Single(result.Contributors!);
        Assert.Equal("The Artist", contributor.Name!.DisplayName);
    }

    [Fact]
    public void MapArtist_WhenAUrlRelationPrecedesAnOfficialHomepage_ShouldPreferTheOfficialHomepage()
    {
        // Arrange
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            relations:
            [
                _musicBrainzRelationResponseFixture.Create(type: "wikidata", targetType: "url", includeUrl: true, url: _musicBrainzUrlResponseFixture.Create(resource: "https://wikidata.example")),
                _musicBrainzRelationResponseFixture.Create(type: "official homepage", targetType: "url", includeUrl: true, url: _musicBrainzUrlResponseFixture.Create(resource: "https://artist.example"))
            ],
            aliases: [],
            tags: [],
            genres: []);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        Assert.Equal("https://artist.example", result.Website);
    }

    [Fact]
    public void MapArtist_WhenAnArtistRelationHasNoArtist_ShouldSkipIt()
    {
        // Arrange
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            relations: [_musicBrainzRelationResponseFixture.Create(type: "producer", targetType: "artist", includeArtist: false)],
            aliases: [],
            tags: [],
            genres: []);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        Assert.Empty(result.Contributors!);
    }

    [Fact]
    public void MapArtist_WhenTheSameNameIsCreditedWithTwoRoles_ShouldKeepBothContributors()
    {
        // Arrange
        MusicBrainzArtistResponse artist = _musicBrainzArtistResponseFixture.Create(
            relations:
            [
                _musicBrainzRelationResponseFixture.Create(type: "producer", targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "The Person")),
                _musicBrainzRelationResponseFixture.Create(type: "composer", targetType: "artist", includeArtist: true, artist: _musicBrainzArtistResponseFixture.Create(name: "The Person"))
            ],
            aliases: [],
            tags: [],
            genres: []);

        // Act
        ArtistMetadataDto result = MusicBrainzMapper.MapArtist(artist);

        // Assert
        Assert.Equal(2, result.Contributors!.Count);
    }

    [Fact]
    public void MapAlbum_WhenNeitherCreditHasAnArtistIdentifier_ShouldReturnNullReleaseArtistId()
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(
            artistCredit: [_musicBrainzArtistCreditResponseFixture.Create(name: "Group Artist", includeArtist: false)],
            tags: [],
            genres: []);
        MusicBrainzReleaseResponse release = _musicBrainzReleaseResponseFixture.Create(
            includeReleaseGroup: false,
            includeTextRepresentation: false,
            media: [],
            labelInfo: [],
            artistCredit: [_musicBrainzArtistCreditResponseFixture.Create(name: "Release Artist", includeArtist: false)],
            tags: [],
            genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, release);

        // Assert
        Assert.Null(result.MusicBrainzReleaseArtistId);
    }

    [Fact]
    public void MapAlbum_WhenTheReleaseIsNullAndTheCreditHasNoArtistIdentifier_ShouldReturnNullReleaseArtistId()
    {
        // Arrange
        MusicBrainzReleaseGroupResponse releaseGroup = _musicBrainzReleaseGroupResponseFixture.Create(
            artistCredit: [_musicBrainzArtistCreditResponseFixture.Create(name: "Group Artist", includeArtist: false)],
            tags: [],
            genres: []);

        // Act
        AlbumMetadataDto result = MusicBrainzMapper.MapAlbum(releaseGroup, null);

        // Assert
        Assert.Null(result.MusicBrainzReleaseArtistId);
    }
}
