#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text;
#endregion

namespace Lumina.Presentation.Api.Fixtures.Core.Endpoints.Themes;

/// <summary>
/// Fixture class for building theme pack ZIP archives used by the theme integration tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class TestThemeArchiveFixture
{
    /// <summary>
    /// Builds a valid theme pack ZIP archive in memory, containing a <c>theme.json</c> manifest and a default template.
    /// </summary>
    /// <param name="themeId">Optional manifest id of the theme; a unique id is generated when not supplied.</param>
    /// <returns>The bytes of the theme pack ZIP archive.</returns>
    public byte[] Create(string? themeId = null)
    {
        string resolvedThemeId = themeId ?? $"test-theme-{Guid.NewGuid():N}";

        using MemoryStream stream = new();
        using (ZipArchive archive = new(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            ZipArchiveEntry manifestEntry = archive.CreateEntry("theme.json");
            using (StreamWriter writer = new(manifestEntry.Open(), Encoding.UTF8))
            {
                writer.Write($$"""
                {
                  "schemaVersion": 1,
                  "id": "{{resolvedThemeId}}",
                  "name": "Test Theme",
                  "description": "A test theme used by the integration tests.",
                  "author": "Test Author",
                  "version": "1.0.0",
                  "templates": {
                    "default": "templates/default.html"
                  }
                }
                """);
            }

            ZipArchiveEntry templateEntry = archive.CreateEntry("templates/default.html");
            using (StreamWriter writer = new(templateEntry.Open(), Encoding.UTF8))
                writer.Write("<html><body><h1>Default template</h1></body></html>");
        }

        return stream.ToArray();
    }
}
