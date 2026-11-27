#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicWorkDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicWorkDtoFixture
{
    private readonly Faker _faker = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicWorkDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="musicBrainzWorkId">Optional. The MusicBrainz identifier of the work.</param>
    /// <param name="title">Optional. The title of the work.</param>
    /// <param name="type">Optional. The MusicBrainz type of the work.</param>
    /// <param name="languages">Optional. The languages of the work.</param>
    /// <param name="iswcs">Optional. The ISWC codes of the work.</param>
    /// <param name="includeMusicBrainzWorkId">Whether the MusicBrainz work Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTitle">Whether the title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeType">Whether the type should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLanguages">Whether the languages should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIswcs">Whether the ISWC codes should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicWorkDto"/> instance.</returns>
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
        return new MusicWorkDto
        {
            MusicBrainzWorkId = includeMusicBrainzWorkId ? (musicBrainzWorkId ?? _faker.Random.Guid()) : null,
            Title = includeTitle ? (title ?? _faker.Lorem.Sentence(3)) : null,
            Type = includeType ? (type ?? _faker.Lorem.Word()) : null,
            Languages = includeLanguages ? (languages ?? [.. _languageInfoDtoFixture.CreateMany(_faker.Random.Number(1, 3))]) : null,
            Iswcs = includeIswcs ? (iswcs ?? [.. Enumerable.Range(0, _faker.Random.Number(1, 3)).Select(_ => $"T{_faker.Random.Number(100000000, 999999999)}")]) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicWorkDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicWorkDto"/> instances.</returns>
    public List<MusicWorkDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
