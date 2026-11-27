#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Services.PathTemplate;

/// <summary>
/// Catalog of the path parts available for music libraries, together with the default structure the application expects on disk.
/// </summary>
public sealed class MusicLibraryPathPartCatalog : ILibraryPathPartCatalog
{
    /// <summary>
    /// Gets the media library type that this catalog supports.
    /// </summary>
    public LibraryType SupportedLibraryType => LibraryType.Music;

    /// <summary>
    /// Gets the parts that can be used in the path template of a music library.
    /// </summary>
    /// <returns>The available path parts.</returns>
    public IReadOnlyList<LibraryPathPartDefinition> GetPartDefinitions()
    {
        return
        [
            new(LibraryPathPartKind.Literal, LibraryPathValueType.None, string.Empty, false),
            new(LibraryPathPartKind.Separator, LibraryPathValueType.None, string.Empty, false),
            new(LibraryPathPartKind.Artist, LibraryPathValueType.Text, "{0}", false),
            new(LibraryPathPartKind.ReleaseType, LibraryPathValueType.Enum, "{0}", false),
            new(LibraryPathPartKind.ReleaseYear, LibraryPathValueType.Year, "{0}", false),
            new(LibraryPathPartKind.ReleaseName, LibraryPathValueType.Text, "{0}", false),
            new(LibraryPathPartKind.TrackNumber, LibraryPathValueType.Integer, "{0:00}", false),
            new(LibraryPathPartKind.TrackName, LibraryPathValueType.Text, "{0}", false),
            new(LibraryPathPartKind.Extension, LibraryPathValueType.Text, "{0}", false),
            new(LibraryPathPartKind.DiscNumber, LibraryPathValueType.Integer, "{0:00}", true)
        ];
    }

    /// <summary>
    /// Gets the default path template of a music library, describing the ideal structure the application expects. The disc segment is
    /// optional, so that both single disc and multi disc releases are understood.
    /// </summary>
    /// <returns>The default path template.</returns>
    public LibraryPathTemplate GetDefaultTemplate()
    {
        return LibraryPathTemplate.Create(
        [
            LibraryPathPart.Create(LibraryPathPartKind.Artist, "{0}", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Separator, null, false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.ReleaseType, "{0}", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Separator, null, false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.ReleaseYear, "{0}", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Literal, " - ", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.ReleaseName, "{0}", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Separator, null, true).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Literal, "Disk ", true).Value,
            LibraryPathPart.Create(LibraryPathPartKind.DiscNumber, "{0:00}", true).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Separator, null, false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.TrackNumber, "{0:00}", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Literal, " - ", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.TrackName, "{0}", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Literal, ".", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Extension, "{0}", false).Value
        ]);
    }
}
