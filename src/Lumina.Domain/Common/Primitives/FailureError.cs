namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Error representing that some operation failing.
/// </summary>
public record FailureError(string Code, string Description) : Error
{
    public override string Code { get; } = Code;
    public override string Description { get; } = Description;
}
