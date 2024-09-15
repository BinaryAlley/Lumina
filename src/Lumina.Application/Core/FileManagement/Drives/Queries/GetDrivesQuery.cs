#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.FileManagement.Drives.Queries;

/// <summary>
/// Query for retrieving the list of drives.
/// </summary>
public record GetDrivesQuery() : IRequest<ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>>;