#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MediaContributors;

/// <summary>
/// Interface for the repository for media contributors.
/// </summary>
public interface IMediaContributorRepository : IRepository<MediaContributorEntity>,
                                               IInsertRepositoryAction<MediaContributorEntity>
{
    /// <summary>
    /// Gets the media contributor whose display name matches the provided <paramref name="displayName"/>, or creates and inserts a new one
    /// when no contributor with that name exists yet, guaranteeing a single contributor per person.
    /// </summary>
    /// <param name="displayName">The name by which the contributor is popularly known.</param>
    /// <param name="legalName">The optional legal name of the contributor.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the existing or newly created <see cref="MediaContributorEntity"/>, or an error.</returns>
    Task<Result<MediaContributorEntity>> FindOrCreateByDisplayNameAsync(string displayName, string? legalName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the media contributors identified by the provided <paramref name="ids"/>.
    /// </summary>
    /// <param name="ids">The unique identifiers of the media contributors to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the retrieved <see cref="MediaContributorEntity"/>s, or an error.</returns>
    Task<Result<IReadOnlyList<MediaContributorEntity>>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
}
