#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.AudioLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.AudioLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="AudioMetadata"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioMetadataFixture
{
    private readonly Faker _faker = new();
    private readonly GenreFixture _genreFixture = new();
    private readonly TagFixture _tagFixture = new();
    private readonly ReleaseInfoFixture _releaseInfoFixture = new();
    private readonly LanguageInfoFixture _languageInfoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="AudioMetadata"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the audio.</param>
    /// <param name="originalTitle">Optional. The original title of the audio.</param>
    /// <param name="durationInSeconds">Optional. The duration of the audio, in seconds.</param>
    /// <param name="sampleRate">Optional. The sample rate of the audio, in Hz.</param>
    /// <param name="channels">Optional. The number of audio channels.</param>
    /// <param name="releaseInfo">Optional. The release information of the audio.</param>
    /// <param name="description">Optional. The description of the audio.</param>
    /// <param name="genres">Optional. The genres of the audio.</param>
    /// <param name="tags">Optional. The tags associated with the audio.</param>
    /// <param name="language">Optional. The language of the audio.</param>
    /// <param name="originalLanguage">Optional. The original language of the audio.</param>
    /// <param name="bitDepth">Optional. The bit depth of the audio.</param>
    /// <param name="audioCodec">Optional. The audio codec used.</param>
    /// <param name="bitrate">Optional. The bitrate of the audio, in kbps.</param>
    /// <returns>The created <see cref="AudioMetadata"/>.</returns>
    public AudioMetadata Create(
        string? title = null,
        Optional<string>? originalTitle = null,
        int? durationInSeconds = null,
        int? sampleRate = null,
        int? channels = null,
        ReleaseInfo? releaseInfo = null,
        Optional<string>? description = null,
        List<Genre>? genres = null,
        List<Tag>? tags = null,
        Optional<LanguageInfo>? language = null,
        Optional<LanguageInfo>? originalLanguage = null,
        Optional<int>? bitDepth = null,
        Optional<string>? audioCodec = null,
        Optional<int>? bitrate = null)
    {
        return AudioMetadata.Create(
            title ?? _faker.Music.Genre(),
            originalTitle ?? Optional<string>.None(),
            durationInSeconds ?? _faker.Random.Int(60, 7200),
            sampleRate ?? _faker.Random.Int(44100, 192000),
            channels ?? _faker.Random.Int(1, 8),
            releaseInfo ?? _releaseInfoFixture.Create(),
            description ?? Optional<string>.Some(_faker.Lorem.Sentence()),
            genres ?? [_genreFixture.Create(), _genreFixture.Create()],
            tags ?? [_tagFixture.Create(), _tagFixture.Create()],
            language ?? Optional<LanguageInfo>.Some(_languageInfoFixture.Create()),
            originalLanguage ?? Optional<LanguageInfo>.None(),
            bitDepth ?? Optional<int>.Some(_faker.Random.Int(8, 32)),
            audioCodec ?? Optional<string>.Some("PCM"),
            bitrate ?? Optional<int>.Some(_faker.Random.Int(128, 320))).Value;
    }

    /// <summary>
    /// Creates multiple <see cref="AudioMetadata"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AudioMetadata"/> instances.</returns>
    public List<AudioMetadata> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
