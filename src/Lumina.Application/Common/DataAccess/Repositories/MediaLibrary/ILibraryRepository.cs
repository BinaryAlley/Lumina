#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;

/// <summary>
/// Interface for the repository for media libraries.
/// </summary>
public interface ILibraryRepository : IRepository<LibraryEntity>,
                                      IGetByIdRepositoryAction<LibraryEntity, Guid>,
                                      IGetAllRepositoryAction<LibraryEntity>,
                                      IInsertRepositoryAction<LibraryEntity>,
                                      IUpdateRepositoryAction<LibraryEntity>,
                                      IDeleteByIdRepositoryAction<Guid>
{
    /// <summary>
    /// Gets all media libraries that are marked as enabled and unlocked, from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="LibraryEntity"/>, or an error.</returns>
    Task<ErrorOr<IEnumerable<LibraryEntity>>> GetAllEnabledAndUnlockedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets all media libraries that are marked as enabled from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="LibraryEntity"/>, or an error.</returns>
    Task<ErrorOr<IEnumerable<LibraryEntity>>> GetAllEnabledAsync(CancellationToken cancellationToken);
}
