#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using System.Collections.Generic;
#endregion

namespace Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;

/// <summary>
/// Interface for the service for handling drives.
/// </summary>
public interface IDriveService
{
    /// <summary>
    /// Retrieves the list of drives.
    /// </summary>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of drives or an error.</returns>
    Result<IEnumerable<FileSystemItem>> GetDrives();
}
