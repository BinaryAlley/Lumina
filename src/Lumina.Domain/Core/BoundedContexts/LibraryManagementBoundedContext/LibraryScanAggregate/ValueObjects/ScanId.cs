#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a media library scan.
/// </summary>
[DebuggerDisplay("Value: {Value}")]
public sealed class ScanId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScanId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private ScanId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ScanId"/> class.
    /// </summary>
    /// <returns>The created <see cref="ScanId"/> instance.</returns>
    public static ScanId CreateUnique()
    {
        return new ScanId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ScanId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="ScanId"/> instance.</param>
    /// <returns>The created <see cref="ScanId"/> instance.</returns>
    public static ScanId Create(Guid value)
    {
        return new ScanId(value);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
