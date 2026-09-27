#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.DataAccess.Common.Persistence;

/// <summary>
/// Copies the editable scalar values of a mapped repository entity onto its tracked counterpart, while leaving the primary key and the audit columns under the sole ownership of the persistence medium.
/// </summary>
internal static class EditableValuesCopier
{
    /// <summary>
    /// Copies the scalar values of <paramref name="incoming"/> onto the tracked <paramref name="tracked"/> entity, except for its primary key and its audit columns,
    /// so that the tracked row is only considered modified when an editable value actually changed, and its stored identity is never reassigned.
    /// </summary>
    /// <remarks>
    /// The primary key is not copied, so that a mapped entity carrying a different, or unset, key can never retarget the tracked row. The audit interceptor stamps the
    /// modification columns of the rows that are actually modified, so the audit columns are not copied either: copying them from the mapped entity would mark untouched
    /// rows as modified, which would then be stamped with a change that the user never made.
    /// </remarks>
    /// <typeparam name="TEntity">The type of the tracked and mapped repository entity.</typeparam>
    /// <param name="context">The Entity Framework context that tracks <paramref name="tracked"/>.</param>
    /// <param name="tracked">The tracked entity whose editable values are updated.</param>
    /// <param name="incoming">The mapped entity carrying the desired values.</param>
    public static void CopyEditableValues<TEntity>(LuminaDbContext context, TEntity tracked, TEntity incoming) where TEntity : class, IAuditableEntity
    {
        EntityEntry<TEntity> trackedEntry = context.Entry(tracked);
        EntityEntry<TEntity> incomingEntry = context.Entry(incoming);

        // The primary key and the audit columns are owned by the persistence medium, so they are never copied from the mapped entity.
        HashSet<string> excludedProperties =
        [
            .. (trackedEntry.Metadata.FindPrimaryKey()?.Properties.Select(property => property.Name) ?? []),
            nameof(IAuditableEntity.CreatedOnUtc),
            nameof(IAuditableEntity.CreatedBy),
            nameof(IAuditableEntity.UpdatedOnUtc),
            nameof(IAuditableEntity.UpdatedBy)
        ];

        foreach (IProperty property in trackedEntry.Metadata.GetProperties())
        {
            if (excludedProperties.Contains(property.Name))
                continue;
            trackedEntry.Property(property.Name).CurrentValue = incomingEntry.Property(property.Name).CurrentValue;
        }
    }
}
