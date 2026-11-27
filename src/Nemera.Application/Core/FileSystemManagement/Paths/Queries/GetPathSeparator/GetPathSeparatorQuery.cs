#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.Paths.Queries.GetPathSeparator;

/// <summary>
/// Query for retrieving the file system path separator.
/// </summary>
public record GetPathSeparatorQuery() : IQuery;