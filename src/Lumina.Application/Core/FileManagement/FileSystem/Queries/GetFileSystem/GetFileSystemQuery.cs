#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileManagement;
using Mediator;
#endregion

namespace Lumina.Application.Core.FileManagement.FileSystem.Queries.GetFileSystem;

/// <summary>
/// Query for retrieving the file system type.
/// </summary>
public record GetFileSystemQuery() : IRequest<FileSystemTypeResponse>;
