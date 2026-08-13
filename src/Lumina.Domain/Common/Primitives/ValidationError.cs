namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Error representing that the input or data failed validation rules.
/// </summary>
public record ValidationError(string Code, string Description) : Error
{
    public override string Code { get; } = Code;
    public override string Description { get; } = Description;
}
