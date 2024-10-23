#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Drives.Queries.GetDrives;

/// <summary>
/// Query for retrieving the list of drives.
/// </summary>
public record GetDrivesQuery() : IRequest<ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>>;