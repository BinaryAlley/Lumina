namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Error representing that the operation is not allowed due to insufficient permissions or restrictions.
/// </summary>
public record ForbiddenError(string Code, string Description) : Error
{
    public override string Code { get; } = Code;
    public override string Description { get; } = Description;
}
