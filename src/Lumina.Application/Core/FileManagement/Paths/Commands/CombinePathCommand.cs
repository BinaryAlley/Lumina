#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Commands;

/// <summary>
/// Command for combining two file system path.
/// </summary>
/// <param name="OriginalPath">The path to combine to.</param>
/// <param name="NewPath">The path to combine with.</param>
public record CombinePathCommand(
    string OriginalPath, string NewPath
) : IRequest<ErrorOr<PathSegmentResponse>>;