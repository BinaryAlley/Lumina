#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
#endregion

namespace Lumina.Infrastructure.Fixtures.Core.Themes;

/// <summary>
/// Fixture class for generating theme pack ZIP archives used by theme tests.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ThemePackFixture
{
    /// <summary>
    /// Creates a valid theme pack ZIP archive containing a manifest, the template files referenced by it and a preview asset.
    /// </summary>
    /// <param name="themeId">Optional theme id written to the manifest.</param>
    /// <param name="name">Optional display name written to the manifest.</param>
    /// <param name="description">Optional description written to the manifest.</param>
    /// <param name="author">Optional author written to the manifest.</param>
    /// <param name="version">Optional version written to the manifest.</param>
    /// <param name="schemaVersion">Optional schema version written to the manifest.</param>
    /// <param name="templates">Optional template mappings written to the manifest.</param>
    /// <param name="preview">Optional preview path written to the manifest, or <see langword="null"/> to omit the preview.</param>
    /// <param name="defaultTemplateContent">Optional content for the default template file.</param>
    /// <param name="includePreviewAsset">Whether to include the preview asset file in the archive.</param>
    /// <param name="additionalFiles">Optional additional files written into the archive.</param>
    /// <returns>The created theme pack ZIP archive.</returns>
    public byte[] Create(
        string themeId = "test-theme",
        string? name = null,
        string? description = null,
        string? author = null,
        string? version = null,
        int? schemaVersion = null,
        Dictionary<string, string>? templates = null,
        string? preview = "assets/preview.png",
        string? defaultTemplateContent = null,
        bool includePreviewAsset = true,
        IReadOnlyDictionary<string, string>? additionalFiles = null)
    {
        Dictionary<string, string> manifestTemplates = templates ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["default"] = "templates/default.html"
        };

        string manifestJson = CreateManifestJson(
            themeId,
            name,
            description,
            author,
            version,
            schemaVersion,
            manifestTemplates,
            preview);

        List<ThemeArchiveEntry> entries = [new("theme.json", manifestJson)];
        foreach ((string key, string templatePath) in manifestTemplates)
            entries.Add(new(templatePath, defaultTemplateContent ?? $"<html><body>{key} template</body></html>"));

        if (preview is not null && includePreviewAsset)
            entries.Add(new("assets/preview.png", "preview-image-bytes"));

        if (additionalFiles is not null)
            entries.AddRange(additionalFiles.Select(file => new ThemeArchiveEntry(file.Key, file.Value)));

        return CreateArchive([.. entries]);
    }

    /// <summary>
    /// Creates the JSON content of a theme manifest with test defaults that can be overridden per field.
    /// </summary>
    /// <param name="themeId">Optional theme id written to the manifest.</param>
    /// <param name="name">Optional display name written to the manifest.</param>
    /// <param name="description">Optional description written to the manifest.</param>
    /// <param name="author">Optional author written to the manifest.</param>
    /// <param name="version">Optional version written to the manifest.</param>
    /// <param name="schemaVersion">Optional schema version written to the manifest.</param>
    /// <param name="templates">Optional template mappings written to the manifest.</param>
    /// <param name="preview">Optional preview path written to the manifest, or <see langword="null"/> to omit the preview.</param>
    /// <returns>The created manifest JSON content.</returns>
    public string CreateManifestJson(
        string themeId = "test-theme",
        string? name = null,
        string? description = null,
        string? author = null,
        string? version = null,
        int? schemaVersion = null,
        Dictionary<string, string>? templates = null,
        string? preview = "assets/preview.png")
    {
        object manifest = new
        {
            schemaVersion = schemaVersion ?? 1,
            id = themeId,
            name = name ?? "Test Theme",
            description = description ?? "A theme used for testing.",
            author = author ?? "Lumina Tests",
            version = version ?? "1.0.0",
            preview,
            templates = templates ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["default"] = "templates/default.html"
            }
        };

        return JsonSerializer.Serialize(manifest);
    }

    /// <summary>
    /// Creates a ZIP archive from the provided entries, allowing tests to build arbitrary valid and invalid theme packs.
    /// </summary>
    /// <param name="entries">The entries to write into the archive.</param>
    /// <returns>The created ZIP archive bytes.</returns>
    public byte[] CreateArchive(params ThemeArchiveEntry[] entries)
    {
        using MemoryStream output = new();
        using (ZipArchive archive = new(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (ThemeArchiveEntry entry in entries)
            {
                ZipArchiveEntry zipEntry = archive.CreateEntry(entry.Path);
                if (entry.UnixFileType is not null)
                    zipEntry.ExternalAttributes = (entry.UnixFileType.Value << 16) | 0x81A4;

                using StreamWriter writer = new(zipEntry.Open());
                writer.Write(entry.Content);
            }
        }

        return output.ToArray();
    }

    /// <summary>
    /// Creates multiple valid theme pack ZIP archives with randomized test data.
    /// </summary>
    /// <param name="count">Number of archives to create.</param>
    /// <returns>List of configured theme pack ZIP archives.</returns>
    public List<byte[]> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(index => Create(themeId: $"test-theme-{index}"))];
    }
}
