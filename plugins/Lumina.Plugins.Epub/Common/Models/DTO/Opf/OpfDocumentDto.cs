#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Plugins.Epub.Common.Models.DTO.Opf;

/// <summary>
/// Data transfer object for the parsed OPF package document of an EPUB.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
internal sealed class OpfDocumentDto
{
    /// <summary>
    /// Gets or sets the title of the book.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the author of the book, if applicable.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Gets the manifest items of the EPUB.
    /// </summary>
    public List<OpfManifestItemDto> Items { get; } = [];

    /// <summary>
    /// Gets the manifest items of the EPUB, keyed by their Id.
    /// </summary>
    public Dictionary<string, OpfManifestItemDto> ItemsById { get; } = [];

    /// <summary>
    /// Gets the Ids of the manifest items that are reading sections, in reading order.
    /// </summary>
    public List<string> SpineIds { get; } = [];

    /// <summary>
    /// Gets or sets the Id of the manifest item of the navigation document, if applicable.
    /// </summary>
    public string? NavItemId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the manifest item of the NCX document, if applicable.
    /// </summary>
    public string? NcxItemId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the manifest item of the cover image, if applicable.
    /// </summary>
    public string? CoverItemId { get; set; }
}
