namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Error representing that authentication is required or has failed.
/// </summary>
public record UnauthorizedError(string Code, string Description) : Error
{
    public override string Code { get; } = Code;
    public override string Description { get; } = Description;
}
