#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Common.Interceptors;

/// <summary>
/// Interceptor for updating audit fields automatically for entities implementing <see cref="IAuditableEntity"/>.
/// </summary>
public sealed class UpdateAuditableEntitiesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAuditableEntitiesInterceptor"/> class.
    /// </summary>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="dateTimeProvider">Injected service for time related functionality.</param>
    public UpdateAuditableEntitiesInterceptor(ICurrentUserService currentUserService, IDateTimeProvider dateTimeProvider)
    {
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Intercepts the synchronous SaveChanges operation to update audit fields.
    /// </summary>
    /// <param name="eventData">The event data containing the <see cref="DbContext"/>.</param>
    /// <param name="result">The result of the SaveChanges operation.</param>
    /// <returns>The interception result.</returns>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Intercepts the asynchronous SaveChangesAsync operation to update audit fields.
    /// </summary>
    /// <param name="eventData">The event data containing the <see cref="DbContext"/>.</param>
    /// <param name="result">The result of the SaveChanges operation.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A <see cref="ValueTask"/> containing the interception result.</returns>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Updates the audit fields for all changed entities that implement IAuditableEntity.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/> containing the entities to update.</param>
    private void UpdateAuditableEntities(DbContext? context)
    {
        if (context is null)
            return;

        Guid? userId = _currentUserService.UserId;
        DateTime utcNow = _dateTimeProvider.UtcNow;

        // retrieve all entities implementing IAuditableEntity that are being tracked and have either Added or Modified state
        foreach (EntityEntry<IAuditableEntity> entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                SetCreationProperties(entry.Entity, userId, utcNow);
            else if(entry.State == EntityState.Modified)
                SetModificationProperties(entry, userId, utcNow);
        }
    }

    /// <summary>
    /// Sets the creation audit properties for a newly added entity.
    /// </summary>
    /// <param name="entity">The entity being created.</param>
    /// <param name="userId">The Id of the user creating the entity.</param>
    /// <param name="utcNow">The current UTC timestamp.</param>
    private static void SetCreationProperties(IAuditableEntity entity, Guid? userId, DateTime utcNow)
    {
        entity.CreatedOnUtc = utcNow;
        entity.CreatedBy = userId ?? default;
        entity.UpdatedOnUtc = null;
        entity.UpdatedBy = null;
    }

    /// <summary>
    /// Sets the modification audit properties for an updated entity while protecting creation properties.
    /// </summary>
    /// <param name="entry">The entity entry being modified.</param>
    /// <param name="userId">The Id of the user modifying the entity.</param>
    /// <param name="utcNow">The current UTC timestamp.</param>
    private static void SetModificationProperties(EntityEntry<IAuditableEntity> entry, Guid? userId, DateTime utcNow)
    {
        // prevent modification of creation properties
        entry.Property(auditableEntity => auditableEntity.CreatedOnUtc).IsModified = false;
        entry.Property(auditableEntity => auditableEntity.CreatedBy).IsModified = false;

        // set modification properties
        entry.Entity.UpdatedOnUtc = utcNow;
        entry.Entity.UpdatedBy = userId;
    }
}
