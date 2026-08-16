#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a set of user settings.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class UserSettingsId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserSettingsId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private UserSettingsId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UserSettingsId"/> class.
    /// </summary>
    /// <returns>The created <see cref="UserSettingsId"/> instance.</returns>
    public static UserSettingsId CreateUnique()
    {
        return new UserSettingsId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UserSettingsId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="UserSettingsId"/> instance.</param>
    /// <returns>The created <see cref="UserSettingsId"/> instance.</returns>
    public static UserSettingsId Create(Guid value)
    {
        return new UserSettingsId(value);
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
