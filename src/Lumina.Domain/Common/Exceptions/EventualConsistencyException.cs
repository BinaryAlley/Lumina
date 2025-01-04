#region ========================================================================= USING =====================================================================================
using ErrorOr;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Common.Exceptions;

/// <summary>
/// Custom exception for domain aggregates eventual consistency.
/// </summary>
public class EventualConsistencyException : Exception
{
    /// <summary>
    /// Gets the primary error associated with the eventual consistency failure.
    /// </summary>
    public Error EventualConsistencyError { get; }

    /// <summary>
    /// Gets a list of underlying errors that contributed to the eventual consistency failure.
    /// </summary>
    public List<Error> UnderlyingErrors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventualConsistencyException"/> class.
    /// </summary>
    /// <param name="eventualConsistencyError">The primary error representing the eventual consistency issue.</param>
    /// <param name="underlyingErrors">A list of additional errors contributing to the issue, or <see langword="null"/> if no additional errors exist.</param>
    public EventualConsistencyException(Error eventualConsistencyError, List<Error>? underlyingErrors = null) : base(message: eventualConsistencyError.Description)
    {
        EventualConsistencyError = eventualConsistencyError;
        UnderlyingErrors = underlyingErrors ?? [];
    }
}
