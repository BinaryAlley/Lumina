#region ========================================================================= USING =====================================================================================
#endregion

namespace Lumina.Application.Common.Infrastructure.Validation;

/// <summary>
/// Concrete <see cref="AbstractValidator{TRequest}"/> used to declare validation rules inline for nested complex properties.
/// </summary>
/// <typeparam name="TRequest">The type of the request (command or query) to be validated.</typeparam>
public class InlineValidator<TRequest> : AbstractValidator<TRequest>
{
}
