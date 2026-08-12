#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.Plugins;
using Mediator;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.Plugins.Queries.GetLibraryMetadataProviders;

/// <summary>
/// Query for getting the metadata providers configured for a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose metadata providers are retrieved.</param>
public record GetLibraryMetadataProvidersQuery(
    Guid LibraryId
) : IRequest<ErrorOr<IReadOnlyList<LibraryMetadataProviderResponse>>>;
