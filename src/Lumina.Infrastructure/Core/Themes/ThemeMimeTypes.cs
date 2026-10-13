#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
#endregion

namespace Lumina.Infrastructure.Core.Themes;

/// <summary>
/// Resolves the MIME content type of theme files based on their file extension.
/// </summary>
internal static class ThemeMimeTypes
{
    private static readonly FrozenDictionary<string, string> s_fileExtensionToMimeType = new Dictionary<string, string>
    {
        { ".htm", "text/html" },
        { ".html", "text/html" },
        { ".css", "text/css" },
        { ".js", "application/javascript" },
        { ".json", "application/json" },
        { ".png", "image/png" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".gif", "image/gif" },
        { ".webp", "image/webp" },
        { ".svg", "image/svg+xml" },
        { ".ico", "image/x-icon" },
        { ".woff", "font/woff" },
        { ".woff2", "font/woff2" },
        { ".ttf", "font/ttf" },
        { ".txt", "text/plain" }
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the MIME content type of a file based on its extension.
    /// </summary>
    /// <param name="filePath">The path of the file to get the content type of.</param>
    /// <returns>The MIME content type, or <c>application/octet-stream</c> when the extension is unknown.</returns>
    public static string GetMimeType(string filePath)
    {
        return s_fileExtensionToMimeType.TryGetValue(Path.GetExtension(filePath), out string? mimeType) ? mimeType : "application/octet-stream";
    }
}
