#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using System;
#endregion

namespace Lumina.DataAccess.Common.Persistence;

/// <summary>
/// Copies the editable scalar values of a mapped repository entity onto its tracked counterpart, while leaving the audit columns under the sole ownership of the auditing interceptor.
/// </summary>
internal static class EditableValuesCopier
{
    /// <summary>
    /// Copies the scalar values of <paramref name="incoming"/> onto the tracked <paramref name="tracked"/> entity, restoring the identity and audit columns afterwards,
    /// so that the tracked row is only considered modified when an editable value actually changed.
    /// </summary>
    /// <remarks>
    /// The audit interceptor stamps the modification columns of the rows that are actually modified. Copying the audit columns from the mapped entity would mark untouched
    /// rows as modified, which would then be stamped with a change that the user never made.
    /// </remarks>
    /// <typeparam name="TEntity">The type of the tracked and mapped repository entity.</typeparam>
    /// <param name="context">The Entity Framework context that tracks <paramref name="tracked"/>.</param>
    /// <param name="tracked">The tracked entity whose editable values are updated.</param>
    /// <param name="incoming">The mapped entity carrying the desired values.</param>
    public static void CopyEditableValues<TEntity>(LuminaDbContext context, TEntity tracked, TEntity incoming) where TEntity : class, IAuditableEntity
    {
        DateTime createdOnUtc = tracked.CreatedOnUtc;
        Guid createdBy = tracked.CreatedBy;
        DateTime? updatedOnUtc = tracked.UpdatedOnUtc;
        Guid? updatedBy = tracked.UpdatedBy;
        context.Entry(tracked).CurrentValues.SetValues(incoming);
        tracked.CreatedOnUtc = createdOnUtc;
        tracked.CreatedBy = createdBy;
        tracked.UpdatedOnUtc = updatedOnUtc;
        tracked.UpdatedBy = updatedBy;
    }
}
