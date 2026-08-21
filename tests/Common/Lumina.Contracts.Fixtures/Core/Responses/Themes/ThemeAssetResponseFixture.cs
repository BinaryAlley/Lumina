#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Themes;

/// <summary>
/// Fixture class for the <see cref="ThemeAssetResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeAssetResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ThemeAssetResponse"/>.
    /// </summary>
    /// <param name="bytes">Optional. The bytes of the asset file.</param>
    /// <param name="contentType">Optional. The MIME content type of the asset file.</param>
    /// <returns>The created <see cref="ThemeAssetResponse"/>.</returns>
    public ThemeAssetResponse Create(byte[]? bytes = null, string? contentType = null)
    {
        return new ThemeAssetResponse(
            bytes ?? Encoding.UTF8.GetBytes(_faker.Lorem.Paragraph()),
            contentType ?? _faker.System.MimeType());
    }

    /// <summary>
    /// Creates a list of <see cref="ThemeAssetResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ThemeAssetResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
