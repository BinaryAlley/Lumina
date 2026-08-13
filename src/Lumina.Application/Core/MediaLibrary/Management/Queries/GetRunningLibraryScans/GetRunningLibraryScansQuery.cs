#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;

/// <summary>
/// Query for getting the list of ongoing media library scans.
/// </summary>
public record GetRunningLibraryScansQuery : IQuery;
