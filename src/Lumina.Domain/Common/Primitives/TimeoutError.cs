namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Error representing that the operation timed out before completion.
/// </summary>
public record TimeoutError(string Code, string Description) : Error
{
    public override string Code { get; } = Code;
    public override string Description { get; } = Description;
}
