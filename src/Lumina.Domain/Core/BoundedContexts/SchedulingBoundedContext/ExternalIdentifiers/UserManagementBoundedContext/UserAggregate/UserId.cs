#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;

/// <summary>
/// Value object for the Id of a user, duplicated from the UserManagement bounded context to keep the cross bounded context references decoupled.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class UserId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private UserId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UserId"/> class.
    /// </summary>
    /// <returns>The created <see cref="UserId"/> instance.</returns>
    public static UserId CreateUnique()
    {
        return new UserId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UserId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="UserId"/> instance.</param>
    /// <returns>The created <see cref="UserId"/> instance.</returns>
    public static UserId Create(Guid value)
    {
        return new UserId(value);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
