namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Error representing that the requested functionality is not implemented.
/// </summary>
public record NotImplementedError(string Code, string Description) : Error
{
    public override string Code { get; } = Code;
    public override string Description { get; } = Description;
}
