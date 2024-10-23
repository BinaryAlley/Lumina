#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Mediator;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.ValidatePath;

/// <summary>
/// Query for validating a path.
/// </summary>
/// <param name="Path">The path to be validated.</param>
[DebuggerDisplay("Path: {Path}")]
public record ValidatePathQuery(
    string? Path
) : IRequest<PathValidResponse>;
