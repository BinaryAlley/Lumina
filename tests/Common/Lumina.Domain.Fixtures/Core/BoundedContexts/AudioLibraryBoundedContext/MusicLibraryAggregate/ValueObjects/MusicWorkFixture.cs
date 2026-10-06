#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MusicWork"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicWorkFixture
{
    private readonly Faker _faker = new();
    private readonly MusicBrainzIdFixture _musicBrainzIdFixture = new();
    private readonly LanguageInfoFixture _languageInfoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="MusicWork"/>.
    /// </summary>
    /// <param name="musicBrainzWorkId">Optional. The MusicBrainz identifier of the work.</param>
    /// <param name="title">Optional. The title of the work.</param>
    /// <param name="type">Optional. The MusicBrainz type of the work.</param>
    /// <param name="languages">Optional. The languages of the work.</param>
    /// <param name="iswcs">Optional. The ISWC codes of the work.</param>
    /// <returns>The created <see cref="MusicWork"/>.</returns>
    public MusicWork Create(
        MusicBrainzId? musicBrainzWorkId = null,
        string? title = null,
        Optional<string>? type = null,
        List<LanguageInfo>? languages = null,
        List<string>? iswcs = null)
    {
        Result<MusicWork> workResult = MusicWork.Create(
            musicBrainzWorkId ?? _musicBrainzIdFixture.Create(),
            title ?? _faker.Music.Genre(),
            type ?? Optional<string>.Some(_faker.Random.ArrayElement(["Song", "Instrumental"])),
            languages ?? _languageInfoFixture.CreateMany(_faker.Random.Int(1, 2)),
            iswcs ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 2)).Select(_ => $"T{_faker.Random.AlphaNumeric(10)}")]);

        if (workResult.IsFailure)
            throw new System.InvalidOperationException("Failed to create MusicWork: " + string.Join(", ", workResult.Errors));
        return workResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="MusicWork"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicWork"/> instances.</returns>
    public List<MusicWork> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
