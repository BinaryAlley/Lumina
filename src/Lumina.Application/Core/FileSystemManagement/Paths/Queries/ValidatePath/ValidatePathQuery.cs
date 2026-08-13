#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
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
) : IQuery;
