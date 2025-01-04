#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;

/// <summary>
/// Interface for the service for seeding initial file system structure.
/// </summary>
public interface IFileSystemStructureSeedService
{
    /// <summary>
    /// Sets up the default file system directories needed by the application.
    /// </summary>
    /// <param name="rootPath">The base path where the directories will be created relative to.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    ErrorOr<Created> SetDefaultDirectories(string rootPath);
}
