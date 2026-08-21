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
/// Fixture class for the <see cref="ThemeArchiveResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeArchiveResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ThemeArchiveResponse"/>.
    /// </summary>
    /// <param name="bytes">Optional. The bytes of the ZIP archive.</param>
    /// <param name="fileName">Optional. The file name to expose when downloading the archive.</param>
    /// <param name="contentType">Optional. The MIME content type of the archive.</param>
    /// <returns>The created <see cref="ThemeArchiveResponse"/>.</returns>
    public ThemeArchiveResponse Create(byte[]? bytes = null, string? fileName = null, string? contentType = null)
    {
        return new ThemeArchiveResponse(
            bytes ?? Encoding.UTF8.GetBytes(_faker.Lorem.Paragraph()),
            fileName ?? $"{_faker.Lorem.Word()}.zip",
            contentType ?? _faker.System.MimeType());
    }

    /// <summary>
    /// Creates a list of <see cref="ThemeArchiveResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ThemeArchiveResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
