#region ========================================================================= USING =====================================================================================
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;
#endregion

namespace Lumina.DataAccess.Common.ValueGeneration;

/// <summary>
/// Generates time ordered version 7 GUIDs for identifier properties whose values are assigned by the persistence layer.
/// </summary>
public sealed class GuidV7ValueGenerator : ValueGenerator<Guid>
{
    /// <summary>
    /// Gets a value indicating whether the generated values are temporary.
    /// </summary>
    /// <remarks>The generated identifiers are stored permanently, so they are not temporary values.</remarks>
    public override bool GeneratesTemporaryValues => false;

    /// <summary>
    /// Generates the next time ordered identifier.
    /// </summary>
    /// <param name="entry">The tracked entity entry that owns the property.</param>
    /// <returns>A new version 7 GUID.</returns>
    public override Guid Next(EntityEntry entry)
    {
        return Guid.CreateVersion7();
    }
}
