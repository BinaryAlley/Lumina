#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a book.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class FileSystemId : EntityId<Guid>
{
    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private FileSystemId(Guid value) : base(value)
    {
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="FileSystemId"/> class.
    /// </summary>
    /// <returns>The created <see cref="FileSystemId"/> instance.</returns>
    public static FileSystemId CreateUnique()
    {
        return new FileSystemId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="FileSystemId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="FileSystemId"/> instance.</param>
    /// <returns>The created <see cref="FileSystemId"/> instance.</returns>
    public static FileSystemId Create(Guid value)
    {
        return new FileSystemId(value);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    #endregion
}