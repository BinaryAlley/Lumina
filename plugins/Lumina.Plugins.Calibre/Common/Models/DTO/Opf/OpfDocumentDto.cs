#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Plugins.Calibre.Common.Models.DTO.Opf;

/// <summary>
/// Data transfer object for the metadata read from a Calibre OPF file.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
internal sealed class OpfDocumentDto
{
    /// <summary>
    /// Gets or sets the title of the book.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the book, in HTML format.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the publisher of the book.
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    /// Gets or sets the publication date of the book.
    /// </summary>
    public DateTimeOffset? PublishDate { get; set; }

    /// <summary>
    /// Gets or sets the language code of the book.
    /// </summary>
    public string? LanguageCode { get; set; }

    /// <summary>
    /// Gets or sets the name of the series the book belongs to, if applicable.
    /// </summary>
    public string? Series { get; set; }

    /// <summary>
    /// Gets or sets the index of the book within its series, if applicable.
    /// </summary>
    public double? SeriesIndex { get; set; }

    /// <summary>
    /// Gets or sets the personal rating of the book, on a scale of 0 to 10.
    /// </summary>
    public int? Rating { get; set; }

    /// <summary>
    /// Gets or sets the href of the cover image, relative to the directory of the book.
    /// </summary>
    public string? CoverHref { get; set; }

    /// <summary>
    /// Gets the creators of the book.
    /// </summary>
    public List<OpfCreatorDto> Creators { get; } = [];

    /// <summary>
    /// Gets the contributors of the book.
    /// </summary>
    public List<OpfContributorDto> Contributors { get; } = [];

    /// <summary>
    /// Gets the identifiers of the book, with their schemes.
    /// </summary>
    public List<OpfIdentifierDto> Identifiers { get; } = [];

    /// <summary>
    /// Gets the subjects of the book, used as tags.
    /// </summary>
    public List<string> Subjects { get; } = [];
}
