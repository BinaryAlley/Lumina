#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.Drives.Queries.GetDrives;

/// <summary>
/// Query for retrieving the list of drives.
/// </summary>
public record GetDrivesQuery() : IQuery;