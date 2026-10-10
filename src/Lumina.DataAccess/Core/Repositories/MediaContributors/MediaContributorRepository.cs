#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Repositories.MediaContributors;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.MediaContributors;

/// <summary>
/// Repository for media contributors.
/// </summary>
internal sealed class MediaContributorRepository : IMediaContributorRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaContributorRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public MediaContributorRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new media contributor.
    /// </summary>
    /// <param name="contributor">The media contributor to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(MediaContributorEntity contributor, CancellationToken cancellationToken)
    {
        bool doesContributorExist = await _luminaDbContext.MediaContributors
            .AnyAsync(repositoryContributor => repositoryContributor.Id == contributor.Id, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (doesContributorExist)
            return Errors.MediaContributor.MediaContributorAlreadyExists;

        // A contributor is unique by its display name, so the same person can never be registered twice under different spellings of the same name.
        bool doesContributorNameExist = await _luminaDbContext.MediaContributors
            .AnyAsync(repositoryContributor => EF.Functions.Collate(repositoryContributor.DisplayName, "NOCASE") == contributor.DisplayName, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (doesContributorNameExist)
            return Errors.MediaContributor.MediaContributorAlreadyExists;

        _luminaDbContext.MediaContributors.Add(contributor);
        return Result.Created;
    }

    /// <summary>
    /// Gets the media contributor whose display name matches the provided <paramref name="displayName"/>, or creates and inserts a new one
    /// when no contributor with that name exists yet, guaranteeing a single contributor per person.
    /// </summary>
    /// <param name="displayName">The name by which the contributor is popularly known.</param>
    /// <param name="legalName">The optional legal name of the contributor.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the existing or newly created <see cref="MediaContributorEntity"/>, or an error.</returns>
    public async Task<Result<MediaContributorEntity>> FindOrCreateByDisplayNameAsync(string displayName, string? legalName, CancellationToken cancellationToken)
    {
        // the comparison is case-insensitive, so that "Stephen King" and "stephen king" are never treated as distinct contributors
        string normalizedDisplayName = displayName.ToLowerInvariant();

        // Contributors added earlier in the same unit of work are not returned by a database query, so the change tracker is consulted
        // first; without this, the same contributor discovered for several items of the same save would be inserted more than once and
        // violate the unique index on the display name.
        MediaContributorEntity? trackedContributor = _luminaDbContext.MediaContributors.Local
            .FirstOrDefault(contributor => contributor.DisplayName.ToLowerInvariant() == normalizedDisplayName);
        if (trackedContributor is not null)
            return trackedContributor;

        // The lookup must use the same case-insensitive collation as the unique index on the display name, so that a name whose case is
        // not folded by the storage medium (for example one that starts with a non-ASCII uppercase letter) is still matched, instead of
        // being inserted again and violating the unique index.
        MediaContributorEntity? existingContributor = await _luminaDbContext.MediaContributors
            .FirstOrDefaultAsync(contributor => EF.Functions.Collate(contributor.DisplayName, "NOCASE") == displayName, cancellationToken)
            .ConfigureAwait(false);
        if (existingContributor is not null)
            return existingContributor;

        MediaContributorEntity contributor = new()
        {
            Id = Guid.NewGuid(),
            DisplayName = displayName,
            LegalName = legalName,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.Empty,
            UpdatedBy = null
        };
        _luminaDbContext.MediaContributors.Add(contributor);
        return contributor;
    }

    /// <summary>
    /// Gets the media contributors identified by the provided <paramref name="ids"/>.
    /// </summary>
    /// <param name="ids">The unique identifiers of the media contributors to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the retrieved <see cref="MediaContributorEntity"/>s, or an error.</returns>
    public async Task<Result<IReadOnlyList<MediaContributorEntity>>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.MediaContributors
            .Where(contributor => ids.Contains(contributor.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
