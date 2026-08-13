namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Error representing that the operation could not be completed due to a conflict with the current state.
/// </summary>
public record ConflictError(string Code, string Description) : Error
{
    public override string Code { get; } = Code;
    public override string Description { get; } = Description;
}
