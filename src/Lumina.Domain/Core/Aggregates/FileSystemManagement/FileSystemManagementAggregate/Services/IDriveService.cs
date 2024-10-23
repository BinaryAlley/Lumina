#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Services;

/// <summary>
/// Interface for the service for handling drives.
/// </summary>
public interface IDriveService
{
    /// <summary>
    /// Retrieves the list of drives.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of drives or an error.</returns>
    ErrorOr<IEnumerable<FileSystemItem>> GetDrives();
}
