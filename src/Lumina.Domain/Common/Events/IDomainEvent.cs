#region ========================================================================= USING =====================================================================================
using Mediator;
using System;
#endregion

namespace Lumina.Domain.Common.Events;

/// <summary>
/// Interface defining a domain event.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the Id of the domain event.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the date and time when the domain event occurred.
    /// </summary>
    DateTime OccurredOnUtc { get; }
}
