namespace Lumina.Application.Common.DataAccess.Entities.Common;

/// <summary>
/// Interface for a shared reference entity, whose identity is its normalized name and whose rows are shared across the whole storage medium instead of being owned by a single parent.
/// </summary>
public interface ISharedReferenceEntity
{
    /// <summary>
    /// Gets the name that identifies the shared reference, stored in lowercase and culture-invariant form.
    /// </summary>
    string? Name { get; }
}
