#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;

/// <summary>
/// Data transfer object for deserializing media settings.
/// </summary>
[DebuggerDisplay("{SECTION_NAME}")]
public class MediaSettingsDto
{
    public const string SECTION_NAME = "MediaSettings";

    /// <summary>
    /// Gets or sets the root directory where media files are stored.
    /// </summary>
    public required string RootDirectory { get; init; }

    /// <summary>
    /// Gets or sets the directory where media library files are stored.
    /// </summary>
    public required string LibrariesDirectory { get; init; }

    /// <summary>
    /// Gets or sets the directory where the media item files are stored.
    /// </summary>
    public required string BooksDirectory { get; init; }
}
