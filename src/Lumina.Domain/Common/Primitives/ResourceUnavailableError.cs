namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Error representing that the required resource is currently unavailable.
/// </summary>
public record ResourceUnavailableError(string Code, string Description) : Error
{
    public override string Code { get; } = Code;
    public override string Description { get; } = Description;
}
