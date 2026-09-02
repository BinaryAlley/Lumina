#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Plugins.Epub.Common.Models.DTO.Opf;

/// <summary>
/// Data transfer object for a manifest item of the OPF package document of an EPUB.
/// </summary>
[DebuggerDisplay("Id: {Id}")]
internal sealed class OpfManifestItemDto
{
    /// <summary>
    /// Gets or sets the Id of the manifest item.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the href of the manifest item.
    /// </summary>
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the media type of the manifest item.
    /// </summary>
    public string MediaType { get; set; } = string.Empty;
}
