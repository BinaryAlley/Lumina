#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Common.Persistence;

/// <summary>
/// Resolves shared reference rows, whose identity is their name, so that each distinct name is represented by a single tracked instance.
/// </summary>
/// <remarks>
/// This is the toolkit's only concern expressed over a family of entity shapes, through the <see cref="ISharedReferenceEntity"/> marker interface:
/// a shared reference is resolved by its name, the stored row wins over the incoming instance, and within-request duplicates are collapsed to one instance.
/// No per-entity method is ever needed, so a new shared reference type adds no new persistence type. A shared reference whose name is null or whitespace
/// cannot be keyed and would be lost silently, so it fails the resolution with the caller-provided error instead of being dropped.
/// </remarks>
internal static class SharedReferenceResolver
{
    /// <summary>
    /// Resolves the single <typeparamref name="TEntity"/> instance that represents each distinct name carried by <paramref name="incomingEntities"/>.
    /// </summary>
    /// <remarks>
    /// The stored row wins over the incoming instance, so that a shared reference that is already persisted is reused instead of inserted again. The
    /// instances are also collapsed by name, so that a name shared by several parents of the same request is only ever tracked once.
    /// </remarks>
    /// <typeparam name="TEntity">The type of the shared reference entity.</typeparam>
    /// <param name="context">The Entity Framework context the stored rows are read through.</param>
    /// <param name="incomingEntities">The shared references carried by the entity that is about to be persisted.</param>
    /// <param name="invalidNameError">The error returned when a shared reference carries a name that is null or whitespace.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A result containing either a dictionary that maps each distinct name to the single instance that represents it, or an error.</returns>
    internal static async Task<Result<IReadOnlyDictionary<string, TEntity>>> ResolveAsync<TEntity>(LuminaDbContext context, IEnumerable<TEntity> incomingEntities, Error invalidNameError, CancellationToken cancellationToken)
        where TEntity : class, ISharedReferenceEntity
    {
        // A shared reference whose name is null or whitespace cannot be keyed, and the database requires the name to be present, so the whole resolution
        // fails instead of silently dropping the reference.
        Dictionary<string, TEntity> resolvedByName = new(StringComparer.Ordinal);
        foreach (TEntity incomingEntity in incomingEntities)
        {
            string? name = incomingEntity.Name;
            if (string.IsNullOrWhiteSpace(name))
                return invalidNameError;
            resolvedByName.TryAdd(name, incomingEntity);
        }

        if (resolvedByName.Count == 0)
            return resolvedByName;

        // Prefer the instances that the context already tracks, so that a reference resolved earlier in the same request does not trigger another query.
        HashSet<string> namesResolvedFromTrackedEntities = [];
        foreach (TEntity trackedEntity in context.Set<TEntity>().Local)
        {
            string? trackedName = trackedEntity.Name;
            if (!string.IsNullOrWhiteSpace(trackedName) && resolvedByName.ContainsKey(trackedName))
            {
                resolvedByName[trackedName] = trackedEntity;
                namesResolvedFromTrackedEntities.Add(trackedName);
            }
        }

        // Only the names that are not already tracked are read from the storage medium.
        List<string> unresolvedNames = [.. resolvedByName.Keys.Where(name => !namesResolvedFromTrackedEntities.Contains(name))];
        if (unresolvedNames.Count > 0)
        {
            List<TEntity> storedEntities = await context.Set<TEntity>()
                .Where(entity => unresolvedNames.Contains(EF.Property<string>(entity, nameof(ISharedReferenceEntity.Name))))
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            foreach (TEntity storedEntity in storedEntities)
                resolvedByName[storedEntity.Name!] = storedEntity;
        }

        return resolvedByName;
    }

    /// <summary>
    /// Maps <paramref name="incomingEntities"/> to the single resolved instance that represents each of their names.
    /// </summary>
    /// <typeparam name="TEntity">The type of the shared reference entity.</typeparam>
    /// <param name="incomingEntities">The shared references that must be mapped to their resolved instances.</param>
    /// <param name="resolvedEntities">The resolved instances, as returned by <see cref="ResolveAsync{TEntity}"/>.</param>
    /// <returns>The resolved instances, without any duplicate.</returns>
    internal static IReadOnlyCollection<TEntity> Normalize<TEntity>(IEnumerable<TEntity> incomingEntities, IReadOnlyDictionary<string, TEntity> resolvedEntities)
        where TEntity : class, ISharedReferenceEntity
    {
        HashSet<TEntity> normalizedEntities = [];
        foreach (TEntity incomingEntity in incomingEntities)
        {
            string? name = incomingEntity.Name;
            if (string.IsNullOrWhiteSpace(name))
                continue;
            if (resolvedEntities.TryGetValue(name, out TEntity? resolvedEntity) && resolvedEntity is not null)
                normalizedEntities.Add(resolvedEntity);
        }
        return normalizedEntities;
    }

    /// <summary>
    /// Reconciles the shared reference collection <paramref name="existingEntities"/> against <paramref name="incomingEntities"/>, reusing the stored rows whose names already exist.
    /// </summary>
    /// <typeparam name="TEntity">The type of the shared reference entity.</typeparam>
    /// <param name="context">The Entity Framework context the stored rows are read through.</param>
    /// <param name="existingEntities">The tracked collection that is reconciled in place.</param>
    /// <param name="incomingEntities">The desired shared references.</param>
    /// <param name="invalidNameError">The error returned when a shared reference carries a name that is null or whitespace.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    internal static async Task<Result<Updated>> ReconcileAsync<TEntity>(LuminaDbContext context, ICollection<TEntity> existingEntities, IEnumerable<TEntity> incomingEntities, Error invalidNameError, CancellationToken cancellationToken)
        where TEntity : class, ISharedReferenceEntity
    {
        List<TEntity> incomingList = [.. incomingEntities];
        Result<IReadOnlyDictionary<string, TEntity>> resolveResult = await ResolveAsync(context, incomingList, invalidNameError, cancellationToken).ConfigureAwait(false);
        if (resolveResult.IsFailure)
            return resolveResult.Errors;

        CollectionReconciler.Reconcile(
            existingEntities,
            Normalize(incomingList, resolveResult.Value),
            existingEntity => existingEntity.Name!,
            incomingEntity => incomingEntity.Name!,
            shouldReplace: (existingEntity, incomingEntity) => false,
            createNew: incomingEntity => incomingEntity);
        return Result.Updated;
    }
}
