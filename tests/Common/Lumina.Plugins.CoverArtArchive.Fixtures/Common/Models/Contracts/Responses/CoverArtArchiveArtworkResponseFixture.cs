#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.CoverArtArchive.Common.Models.Contracts.Responses;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.CoverArtArchive.Fixtures.Common.Models.Contracts.Responses;

/// <summary>
/// Fixture class for the <see cref="CoverArtArchiveArtworkResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class CoverArtArchiveArtworkResponseFixture
{
    private readonly CoverArtArchiveImageResponseFixture _coverArtArchiveImageResponseFixture = new();

    /// <summary>
    /// Creates a new <see cref="CoverArtArchiveArtworkResponse"/> instance with randomized test data.
    /// </summary>
    /// <param name="images">Optional. The images of the artwork.</param>
    /// <param name="includeImages">Whether the images should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="CoverArtArchiveArtworkResponse"/> instance.</returns>
    public CoverArtArchiveArtworkResponse Create(
        List<CoverArtArchiveImageResponse?>? images = null,
        bool includeImages = true)
    {
        return new CoverArtArchiveArtworkResponse
        {
            Images = includeImages ? images ?? [.. _coverArtArchiveImageResponseFixture.CreateMany()] : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="CoverArtArchiveArtworkResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="CoverArtArchiveArtworkResponse"/> instances.</returns>
    public List<CoverArtArchiveArtworkResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
