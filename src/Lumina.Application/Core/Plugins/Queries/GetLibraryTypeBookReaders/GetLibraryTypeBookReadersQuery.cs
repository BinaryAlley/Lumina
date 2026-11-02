#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryTypeBookReaders;

/// <summary>
/// Query for getting the book readers available for a media library type.
/// </summary>
/// <param name="LibraryType">The media library type whose available book readers are retrieved.</param>
[DebuggerDisplay("LibraryType: {LibraryType}")]
public record GetLibraryTypeBookReadersQuery(
    string? LibraryType
) : IQuery;
