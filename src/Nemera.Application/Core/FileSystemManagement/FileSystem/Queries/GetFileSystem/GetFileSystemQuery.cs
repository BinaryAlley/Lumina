#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.FileSystem.Queries.GetFileSystem;

/// <summary>
/// Query for retrieving the file system type.
/// </summary>
public record GetFileSystemQuery() : IQuery;
