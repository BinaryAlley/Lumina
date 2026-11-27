#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryArtworkProviders;

/// <summary>
/// Query for getting the artwork providers configured for a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose artwork providers are retrieved.</param>
public record GetLibraryArtworkProvidersQuery(
    Guid LibraryId
) : IQuery;
