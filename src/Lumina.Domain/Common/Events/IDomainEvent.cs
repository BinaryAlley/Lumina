namespace Lumina.Domain.Common.Events;

/// <summary>
/// Interface defining a domain event.
/// </summary>
public interface IDomainEvent
{
    #region ==================================================================== PROPERTIES =================================================================================
    Guid Id { get; }
    DateTime OccurredOn { get; }
    #endregion
}