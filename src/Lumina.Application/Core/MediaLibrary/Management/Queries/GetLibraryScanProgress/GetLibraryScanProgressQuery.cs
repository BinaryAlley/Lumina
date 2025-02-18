#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Mediator;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;

/// <summary>
/// Query for getting the progress of a media library scan.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose scan progress is requested.</param>
/// <param name="ScanId">The Id of the media library scan whose progress is requested.</param>
public record GetLibraryScanProgressQuery(
    Guid LibraryId,
    Guid ScanId
) : IRequest<ErrorOr<MediaLibraryScanProgressResponse>>;
