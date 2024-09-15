#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Commands;

/// <summary>
/// Command for splitting a file system path.
/// </summary>
/// <param name="Path">The path to split.</param>
public record SplitPathCommand(
    string Path
) : IRequest<ErrorOr<IEnumerable<PathSegmentResponse>>>;