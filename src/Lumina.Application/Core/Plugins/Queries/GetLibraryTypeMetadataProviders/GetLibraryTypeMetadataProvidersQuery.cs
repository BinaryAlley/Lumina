#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryTypeMetadataProviders;

/// <summary>
/// Query for getting the metadata providers available for a media library type.
/// </summary>
/// <param name="LibraryType">The media library type whose available metadata providers are retrieved.</param>
[DebuggerDisplay("LibraryType: {LibraryType}")]
public record GetLibraryTypeMetadataProvidersQuery(
    string? LibraryType
) : IQuery;
