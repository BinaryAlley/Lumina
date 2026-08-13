#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathSeparator;

/// <summary>
/// Query for retrieving the file system path separator.
/// </summary>
public record GetPathSeparatorQuery() : IQuery;