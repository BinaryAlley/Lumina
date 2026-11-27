#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryPathTemplateParts;

/// <summary>
/// Query for getting the path parts available for a media library type.
/// </summary>
/// <param name="LibraryType">The media library type whose available path parts are retrieved.</param>
[DebuggerDisplay("LibraryType: {LibraryType}")]
public record GetLibraryPathTemplatePartsQuery(
    string? LibraryType
) : IQuery;
