namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Error representing that the requested resource or entity was not found.
/// </summary>
public record NotFoundError(string Code, string Description) : Error
{
    public override string Code { get; } = Code;
    public override string Description { get; } = Description;
}
