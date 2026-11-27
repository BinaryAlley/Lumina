#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Drives.Queries.GetDrives;

/// <summary>
/// Query for retrieving the list of drives.
/// </summary>
public record GetDrivesQuery() : IQuery;