#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.PathTemplate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.PathTemplate;

/// <summary>
/// Catalog of the path parts available for book libraries, together with the default structure the application expects on disk.
/// </summary>
/// <remarks>
/// The default structure mirrors the Calibre library layout, where each book lives in an author directory and a book directory named after
/// the title and the Calibre identifier, holding the book file named after the title and the author.
/// </remarks>
public sealed class BookLibraryPathPartCatalog : ILibraryPathPartCatalog
{
    /// <summary>
    /// Gets the media library type that this catalog supports.
    /// </summary>
    public LibraryType SupportedLibraryType => LibraryType.Book;

    /// <summary>
    /// Gets the parts that can be used in the path template of a book library.
    /// </summary>
    /// <returns>The available path parts.</returns>
    public IReadOnlyList<LibraryPathPartDefinition> GetPartDefinitions()
    {
        return
        [
            new(LibraryPathPartKind.Literal, LibraryPathValueType.None, string.Empty, false),
            new(LibraryPathPartKind.Separator, LibraryPathValueType.None, string.Empty, false),
            new(LibraryPathPartKind.Author, LibraryPathValueType.Text, "{0}", false),
            new(LibraryPathPartKind.Title, LibraryPathValueType.Text, "{0}", false),
            new(LibraryPathPartKind.Series, LibraryPathValueType.Text, "{0}", true),
            new(LibraryPathPartKind.SeriesNumber, LibraryPathValueType.Integer, "{0}", true),
            new(LibraryPathPartKind.BookId, LibraryPathValueType.Integer, "{0}", false),
            new(LibraryPathPartKind.Extension, LibraryPathValueType.Text, "{0}", false)
        ];
    }

    /// <summary>
    /// Gets the default path template of a book library, describing the Calibre structure the application expects.
    /// </summary>
    /// <returns>The default path template.</returns>
    public LibraryPathTemplate GetDefaultTemplate()
    {
        return LibraryPathTemplate.Create(
        [
            LibraryPathPart.Create(LibraryPathPartKind.Author, "{0}", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Separator, null, false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Title, "{0}", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Literal, " (", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.BookId, "{0}", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Literal, ")", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Separator, null, false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Title, "{0}", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Literal, " - ", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Author, "{0}", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Literal, ".", false).Value,
            LibraryPathPart.Create(LibraryPathPartKind.Extension, "{0}", false).Value
        ]);
    }
}
