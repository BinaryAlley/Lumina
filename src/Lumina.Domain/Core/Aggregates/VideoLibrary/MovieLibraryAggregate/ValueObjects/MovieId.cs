#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.VideoLibrary.MovieLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a movie.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class MovieId : EntityId<Guid>
{    
    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="MovieId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private MovieId(Guid value) : base(value)
    {
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="MovieId"/> class.
    /// </summary>
    /// <returns>The created <see cref="MovieId"/> instance.</returns>
    public static MovieId CreateUnique()
    {
        // TODO: enforce invariants
        return new MovieId(Guid.NewGuid()); 
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MovieId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="MovieId"/> instance.</param>
    /// <returns>The created <see cref="MovieId"/> instance.</returns>
    public static MovieId Create(Guid value)
    {
        // TODO: enforce invariants
        return new MovieId(value); 
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    #endregion
}