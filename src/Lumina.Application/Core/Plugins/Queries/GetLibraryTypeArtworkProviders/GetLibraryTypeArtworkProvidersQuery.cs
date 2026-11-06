#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryTypeArtworkProviders;

/// <summary>
/// Query for getting the artwork providers available for a media library type.
/// </summary>
/// <param name="LibraryType">The media library type whose available artwork providers are retrieved.</param>
[DebuggerDisplay("LibraryType: {LibraryType}")]
public record GetLibraryTypeArtworkProvidersQuery(
    string? LibraryType
) : IQuery;
