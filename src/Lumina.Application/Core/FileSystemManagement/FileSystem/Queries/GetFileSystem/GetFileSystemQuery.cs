#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileSystemManagement.FileSystem;
using Mediator;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.FileSystem.Queries.GetFileSystem;

/// <summary>
/// Query for retrieving the file system type.
/// </summary>
public record GetFileSystemQuery() : IRequest<FileSystemTypeResponse>;
