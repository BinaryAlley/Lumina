namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Error representing that the operation is invalid in the current state or context.
/// </summary>
public record InvalidOperationError(string Code, string Description) : Error
{
    public override string Code { get; } = Code;
    public override string Description { get; } = Description;
}
