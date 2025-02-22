#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;

/// <summary>
/// Query for getting the list of ongoing media library scans.
/// </summary>
public record GetRunningLibraryScansQuery : IRequest<ErrorOr<IEnumerable<MediaLibraryScanProgressResponse>>>;
