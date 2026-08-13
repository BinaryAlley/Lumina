#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.FileSystem.Queries.GetFileSystem;

/// <summary>
/// Query for retrieving the file system type.
/// </summary>
public record GetFileSystemQuery() : IQuery;
