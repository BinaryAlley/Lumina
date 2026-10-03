#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Plugins.CoverArtArchive.Common.Models.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.CoverArtArchive.Fixtures.Common.Models.Contracts.Responses;

/// <summary>
/// Fixture class for the <see cref="CoverArtArchiveImageResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class CoverArtArchiveImageResponseFixture
{
    private static readonly string[] s_knownTypes =
    [
        "Front", "Back", "Booklet", "Medium", "Tray", "Spine", "Obi", "Sticker", "Poster", "Liner", "Watermark", "Other"
    ];

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="CoverArtArchiveImageResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="image">Optional. The URL of the full size image.</param>
    /// <param name="includeImage">Whether the image URL should be included, or forced to <see langword="null"/>.</param>
    /// <param name="thumbnail">Optional. The URL of the thumbnail of the image.</param>
    /// <param name="types">Optional. The types of the image.</param>
    /// <param name="includeTypes">Whether the types should be included, or forced to <see langword="null"/>.</param>
    /// <param name="isFront">Optional. Whether the image is the front cover.</param>
    /// <param name="isBack">Optional. Whether the image is the back cover.</param>
    /// <param name="comment">Optional. The comment describing the image.</param>
    /// <returns>A configured <see cref="CoverArtArchiveImageResponse"/> instance.</returns>
    public CoverArtArchiveImageResponse Create(
        string? image = null,
        bool includeImage = true,
        string? thumbnail = null,
        List<string?>? types = null,
        bool includeTypes = true,
        bool? isFront = null,
        bool? isBack = null,
        string? comment = null)
    {
        return new CoverArtArchiveImageResponse
        {
            Image = includeImage ? image ?? $"https://coverartarchive.org/release/{Guid.NewGuid():D}/{_faker.Random.Long(1, 999_999_999)}.jpg" : null,
            Thumbnail = thumbnail ?? $"https://coverartarchive.org/release/{Guid.NewGuid():D}/thumb.jpg",
            Types = includeTypes ? types ?? [_faker.PickRandom(s_knownTypes)] : null,
            IsFront = isFront ?? _faker.Random.Bool(),
            IsBack = isBack ?? _faker.Random.Bool(),
            Comment = comment ?? _faker.Lorem.Sentence()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="CoverArtArchiveImageResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="CoverArtArchiveImageResponse"/> instances.</returns>
    public List<CoverArtArchiveImageResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
