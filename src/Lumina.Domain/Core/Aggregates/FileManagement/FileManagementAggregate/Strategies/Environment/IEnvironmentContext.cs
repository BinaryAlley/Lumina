#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Environment;

/// <summary>
/// Interface for defining the contract of an environment-specific context, encapsulating all platform-specific services and strategies.
/// </summary>
public interface IEnvironmentContext
{
    /// <summary>
    /// Gets the service for identifying and handling different file types.
    /// </summary>
    IFileTypeService FileTypeService { get; }

    /// <summary>
    /// Gets the service for providing file system files.
    /// </summary>
    IFileProviderService FileProviderService { get; }

    /// <summary>
    /// Gets the service for providing file system directories.
    /// </summary>
    IDirectoryProviderService DirectoryProviderService { get; }
}