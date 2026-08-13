#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryMetadataProviders;

/// <summary>
/// Query for getting the metadata providers configured for a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose metadata providers are retrieved.</param>
public record GetLibraryMetadataProvidersQuery(
    Guid LibraryId
) : IQuery;
