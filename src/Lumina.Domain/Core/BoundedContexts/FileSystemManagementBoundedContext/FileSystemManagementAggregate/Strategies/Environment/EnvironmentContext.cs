#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;

/// <summary>
/// Represents the context for environment-specific operations and services.
/// </summary>
internal class EnvironmentContext : IEnvironmentContext
{
    /// <summary>
    /// Gets the service for identifying and handling different file types.
    /// </summary>
    public IFileTypeService FileTypeService { get; private set; }

    /// <summary>
    /// Gets the service for performing file-related operations.
    /// </summary>
    public IFileProviderService FileProviderService { get; private set; }

    /// <summary>
    /// Gets the service for performing directory-related operations.
    /// </summary>
    public IDirectoryProviderService DirectoryProviderService { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentContext"/> class.
    /// </summary>
    /// <param name="directoryProviderService">Injected service for directory providers.</param>
    /// <param name="fileProviderService">Injected service for file providers.</param>
    /// <param name="fileTypeService">Injected service for file type.</param>
    public EnvironmentContext(IDirectoryProviderService directoryProviderService, IFileProviderService fileProviderService, IFileTypeService fileTypeService)
    {
        FileTypeService = fileTypeService;
        FileProviderService = fileProviderService;
        DirectoryProviderService = directoryProviderService;
    }
}
