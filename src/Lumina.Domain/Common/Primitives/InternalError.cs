namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Error representing that an unexpected internal error occurred.
/// </summary>
public record InternalError(string Code, string Description) : Error
{
    public override string Code { get; } = Code;
    public override string Description { get; } = Description;
}
