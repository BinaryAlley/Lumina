#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Repositories.MediaContributors;
using Lumina.DataAccess.Core.UoW;
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

        MediaContributorEntity? existingContributor = await _luminaDbContext.MediaContributors
            .FirstOrDefaultAsync(contributor => contributor.DisplayName.ToLower() == normalizedDisplayName, cancellationToken)
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
