#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicWorkDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicWorkDtoFixture
{
    private readonly Faker _faker = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="MusicWorkDto"/>.
    /// </summary>
    /// <param name="musicBrainzWorkId">Optional. The MusicBrainz identifier of the work.</param>
    /// <param name="title">Optional. The title of the work.</param>
    /// <param name="type">Optional. The MusicBrainz type of the work.</param>
    /// <param name="languages">Optional. The list of languages of the work.</param>
    /// <param name="iswcs">Optional. The list of ISWC of the work.</param>
    /// <param name="includeMusicBrainzWorkId">Whether the MusicBrainz work Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTitle">Whether the title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeType">Whether the type should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLanguages">Whether the languages should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIswcs">Whether the ISWCs should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="MusicWorkDto"/>.</returns>
    public MusicWorkDto Create(
        Guid? musicBrainzWorkId = null,
        string? title = null,
        string? type = null,
        List<LanguageInfoDto>? languages = null,
        List<string>? iswcs = null,
        bool includeMusicBrainzWorkId = true,
        bool includeTitle = true,
        bool includeType = true,
        bool includeLanguages = true,
        bool includeIswcs = true)
    {
        return new MusicWorkDto(
            includeMusicBrainzWorkId ? (musicBrainzWorkId ?? _faker.Random.Guid()) : null,
            includeTitle ? (title ?? _faker.Music.Genre()) : null,
            includeType ? (type ?? _faker.Random.ArrayElement(["Song", "Instrumental"])) : null,
            includeLanguages ? (languages ?? _languageInfoDtoFixture.CreateMany(_faker.Random.Int(1, 2))) : null,
            includeIswcs ? (iswcs ?? GenerateIswcs()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="MusicWorkDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<MusicWorkDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Creates a list of random ISWC codes.
    /// </summary>
    /// <returns>The generated ISWC codes.</returns>
    private List<string> GenerateIswcs()
    {
        return [.. Enumerable.Range(0, _faker.Random.Int(1, 2)).Select(_ => $"T{_faker.Random.AlphaNumeric(10)}")];
    }
}
