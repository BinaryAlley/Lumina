#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Primitives;
#endregion

namespace Lumina.Presentation.Web.Common.Primitives;

/// <summary>
/// Error representing that an unexpected internal error occurred.
/// </summary>
/// <param name="Code">The error code that uniquely identifies this error type.</param>
/// <param name="Description">The human-readable description of the error.</param>
public record InternalError(string Code, string Description) : Error
{
    /// <summary>
    /// Gets the type of this error.
    /// </summary>
    public override ErrorType Type => ErrorType.Unexpected;

    /// <summary>
    /// Gets the error code that uniquely identifies this error type.
    /// </summary>
    public override string Code { get; } = Code;

    /// <summary>
    /// Gets the human-readable description of the error.
    /// </summary>
    public override string Description { get; } = Description;
}
