#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.Paths.Queries.ValidatePath;

/// <summary>
/// Query for validating a path.
/// </summary>
/// <param name="Path">The path to be validated.</param>
[DebuggerDisplay("Path: {Path}")]
public record ValidatePathQuery(
    string? Path
) : IQuery;
