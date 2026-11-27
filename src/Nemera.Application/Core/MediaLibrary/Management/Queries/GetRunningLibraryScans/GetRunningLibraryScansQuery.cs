#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;

/// <summary>
/// Query for getting the list of ongoing media library scans.
/// </summary>
public record GetRunningLibraryScansQuery : IQuery;
